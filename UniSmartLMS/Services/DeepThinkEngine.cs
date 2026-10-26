using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Services.AI;

namespace UniSmartLMS.Services
{
    public interface IDeepThinkEngine
    {
        Task<DeepThoughtResponse> ThinkAsync(string query, string userId);
    }

    public class DeepThoughtResponse
    {
        public string FinalAnswer { get; set; }
        public string Thinking { get; set; }
        public List<Source> Sources { get; set; } = new List<Source>();
        public double Confidence { get; set; }
        public List<string> FollowUpQuestions { get; set; } = new List<string>();
    }

    public class DeepThinkEngine : IDeepThinkEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private double _currentConfidence;

        public DeepThinkEngine(ApplicationDbContext context, IEmbeddingService embeddingService)
        {
            _context = context;
            _embeddingService = embeddingService;
        }

        public async Task<DeepThoughtResponse> ThinkAsync(string query, string userId)
        {
            var response = new DeepThoughtResponse();
            var lowerQuery = query.ToLower().Trim();

            // ============================================================
            // HARDCODED GREETINGS (NO DOCUMENT SEARCH)
            // ============================================================

            if (lowerQuery == "hi" || lowerQuery == "hello" || lowerQuery == "hlo" || lowerQuery == "hey" || lowerQuery == "heloo" || lowerQuery == "hii")
            {
                response.FinalAnswer = "<div class='greeting-card'><span class='greeting-icon'>👋</span><span class='greeting-text'>Hello! I'm UniSmart AI. Ask me about Python, databases, web development, or type 'help' to see what I can do!</span></div>";
                response.Confidence = 1.0;
                return response;
            }

            if (lowerQuery.Contains("how are you") || lowerQuery == "how r u" || lowerQuery == "how are u")
            {
                response.FinalAnswer = "<div class='greeting-card'><span class='greeting-icon'>🤖</span><span class='greeting-text'>I'm doing great! Ready to help you find answers from your documents.</span></div>";
                response.Confidence = 1.0;
                return response;
            }

            if (lowerQuery == "help" || lowerQuery == "what can you do" || lowerQuery == "how can you help")
            {
                response.FinalAnswer = @"<div class='help-card'><div class='help-title'>📚 How I Can Help</div><div class='help-section'>• 'What is Python?' → Definitions<br>• 'Difference between SQL and NoSQL?' → Comparisons<br>• 'List programming languages' → Key points<br>• 'How to install Docker?' → Step-by-step guides</div><div class='help-section'>💬 Also: Hi, Hello, How are you, Thank you, Bye</div></div>";
                response.Confidence = 1.0;
                return response;
            }

            if (lowerQuery.Contains("thank"))
            {
                response.FinalAnswer = "<div class='greeting-card'><span class='greeting-icon'>😊</span><span class='greeting-text'>You're welcome! Happy to help.</span></div>";
                response.Confidence = 1.0;
                return response;
            }

            if (lowerQuery.Contains("bye") || lowerQuery.Contains("goodbye") || lowerQuery == "by")
            {
                response.FinalAnswer = "<div class='greeting-card'><span class='greeting-icon'>👋</span><span class='greeting-text'>Goodbye! Come back anytime.</span></div>";
                response.Confidence = 1.0;
                return response;
            }

            // ============================================================
            // CHECK IF DOCUMENTS EXIST WITH TIMEOUT HANDLING
            // ============================================================

            List<DocumentChunk> chunks;
            try
            {
                chunks = await _context.DocumentChunks
                    .Where(c => c.Document.IsActive && c.Embedding != null)
                    .Include(c => c.Document)
                    .Take(500)
                    .ToListAsync();
            }
            catch (Exception)
            {
                response.FinalAnswer = "<div class='timeout-card'><div class='timeout-icon'>⏱️</div><div class='timeout-text'>**Server is busy right now.** Please try again in a few moments.</div></div>";
                response.Confidence = 0;
                return response;
            }

            if (!chunks.Any())
            {
                response.FinalAnswer = @"<div class='warning-card'><div class='warning-icon'>📚</div><div class='warning-text'>No documents uploaded yet. Please ask your librarian to upload PDF, DOCX, or PPTX files.</div></div>";
                response.Confidence = 0;
                return response;
            }

            // ============================================================
            // STEP 1: ANALYZE QUESTION
            // ============================================================

            var analysis = AnalyzeDeeply(query);

            // ============================================================
            // STEP 2: GENERATE EMBEDDING AND SEARCH
            // ============================================================

            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(analysis.SearchQuery);
            var chunkScores = new List<DeepChunkScore>();

            foreach (var chunk in chunks)
            {
                var embedding = JsonConvert.DeserializeObject<float[]>(chunk.Embedding);
                double semanticScore = embedding != null ? _embeddingService.CosineSimilarity(queryEmbedding, embedding) : 0;

                double keywordScore = CalculateKeywordScore(analysis.Keywords, chunk.Content);
                double exactMatchScore = CalculateExactMatchScore(analysis.Topic, chunk.Content);
                double structureScore = CalculateStructureScore(chunk.ChunkIndex, chunk.Content);

                double totalScore = (semanticScore * 0.30) + (keywordScore * 0.35) + (exactMatchScore * 0.25) + (structureScore * 0.10);

                if (totalScore > 0.12)
                {
                    chunkScores.Add(new DeepChunkScore
                    {
                        Chunk = chunk,
                        TotalScore = totalScore,
                        Content = chunk.Content,
                        FileName = chunk.FileName
                    });
                }
            }

            var topChunks = chunkScores.OrderByDescending(x => x.TotalScore).Take(5).ToList();

            if (!topChunks.Any())
            {
                response.FinalAnswer = $@"<div class='warning-card'><div class='warning-icon'>🔍</div><div class='warning-text'>No information found about '{analysis.Topic}'. Try different keywords or upload more documents.</div></div>";
                response.Confidence = 0;
                return response;
            }

            // ============================================================
            // STEP 3: BUILD SOURCES
            // ============================================================

            foreach (var cs in topChunks)
            {
                response.Sources.Add(new Source
                {
                    FileName = cs.FileName,
                    Excerpt = cs.Content.Length > 200 ? cs.Content.Substring(0, 200) + "..." : cs.Content,
                    RelevanceScore = cs.TotalScore
                });
            }

            response.Confidence = topChunks.Average(c => c.TotalScore);
            _currentConfidence = response.Confidence;

            // ============================================================
            // STEP 4: BUILD ANSWER (WITH LLM ENHANCEMENT)
            // ============================================================

            //// Try LLM enhancement first (if Ollama is available)
            //if (_llmService != null && _llmService.IsAvailable && response.Sources.Any())
            //{
            //    var llmAnswer = await _llmService.PolishAnswerAsync(query, response.Sources);
            //    if (!string.IsNullOrEmpty(llmAnswer))
            //    {
            //        response.FinalAnswer = llmAnswer;
            //        response.FollowUpQuestions = GenerateFollowUps(analysis, topChunks);
            //        response.Thinking = "✨ Enhanced with DeepSeek-Coder LLM\n";
            //        return response;
            //    }
            //}

            // Fallback to your existing BuildCompactAnswer
            response.FinalAnswer = BuildCompactAnswer(analysis, topChunks);
            response.FollowUpQuestions = GenerateFollowUps(analysis, topChunks);
            response.Thinking = "📄 Answer generated from document search (LLM not available)\n";

            return response;
        }

        // ============================================================
        // QUESTION ANALYSIS
        // ============================================================

        private DeepAnalysis AnalyzeDeeply(string query)
        {
            var lower = query.ToLower().Trim();

            string intent = "general";
            if (Regex.IsMatch(lower, @"^(what|who|when|where|why|how) is|^define|^meaning of"))
                intent = "definition";
            else if (lower.Contains("difference between") || lower.Contains("compare"))
                intent = "comparison";
            else if (lower.Contains("list") || lower.Contains("types of"))
                intent = "list";
            else if (lower.Contains("how to") || lower.Contains("steps"))
                intent = "procedure";

            var topic = lower;
            var patterns = new[] { "what is ", "who is ", "define ", "meaning of ", "difference between ", "list " };
            foreach (var p in patterns)
            {
                if (topic.StartsWith(p))
                    topic = topic.Substring(p.Length);
            }
            topic = topic.Trim().TrimEnd('?');

            var stopWords = new HashSet<string> { "what", "is", "are", "the", "a", "an", "and", "or", "of", "to", "for", "in", "that", "this", "with", "by", "from" };
            var keywords = lower.Split(new[] { ' ', '?', '!', '.', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Distinct()
                .Take(5)
                .ToList();

            return new DeepAnalysis
            {
                Original = query,
                SearchQuery = lower,
                Intent = intent,
                Topic = topic,
                Keywords = keywords
            };
        }

        // ============================================================
        // SCORING FUNCTIONS
        // ============================================================

        private double CalculateKeywordScore(List<string> keywords, string content)
        {
            if (keywords.Count == 0) return 0;
            var lowerContent = content.ToLower();
            var matches = keywords.Count(kw => lowerContent.Contains(kw));
            return (double)matches / keywords.Count;
        }

        private double CalculateExactMatchScore(string topic, string content)
        {
            if (string.IsNullOrEmpty(topic)) return 0;
            var lowerContent = content.ToLower();
            if (lowerContent.Contains(topic.ToLower()))
                return 0.8;
            return 0;
        }

        private double CalculateStructureScore(int chunkIndex, string content)
        {
            double score = 0;
            var lower = content.ToLower();

            if (lower.Contains("definition") || lower.Contains("overview") || lower.Contains("introduction"))
                score += 0.3;
            if (chunkIndex < 3)
                score += 0.2;
            if (lower.Contains("is a") || lower.Contains("refers to"))
                score += 0.2;

            return Math.Min(score, 0.5);
        }

        // ============================================================
        // COMPACT ANSWER BUILDER
        // ============================================================

        private string BuildCompactAnswer(DeepAnalysis analysis, List<DeepChunkScore> topChunks)
        {
            var sb = new StringBuilder();
            var bestChunk = topChunks.First();

            var sentences = bestChunk.Content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 30 && s.Length < 400)
                .ToList();

            var bestSentence = sentences.FirstOrDefault(s =>
                s.ToLower().Contains(analysis.Topic.ToLower()) ||
                analysis.Keywords.Any(k => s.ToLower().Contains(k))) ?? sentences.FirstOrDefault() ?? bestChunk.Content;

            bestSentence = Regex.Replace(bestSentence, @"https?://\S+|openstax|copyright|creative commons", "", RegexOptions.IgnoreCase);
            if (bestSentence.Length > 350)
                bestSentence = bestSentence.Substring(0, 347) + "...";

            var icon = analysis.Intent == "definition" ? "📖" : analysis.Intent == "comparison" ? "🔄" : analysis.Intent == "list" ? "📋" : "💡";
            var title = analysis.Intent == "definition" ? "Definition" : analysis.Intent == "comparison" ? "Comparison" : analysis.Intent == "list" ? "Key Points" : "Answer";

            sb.AppendLine($"<div class='answer-card'><div class='answer-header'><span class='answer-icon'>{icon}</span><span class='answer-title'>{title}</span></div><div class='answer-body'>");
            sb.AppendLine($"<div class='answer-text'><strong>{analysis.Topic.ToUpper()}</strong> — {bestSentence}</div>");

            if (sentences.Count > 1)
            {
                sb.AppendLine("<div class='additional-points'><strong>📌 More:</strong><ul>");
                foreach (var sent in sentences.Skip(1).Take(2))
                {
                    var cleanSent = Regex.Replace(sent, @"https?://\S+|openstax|copyright", "", RegexOptions.IgnoreCase);
                    var shortSent = cleanSent.Length > 100 ? cleanSent.Substring(0, 97) + "..." : cleanSent;
                    sb.AppendLine($"<li>{shortSent}</li>");
                }
                sb.AppendLine("</ul></div>");
            }

            sb.AppendLine("</div>");
            sb.AppendLine("<div class='sources-compact'><strong>📚 Sources:</strong> ");
            sb.AppendLine(string.Join(" • ", topChunks.Take(3).Select(c => $"{c.FileName} ({(c.TotalScore * 100):F0}%)")));
            sb.AppendLine("</div>");

            var confidenceText = _currentConfidence > 0.7 ? "High" : _currentConfidence > 0.4 ? "Medium" : "Low";
            sb.AppendLine($"<div class='answer-footer'><span class='conf-{confidenceText.ToLower()}'>{confidenceText} Confidence</span><span>DeepThink™</span></div></div>");

            return sb.ToString();
        }

        private List<string> GenerateFollowUps(DeepAnalysis analysis, List<DeepChunkScore> topChunks)
        {
            var questions = new List<string>();
            var topic = analysis.Topic;

            if (string.IsNullOrEmpty(topic))
                topic = "this topic";

            if (analysis.Intent == "definition")
                questions.Add($"What are the main features of {topic}?");
            else if (analysis.Intent == "comparison")
                questions.Add($"Which one is better for beginners?");
            else
                questions.Add($"Tell me more about {topic}");

            questions.Add($"What are the key points about {topic}?");
            questions.Add($"Can you give examples of {topic}?");

            return questions.Distinct().Take(3).ToList();
        }

        // ============================================================
        // HELPER CLASSES
        // ============================================================

        private class DeepAnalysis
        {
            public string Original { get; set; }
            public string SearchQuery { get; set; }
            public string Intent { get; set; }
            public string Topic { get; set; }
            public List<string> Keywords { get; set; }
        }

        private class DeepChunkScore
        {
            public DocumentChunk Chunk { get; set; }
            public double TotalScore { get; set; }
            public string Content { get; set; }
            public string FileName { get; set; }
        }
    }
}
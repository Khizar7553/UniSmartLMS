using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using UniSmartLMS.Data;
using UniSmartLMS.Models;

namespace UniSmartLMS.Services
{
    public interface IIntelligentAnswerEngine
    {
        Task<AIResponse> GetAnswerAsync(string query, string userId);
    }

    public class AIResponse
    {
        public string Answer { get; set; }
        public List<Source> Sources { get; set; } = new List<Source>();
        public double Confidence { get; set; }
        public List<string> RelatedQuestions { get; set; } = new List<string>();
        public string Thinking { get; set; }
        public int TotalChunksFound { get; set; }
    }

    public class IntelligentAnswerEngine : IIntelligentAnswerEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;

        public IntelligentAnswerEngine(ApplicationDbContext context, IEmbeddingService embeddingService)
        {
            _context = context;
            _embeddingService = embeddingService;
        }

        public async Task<AIResponse> GetAnswerAsync(string query, string userId)
        {
            var response = new AIResponse();
            var thinking = new StringBuilder();

            thinking.AppendLine("🧠 **AI Thinking Process**\n");

            // ============================================================
            // STEP 1: Understand the question (Pure logic - no API)
            // ============================================================
            thinking.AppendLine("**1. Understanding your question...**");
            var processedQuery = ProcessQuery(query);
            thinking.AppendLine($"   • Original: \"{processedQuery.Original}\"");
            thinking.AppendLine($"   • Intent: {processedQuery.Intent}");
            thinking.AppendLine($"   • Topic: {processedQuery.Topic}");
            thinking.AppendLine($"   • Keywords: {string.Join(", ", processedQuery.Keywords.Take(5))}");

            // ============================================================
            // STEP 2: Check if this is a greeting or casual conversation
            // ============================================================
            var casualResponse = GetCasualResponse(processedQuery.Normalized);
            if (casualResponse != null)
            {
                response.Answer = casualResponse;
                response.Confidence = 1.0;
                response.Thinking = thinking.ToString();
                return response;
            }

            // ============================================================
            // STEP 3: Search documents using multiple strategies
            // ============================================================
            var chunks = await _context.DocumentChunks
                .Where(c => c.Document.IsActive && c.Embedding != null)
                .Include(c => c.Document)
                .ToListAsync();

            if (!chunks.Any())
            {
                response.Answer = GetNoDocumentsMessage();
                response.Thinking = thinking.ToString();
                return response;
            }

            thinking.AppendLine($"\n**2. Searching {chunks.Count} document chunks...**");

            // Generate query embedding once
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(processedQuery.Normalized);

            // Score each chunk
            var scoredChunks = new List<ChunkScore>();

            foreach (var chunk in chunks)
            {
                // Vector similarity (semantic meaning)
                var chunkEmbedding = JsonConvert.DeserializeObject<float[]>(chunk.Embedding);
                double vectorScore = chunkEmbedding != null
                    ? _embeddingService.CosineSimilarity(queryEmbedding, chunkEmbedding)
                    : 0;

                // BM25 (keyword relevance - industry standard)
                double bm25Score = CalculateBM25(processedQuery.Normalized, chunk.Content);

                // Exact keyword matching
                double keywordScore = CalculateKeywordMatch(processedQuery.Keywords, chunk.Content);

                // Semantic word overlap
                double semanticScore = CalculateSemanticOverlap(processedQuery.Normalized, chunk.Content);

                // Position bonus (earlier chunks often contain introductions/definitions)
                double positionBonus = 1.0 - ((double)chunk.ChunkIndex / chunks.Count) * 0.15;

                // Combined weighted score
                double totalScore = (vectorScore * 0.35) + (bm25Score * 0.30) +
                                    (keywordScore * 0.20) + (semanticScore * 0.10) + (positionBonus * 0.05);

                if (totalScore > 0.12)
                {
                    scoredChunks.Add(new ChunkScore
                    {
                        Chunk = chunk,
                        TotalScore = totalScore,
                        VectorScore = vectorScore,
                        BM25Score = bm25Score,
                        Content = chunk.Content,
                        FileName = chunk.FileName
                    });
                }
            }

            var topChunks = scoredChunks.OrderByDescending(x => x.TotalScore).Take(5).ToList();
            response.TotalChunksFound = scoredChunks.Count;
            response.Confidence = topChunks.Any() ? topChunks.First().TotalScore : 0;

            thinking.AppendLine($"   • Found {scoredChunks.Count} relevant matches");
            thinking.AppendLine($"   • Top {topChunks.Count} chunks selected (confidence: {response.Confidence:P0})");

            if (!topChunks.Any())
            {
                response.Answer = GetNoAnswerFoundMessage(processedQuery.Topic);
                response.Thinking = thinking.ToString();
                return response;
            }

            // ============================================================
            // STEP 4: Build sources list
            // ============================================================
            foreach (var cs in topChunks)
            {
                response.Sources.Add(new Source
                {
                    FileName = cs.FileName,
                    Excerpt = cs.Content.Length > 350 ? cs.Content.Substring(0, 350) + "..." : cs.Content,
                    RelevanceScore = cs.TotalScore
                });
            }

            // ============================================================
            // STEP 5: Generate intelligent answer (Pure C# logic)
            // ============================================================
            thinking.AppendLine("\n**3. Synthesizing answer from sources...**");

            response.Answer = SynthesizeAnswer(processedQuery, topChunks);

            // ============================================================
            // STEP 6: Generate related questions
            // ============================================================
            response.RelatedQuestions = GenerateRelatedQuestions(processedQuery, topChunks);

            thinking.AppendLine($"   • Answer synthesized");
            thinking.AppendLine($"   • Generated {response.RelatedQuestions.Count} related questions");

            response.Thinking = thinking.ToString();

            return response;
        }

        // ============================================================
        // QUERY PROCESSING (Pure logic)
        // ============================================================

        private ProcessedQuery ProcessQuery(string query)
        {
            var original = query;
            var normalized = query.ToLower().Trim();

            // Remove extra spaces and punctuation
            normalized = Regex.Replace(normalized, @"[^\w\s\?]", "");

            // Detect intent
            string intent = "general";
            if (Regex.IsMatch(normalized, @"^(what|who|when|where|why|how) is|^(define|explain|tell me about)"))
                intent = "definition";
            else if (Regex.IsMatch(normalized, @"^(how to|steps to|ways to|method to)"))
                intent = "procedural";
            else if (normalized.Contains("difference between") || normalized.Contains("compare"))
                intent = "comparison";
            else if (normalized.Contains("list") || normalized.Contains("types of") || normalized.Contains("kinds of"))
                intent = "list";
            else if (normalized.Contains("summarize") || normalized.Contains("summary"))
                intent = "summary";

            // Extract topic (remove question words)
            var topic = normalized;
            var removePatterns = new[] { "what is ", "who is ", "when is ", "where is ", "why is ",
                                         "how is ", "define ", "explain ", "tell me about ",
                                         "how to ", "what are ", "what does " };
            foreach (var pattern in removePatterns)
            {
                if (topic.StartsWith(pattern))
                    topic = topic.Substring(pattern.Length);
            }
            topic = topic.Trim().TrimEnd('?');

            // Extract keywords (remove stop words)
            var stopWords = new HashSet<string> { "what", "is", "are", "the", "a", "an", "and", "or", "of",
                                                   "to", "for", "in", "that", "this", "with", "by", "from",
                                                   "be", "was", "were", "have", "has", "had", "how", "why",
                                                   "when", "where", "which", "who", "whom", "does", "do", "did" };

            var keywords = normalized.Split(new[] { ' ', '?', '!', '.', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Distinct()
                .ToList();

            return new ProcessedQuery
            {
                Original = original,
                Normalized = normalized,
                Intent = intent,
                Topic = topic,
                Keywords = keywords
            };
        }

        // ============================================================
        // CASUAL RESPONSES (Pure logic - no API)
        // ============================================================

        private string GetCasualResponse(string normalized)
        {
            var responses = new Dictionary<string, string[]>
            {
                { "hi", new[] { "👋 Hello! How can I help you today?", "Hi there! What can I assist you with?" } },
                { "hello", new[] { "👋 Hello! What would you like to know?", "Hi! Ask me anything about your documents." } },
                { "hey", new[] { "Hey! How can I help?", "Hi there! What brings you here today?" } },
                { "how are you", new[] { "🤖 I'm doing great! Ready to help you find answers.", "I'm functioning perfectly! What can I do for you?" } },
                { "what is your name", new[] { "🤖 I'm UniSmart AI, your intelligent document assistant.", "I'm UniSmart AI - here to help you find answers from your documents." } },
                { "who are you", new[] { "🤖 I'm UniSmart AI, a self-contained answer engine. No external APIs - just pure intelligence!", "I'm your personal AI assistant, built to answer questions from your uploaded documents." } },
                { "thank", new[] { "🤖 You're very welcome! Happy to help 😊", "My pleasure! Anything else I can assist with?" } },
                { "bye", new[] { "👋 Goodbye! Come back anytime.", "See you later! Happy learning 📚" } },
                { "goodbye", new[] { "👋 Take care! Feel free to return with more questions.", "Goodbye! Have a great day!" } },
                { "help", new[] { GetHelpMessage() } },
                { "what can you do", new[] { GetCapabilitiesMessage() } }
            };

            foreach (var kvp in responses)
            {
                if (normalized.Contains(kvp.Key))
                {
                    var random = new Random();
                    return kvp.Value[random.Next(kvp.Value.Length)];
                }
            }

            return null;
        }

        private string GetHelpMessage()
        {
            return @"## 🤖 How I Can Help You

I'm a self-contained AI answer engine. Here's what I can do:

| Type | Examples |
|------|----------|
| **Definitions** | ""What is Python?"", ""Define machine learning"" |
| **Procedures** | ""How to install Node.js?"", ""Steps to create a database"" |
| **Comparisons** | ""Difference between SQL and NoSQL"" |
| **Lists** | ""Types of cloud computing"", ""List programming languages"" |
| **Summaries** | ""Summarize this document"" |

**Upload PDF, DOCX, or PPTX files** and I'll answer questions based on them!

💡 **Pro tip:** The more documents you upload, the better my answers become.";
        }

        private string GetCapabilitiesMessage()
        {
            return @"## 🚀 My Capabilities

I'm a **pure C# answer engine** - no external APIs, no OpenAI, no internet required!

### What I can do:
✅ Answer questions from uploaded PDF/DOCX/PPTX files  
✅ Understand intent (definition, procedure, comparison, list, summary)  
✅ Use hybrid search (vector + BM25 + keyword) for accuracy  
✅ Provide sources and confidence scores  
✅ Generate related questions automatically  
✅ Save chat history permanently  

### What makes me unique:
🔬 **100% self-contained** - All intelligence is in my code  
📚 **Document-first** - Answers come only from your uploaded content  
🎯 **Accurate citations** - Every answer includes the source file  
💡 **No training needed** - Just upload and ask!";
        }

        private string GetNoDocumentsMessage()
        {
            return @"## 📚 No Documents Uploaded Yet

I can't answer questions until documents are uploaded to my knowledge base.

### For Librarians/Admins:
1. Go to **Document Upload** page
2. Upload PDF, DOCX, or PPTX files
3. I'll automatically index and learn from them

### For Users:
Ask your librarian to upload relevant documents. Once uploaded, I can answer:
- Definitions and explanations
- Step-by-step procedures
- Comparisons between concepts
- Lists and summaries

**I'm ready when you are!** 🚀";
        }

        private string GetNoAnswerFoundMessage(string topic)
        {
            return $@"## 🔍 No Answer Found

I couldn't find information about '{topic}' in the uploaded documents.

### 💡 Suggestions:
• Try using different keywords
• Check spelling
• Ask a broader question first
• Upload more relevant documents

### 📝 Try these instead:
• ""What is this document about?""
• ""Summarize the main points""
• ""List key topics covered""
• ""Explain [specific term from document]""

---
**Need help?** Type ""help"" to see what I can do!";
        }

        // ============================================================
        // SCORING ALGORITHMS (Pure math - no API)
        // ============================================================

        private double CalculateBM25(string query, string content)
        {
            var queryTerms = query.Split(' ');
            var contentLower = content.ToLower();
            var contentLength = content.Length;
            var avgContentLength = 500.0;

            double score = 0;
            foreach (var term in queryTerms)
            {
                if (term.Length < 3) continue;
                var termFreq = Regex.Matches(contentLower, Regex.Escape(term)).Count;
                if (termFreq == 0) continue;

                var k1 = 1.2;
                var b = 0.75;
                var idf = Math.Log((1000000 - termFreq + 0.5) / (termFreq + 0.5));
                var tf = (termFreq * (k1 + 1)) / (termFreq + k1 * (1 - b + b * contentLength / avgContentLength));
                score += idf * tf;
            }
            return Math.Min(score, 1.0);
        }

        private double CalculateKeywordMatch(List<string> keywords, string content)
        {
            if (keywords.Count == 0) return 0;
            var contentLower = content.ToLower();
            var matches = keywords.Count(kw => contentLower.Contains(kw));
            return (double)matches / keywords.Count;
        }

        private double CalculateSemanticOverlap(string query, string content)
        {
            var queryWords = query.Split(' ');
            var contentWords = content.ToLower().Split(' ');

            var common = queryWords.Intersect(contentWords).Count();
            var total = queryWords.Union(contentWords).Count();

            return total > 0 ? (double)common / total : 0;
        }

        // ============================================================
        // ANSWER SYNTHESIS (The "magic" - pure logic)
        // ============================================================

        private string SynthesizeAnswer(ProcessedQuery query, List<ChunkScore> topChunks)
        {
            var sb = new StringBuilder();
            var bestChunk = topChunks.First();

            // Extract all text from top chunks
            var allText = string.Join(" ", topChunks.Select(c => c.Content));
            var sentences = ExtractSentences(allText);

            // Find sentences relevant to the query
            var relevantSentences = ScoreSentences(sentences, query.Keywords, query.Topic);

            // Build answer based on intent
            switch (query.Intent)
            {
                case "definition":
                    sb.AppendLine(BuildDefinitionAnswer(relevantSentences, bestChunk, query.Topic));
                    break;
                case "procedural":
                    sb.AppendLine(BuildProceduralAnswer(relevantSentences));
                    break;
                case "comparison":
                    sb.AppendLine(BuildComparisonAnswer(relevantSentences));
                    break;
                case "list":
                    sb.AppendLine(BuildListAnswer(relevantSentences));
                    break;
                case "summary":
                    sb.AppendLine(BuildSummaryAnswer(relevantSentences, bestChunk));
                    break;
                default:
                    sb.AppendLine(BuildGeneralAnswer(relevantSentences, bestChunk, query.Keywords));
                    break;
            }

            // Add sources section
            sb.AppendLine(BuildSourcesSection(topChunks));

            // Add confidence footer
            var avgScore = topChunks.Average(c => c.TotalScore);
            sb.AppendLine(BuildFooter(avgScore, topChunks.Count));

            return sb.ToString();
        }

        private List<string> ExtractSentences(string text)
        {
            return text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 30 && s.Length < 500)
                .ToList();
        }

        private List<ScoredSentence> ScoreSentences(List<string> sentences, List<string> keywords, string topic)
        {
            var scored = new List<ScoredSentence>();

            foreach (var sentence in sentences)
            {
                var lowerSentence = sentence.ToLower();
                double score = 0;

                // Keyword matches
                score += keywords.Count(kw => lowerSentence.Contains(kw)) * 0.5;

                // Topic match (higher weight)
                if (lowerSentence.Contains(topic.ToLower()))
                    score += 1.0;

                // Length bonus (not too short, not too long)
                if (sentence.Length > 50 && sentence.Length < 300)
                    score += 0.3;

                // Penalize sentences with "copyright", "openstax", "access for free"
                if (lowerSentence.Contains("copyright") || lowerSentence.Contains("openstax") ||
                    lowerSentence.Contains("access for free") || lowerSentence.Contains("creative commons"))
                    score -= 0.5;

                if (score > 0)
                {
                    scored.Add(new ScoredSentence { Sentence = sentence, Score = score });
                }
            }

            return scored.OrderByDescending(s => s.Score).Take(5).ToList();
        }

        private string BuildDefinitionAnswer(List<ScoredSentence> sentences, ChunkScore bestChunk, string topic)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='ai-answer-card definition-card'>
    <div class='card-header'>
        <span class='card-icon'>📖</span>
        <span class='card-title'>Definition</span>
    </div>
    <div class='card-body'>");

            var definition = sentences.FirstOrDefault();
            if (definition != null)
            {
                sb.AppendLine($"<p class='definition-text'><strong>{topic.ToUpper()}</strong> — {definition.Sentence}</p>");
            }
            else
            {
                var excerpt = bestChunk.Content.Length > 400 ? bestChunk.Content.Substring(0, 400) + "..." : bestChunk.Content;
                sb.AppendLine($"<p>{excerpt}</p>");
            }

            // Add additional points
            if (sentences.Count > 1)
            {
                sb.AppendLine("<div class='additional-info'><strong>📌 Additional Information:</strong><ul>");
                foreach (var sent in sentences.Skip(1).Take(2))
                {
                    var shortSent = sent.Sentence.Length > 150 ? sent.Sentence.Substring(0, 147) + "..." : sent.Sentence;
                    sb.AppendLine($"<li>{shortSent}</li>");
                }
                sb.AppendLine("</ul></div>");
            }

            sb.AppendLine("</div></div>");
            return sb.ToString();
        }

        private string BuildProceduralAnswer(List<ScoredSentence> sentences)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='ai-answer-card procedural-card'>
    <div class='card-header'>
        <span class='card-icon'>🔧</span>
        <span class='card-title'>Step-by-Step Guide</span>
    </div>
    <div class='card-body'>");

            var stepSentences = sentences.Where(s =>
                s.Sentence.ToLower().Contains("step") ||
                s.Sentence.ToLower().Contains("first") ||
                s.Sentence.ToLower().Contains("then") ||
                s.Sentence.ToLower().Contains("next") ||
                s.Sentence.ToLower().Contains("finally")).ToList();

            if (stepSentences.Any())
            {
                sb.AppendLine("<ol class='step-list'>");
                for (int i = 0; i < Math.Min(stepSentences.Count, 6); i++)
                {
                    sb.AppendLine($"<li>{stepSentences[i].Sentence}</li>");
                }
                sb.AppendLine("</ol>");
            }
            else if (sentences.Any())
            {
                sb.AppendLine("<ul class='instruction-list'>");
                foreach (var sent in sentences.Take(5))
                {
                    sb.AppendLine($"<li>{sent.Sentence}</li>");
                }
                sb.AppendLine("</ul>");
            }

            sb.AppendLine("</div></div>");
            return sb.ToString();
        }

        private string BuildComparisonAnswer(List<ScoredSentence> sentences)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='ai-answer-card comparison-card'>
    <div class='card-header'>
        <span class='card-icon'>🔄</span>
        <span class='card-title'>Comparison</span>
    </div>
    <div class='card-body'>
        <table class='comparison-table'>
            <thead>
                <tr><th>Aspect</th><th>Details</th></tr>
            </thead>
            <tbody>");

            for (int i = 0; i < Math.Min(sentences.Count, 4); i++)
            {
                sb.AppendLine($@"
                <tr>
                    <td class='aspect-col'><strong>Point {i + 1}</strong></td>
                    <td>{sentences[i].Sentence}</td>
                </tr>");
            }

            sb.AppendLine(@"</tbody>
        </table>
    </div>
</div>");
            return sb.ToString();
        }

        private string BuildListAnswer(List<ScoredSentence> sentences)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='ai-answer-card list-card'>
    <div class='card-header'>
        <span class='card-icon'>📋</span>
        <span class='card-title'>Key Points</span>
    </div>
    <div class='card-body'>
        <ul class='key-points-list'>");

            foreach (var sent in sentences.Take(7))
            {
                var cleanSent = sent.Sentence.Length > 120 ? sent.Sentence.Substring(0, 117) + "..." : sent.Sentence;
                sb.AppendLine($"<li><i class='fas fa-check-circle'></i> {cleanSent}</li>");
            }

            sb.AppendLine(@"
        </ul>
    </div>
</div>");
            return sb.ToString();
        }

        private string BuildSummaryAnswer(List<ScoredSentence> sentences, ChunkScore bestChunk)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='ai-answer-card summary-card'>
    <div class='card-header'>
        <span class='card-icon'>📝</span>
        <span class='card-title'>Summary</span>
    </div>
    <div class='card-body'>");

            if (sentences.Any())
            {
                foreach (var sent in sentences.Take(3))
                {
                    sb.AppendLine($"<p>• {sent.Sentence}</p>");
                }
            }
            else
            {
                var summary = bestChunk.Content.Length > 500 ? bestChunk.Content.Substring(0, 500) + "..." : bestChunk.Content;
                sb.AppendLine($"<p>{summary}</p>");
            }

            sb.AppendLine("</div></div>");
            return sb.ToString();
        }

        private string BuildGeneralAnswer(List<ScoredSentence> sentences, ChunkScore bestChunk, List<string> keywords)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='ai-answer-card general-card'>
    <div class='card-header'>
        <span class='card-icon'>🎯</span>
        <span class='card-title'>Answer</span>
    </div>
    <div class='card-body'>");

            var bestMatch = sentences.OrderByDescending(s => s.Score).FirstOrDefault();

            if (bestMatch != null)
            {
                var highlighted = HighlightKeywords(bestMatch.Sentence, keywords);
                sb.AppendLine($"<div class='main-answer'>{highlighted}</div>");
            }
            else
            {
                var excerpt = bestChunk.Content.Length > 500 ? bestChunk.Content.Substring(0, 500) + "..." : bestChunk.Content;
                sb.AppendLine($"<div class='main-answer'>{excerpt}</div>");
            }

            // Add key takeaways
            if (sentences.Count > 1)
            {
                sb.AppendLine("<div class='key-takeaways'><strong>💡 Key Takeaways:</strong><ul>");
                foreach (var sent in sentences.Skip(1).Take(2))
                {
                    var shortSent = sent.Sentence.Length > 100 ? sent.Sentence.Substring(0, 97) + "..." : sent.Sentence;
                    sb.AppendLine($"<li>{shortSent}</li>");
                }
                sb.AppendLine("</ul></div>");
            }

            sb.AppendLine("</div></div>");
            return sb.ToString();
        }

        private string HighlightKeywords(string text, List<string> keywords)
        {
            var result = text;
            foreach (var kw in keywords.Where(k => k.Length > 2))
            {
                result = Regex.Replace(result, $@"\b{Regex.Escape(kw)}\b",
                    $"<mark class='keyword-highlight'>{kw}</mark>",
                    RegexOptions.IgnoreCase);
            }
            return result;
        }

        private string BuildSourcesSection(List<ChunkScore> topChunks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"
<div class='sources-section'>
    <div class='sources-header'>
        <i class='fas fa-link'></i> Sources
    </div>
    <div class='sources-list'>");

            var grouped = topChunks.GroupBy(c => c.FileName).Take(3);
            foreach (var group in grouped)
            {
                var avgScore = group.Average(c => c.TotalScore);
                var icon = avgScore > 0.7 ? "🟢" : avgScore > 0.4 ? "🟡" : "🔴";
                sb.AppendLine($@"
        <div class='source-item'>
            <span class='source-icon'>{icon}</span>
            <span class='source-name'>{group.Key}</span>
            <span class='source-confidence'>{avgScore * 100:F0}% relevant</span>
        </div>");
            }

            sb.AppendLine(@"
    </div>
</div>");
            return sb.ToString();
        }

        private string BuildFooter(double confidence, int sourceCount)
        {
            var confidenceText = confidence > 0.7 ? "High" : confidence > 0.4 ? "Medium" : "Low";
            var confidenceClass = confidenceText.ToLower();

            return $@"
<div class='answer-footer'>
    <span class='confidence-badge confidence-{confidenceClass}'>
        <i class='fas fa-chart-line'></i> {confidenceText} Confidence
    </span>
    <span class='source-count'>
        <i class='fas fa-file-alt'></i> Based on {sourceCount} document sections
    </span>
    <span class='ai-badge'>
        <i class='fas fa-microchip'></i> Pure C# AI
    </span>
</div>";
        }

        private List<string> GenerateRelatedQuestions(ProcessedQuery query, List<ChunkScore> topChunks)
        {
            var questions = new List<string>();
            var topic = query.Topic;

            switch (query.Intent)
            {
                case "definition":
                    questions.Add($"What else should I know about {topic}?");
                    questions.Add($"Can you explain {topic} in more detail?");
                    break;
                case "procedural":
                    questions.Add($"What are the prerequisites for {topic}?");
                    questions.Add($"Are there any alternatives to {topic}?");
                    break;
                case "comparison":
                    questions.Add($"Which one is better for beginners?");
                    questions.Add($"What are the advantages of each?");
                    break;
                default:
                    questions.Add($"Tell me more about {topic}");
                    questions.Add($"What are the key points?");
                    questions.Add($"Summarize this information");
                    break;
            }

            // Extract potential questions from content
            foreach (var chunk in topChunks.Take(2))
            {
                var sentences = ExtractSentences(chunk.Content);
                var questionSentence = sentences.FirstOrDefault(s =>
                    s.Contains("?") ||
                    (s.Length > 30 && (s.Contains("why") || s.Contains("how") || s.Contains("what"))));

                if (questionSentence != null && questionSentence.Length < 100 && !questions.Contains(questionSentence))
                {
                    questions.Add(questionSentence.TrimEnd('.') + (questionSentence.EndsWith("?") ? "" : "?"));
                }
            }

            return questions.Distinct().Take(4).ToList();
        }

        // ============================================================
        // HELPER CLASSES
        // ============================================================

        private class ProcessedQuery
        {
            public string Original { get; set; }
            public string Normalized { get; set; }
            public string Intent { get; set; }
            public string Topic { get; set; }
            public List<string> Keywords { get; set; }
        }

        private class ChunkScore
        {
            public DocumentChunk Chunk { get; set; }
            public double TotalScore { get; set; }
            public double VectorScore { get; set; }
            public double BM25Score { get; set; }
            public string Content { get; set; }
            public string FileName { get; set; }
        }

        private class ScoredSentence
        {
            public string Sentence { get; set; }
            public double Score { get; set; }
        }
    }
}
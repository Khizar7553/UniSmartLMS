using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System.Text;
using UniSmartLMS.Data;
using UniSmartLMS.Models;

namespace UniSmartLMS.Services
{
    public interface IOllamaAIService
    {
        Task<string> AskQuestionAsync(string question, string userId);
        Task<bool> IsOllamaRunningAsync();
    }

    public class OllamaAIService : IOllamaAIService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly OllamaApiClient _ollamaClient;

        public OllamaAIService(ApplicationDbContext context, IEmbeddingService embeddingService)
        {
            _context = context;
            _embeddingService = embeddingService;
            _ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "deepseek-coder:6.7b");
        }

        public async Task<bool> IsOllamaRunningAsync()
        {
            try
            {
                var models = await _ollamaClient.ListLocalModelsAsync();
                return models != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> AskQuestionAsync(string question, string userId)
        {
            var relevantChunks = await FindRelevantChunks(question);

            if (!relevantChunks.Any())
            {
                return GetNoDocumentsMessage();
            }

            if (!await IsOllamaRunningAsync())
            {
                return GetOllamaNotRunningMessage(relevantChunks);
            }

            var prompt = BuildVerifiedPrompt(question, relevantChunks);

            try
            {
                // FIXED: Use Chat class (OllamaSharp's recommended approach)
                var chat = new Chat(_ollamaClient);
                var fullResponse = new StringBuilder();

                await foreach (var answerToken in chat.SendAsync(prompt))
                {
                    fullResponse.Append(answerToken);
                }

                return FormatResponse(fullResponse.ToString(), relevantChunks);
            }
            catch (Exception ex)
            {
                return GetFallbackResponse(relevantChunks, ex.Message);
            }
        }

        private async Task<List<RelevantChunk>> FindRelevantChunks(string question)
        {
            var chunks = await _context.DocumentChunks
                .Where(c => c.Document.IsActive)
                .Include(c => c.Document)
                .Take(200)
                .ToListAsync();

            if (!chunks.Any()) return new List<RelevantChunk>();

            var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(question.ToLower());

            var scored = new List<RelevantChunk>();
            foreach (var chunk in chunks)
            {
                var chunkEmbedding = JsonConvert.DeserializeObject<float[]>(chunk.Embedding);
                if (chunkEmbedding == null) continue;

                var similarity = _embeddingService.CosineSimilarity(questionEmbedding, chunkEmbedding);
                if (similarity > 0.15f)
                {
                    scored.Add(new RelevantChunk
                    {
                        Content = chunk.Content,
                        FileName = chunk.FileName,
                        PageNumber = chunk.PageNumber ?? 1,
                        Similarity = similarity
                    });
                }
            }

            return scored.OrderByDescending(x => x.Similarity).Take(5).ToList();
        }

        private string BuildVerifiedPrompt(string question, List<RelevantChunk> chunks)
        {
            return $@"You are UniSmart Verified AI. IMPORTANT RULES:

1. ONLY answer using the documents below
2. If answer NOT in documents, say ""I cannot find this information in the library documents""
3. ALWAYS cite the source [Filename, Page X] after each fact
4. Do NOT add information from outside these documents

USER QUESTION: {question}

DOCUMENTS (Verified Library PDFs):
{string.Join("\n\n---\n", chunks.Select(c => $"[SOURCE: {c.FileName}, Page {c.PageNumber}]\n{c.Content}"))}

VERIFIED ANSWER (with citations):";

        }

        private string FormatResponse(string aiResponse, List<RelevantChunk> chunks)
        {
            return $@"<div class='verified-answer'>
    <div class='answer-header'>
        <i class='fas fa-check-circle'></i> Verified Answer (Ollama + DeepSeek)
    </div>
    <div class='answer-body'>
        {aiResponse}
    </div>
    <div class='sources-footer'>
        <strong>📚 Verified Sources:</strong><br>
        {string.Join("<br>", chunks.Select(c => $"📄 {c.FileName} (Page {c.PageNumber}) - {c.Similarity * 100:F0}% relevant"))}
    </div>
    <div class='trust-badge'>
        ✅ Runs locally on university server | No data leaves campus
    </div>
</div>";
        }

        private string GetNoDocumentsMessage()
        {
            return @"<div class='warning-card'>
    <i class='fas fa-book'></i>
    <h3>No Documents Uploaded</h3>
    <p>Please ask your librarian to upload PDF documents to the Smart Library first.</p>
</div>";
        }

        private string GetOllamaNotRunningMessage(List<RelevantChunk> chunks)
        {
            // FIXED: Parentheses around ternary operator
            var fallbackAnswer = string.Join("\n\n", chunks.Take(3).Select(c =>
                $"From {c.FileName} (Page {c.PageNumber}):\n{(c.Content.Length > 300 ? c.Content.Substring(0, 300) + "..." : c.Content)}"));

            return $@"<div class='warning-card'>
    <i class='fas fa-exclamation-triangle'></i>
    <h3>Ollama Not Running (Fallback Mode)</h3>
    <p><strong>To enable AI:</strong> Run 'ollama serve' in terminal, then 'ollama pull deepseek-coder:6.7b'</p>
    <hr>
    <strong>📄 Most relevant excerpts from your documents:</strong><br>
    {fallbackAnswer}
</div>";
        }

        private string GetFallbackResponse(List<RelevantChunk> chunks, string error)
        {
            return $@"<div class='warning-card'>
    <i class='fas fa-plug'></i>
    <h3>Ollama Connection Issue</h3>
    <p>Error: {error}</p>
    <p><strong>Fix:</strong> Open terminal and run:</p>
    <pre>ollama serve
ollama pull deepseek-coder:6.7b</pre>
    <hr>
    <strong>📄 Documents found (no AI processing):</strong><br>
    {string.Join("<br>", chunks.Select(c => $"📄 {c.FileName} (Page {c.PageNumber})"))}
</div>";
        }

        private class RelevantChunk
        {
            public string Content { get; set; }
            public string FileName { get; set; }
            public int PageNumber { get; set; }
            public float Similarity { get; set; }
        }
    }
}
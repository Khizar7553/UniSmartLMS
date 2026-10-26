using System.Text.Json;
using System.Text.RegularExpressions;

namespace UniSmartLMS.Services
{
    public interface IGeneralQAService
    {
        Task<string> GetResponseAsync(string query);
        bool IsGeneralQuestion(string query);
    }

    public class GeneralQAService : IGeneralQAService
    {
        private readonly Dictionary<string, List<QAPattern>> _patterns;

        public GeneralQAService()
        {
            _patterns = LoadPatterns();
        }

        private Dictionary<string, List<QAPattern>> LoadPatterns()
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "GeneralQA.json");
            var json = File.ReadAllText(jsonPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, List<QAPattern>>>(json);
            return data ?? new Dictionary<string, List<QAPattern>>();
        }

        public bool IsGeneralQuestion(string query)
        {
            var lowerQuery = query.ToLower().Trim();

            foreach (var category in _patterns.Values)
            {
                foreach (var pattern in category)
                {
                    foreach (var keyword in pattern.Patterns)
                    {
                        if (lowerQuery.Contains(keyword.ToLower()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public async Task<string> GetResponseAsync(string query)
        {
            await Task.Delay(1);

            var lowerQuery = query.ToLower().Trim();

            // Check each category
            foreach (var category in _patterns)
            {
                foreach (var pattern in category.Value)
                {
                    foreach (var keyword in pattern.Patterns)
                    {
                        if (lowerQuery.Contains(keyword.ToLower()))
                        {
                            var random = new Random();
                            var response = pattern.Responses[random.Next(pattern.Responses.Count)];
                            return response;
                        }
                    }
                }
            }

            // Default fallback
            return "I'm here to help with your library questions! You can ask me about uploaded documents, get definitions, find key points, or ask for summaries. What would you like to know?";
        }

        private class QAPattern
        {
            public List<string> Patterns { get; set; } = new List<string>();
            public List<string> Responses { get; set; } = new List<string>();
        }
    }
}
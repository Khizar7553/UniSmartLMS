using FuzzySharp;
using System.Text.RegularExpressions;

namespace UniSmartLMS.Services
{
    public interface IQueryUnderstandingService
    {
        Task<ProcessedQuery> ProcessQueryAsync(string query);
        string CorrectSpelling(string query);
        bool IsLibraryQuery(string query);
        string ExtractMainTopic(string query);
    }

    public class ProcessedQuery
    {
        public string OriginalQuery { get; set; }
        public string CorrectedQuery { get; set; }
        public string Intent { get; set; }
        public string MainTopic { get; set; }
        public bool IsLibraryRelated { get; set; }
        public double Confidence { get; set; }
    }

    public class QueryUnderstandingService : IQueryUnderstandingService
    {
        private readonly Dictionary<string, string> _commonMisspellings;
        private readonly List<string> _libraryKeywords;
        private readonly List<string> _intentPatterns;

        public QueryUnderstandingService()
        {
            // Common misspellings mapping
            _commonMisspellings = new Dictionary<string, string>
            {
                { "serch", "search" }, { "seach", "search" }, { "srch", "search" },
                { "bok", "book" }, { "buk", "book" }, { "boook", "book" },
                { "borow", "borrow" }, { "borrrow", "borrow" }, { "barrow", "borrow" },
                { "retrn", "return" }, { "retun", "return" }, { "riturn", "return" },
                { "libary", "library" }, { "librery", "library" }, { "librry", "library" },
                { "studnt", "student" }, { "stdnt", "student" },
                { "faclty", "faculty" }, { "facluty", "faculty" },
                { "fine", "fine" }, { "fines", "fine" },
                { "loan", "loan" }, { "loans", "loan" },
                { "meaning og", "meaning of" }, { "meaning of", "meaning of" },
                { "defination", "definition" }, { "defination of", "definition of" },
                { "wat is", "what is" }, { "wht is", "what is" }, { "wats", "what is" },
                { "ho to", "how to" }, { "hov to", "how to" },
                { "pls", "please" }, { "plz", "please" }
            };

            _libraryKeywords = new List<string>
            { "book", "borrow", "return", "library", "fine", "loan", "member",
              "catalog", "search", "find", "read", "issue", "renew", "due",
              "overdue", "penalty", "card", "registration" };

            _intentPatterns = new List<string>
            { "what is", "how to", "define", "explain", "tell me about",
              "meaning of", "definition of", "where is", "when", "why", "can you" };
        }

        public async Task<ProcessedQuery> ProcessQueryAsync(string query)
        {
            await Task.Delay(1);
            var processed = new ProcessedQuery
            {
                OriginalQuery = query,
                CorrectedQuery = CorrectSpelling(query),
                IsLibraryRelated = IsLibraryQuery(query)
            };

            processed.Intent = DetectIntent(processed.CorrectedQuery);
            processed.MainTopic = ExtractMainTopic(processed.CorrectedQuery);
            processed.Confidence = CalculateConfidence(query, processed.CorrectedQuery);

            return processed;
        }

        public string CorrectSpelling(string query)
        {
            var corrected = query.ToLower();

            // Fix common misspellings
            foreach (var misspelling in _commonMisspellings)
            {
                corrected = corrected.Replace(misspelling.Key, misspelling.Value);
            }

            // Use FuzzySharp for partial matches
            var words = corrected.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                foreach (var correct in _commonMisspellings.Values.Distinct())
                {
                    var score = Fuzz.PartialRatio(words[i], correct);
                    if (score > 80 && words[i] != correct)
                    {
                        words[i] = correct;
                        break;
                    }
                }
            }

            return string.Join(" ", words);
        }

        public bool IsLibraryQuery(string query)
        {
            var lowerQuery = query.ToLower();
            return _libraryKeywords.Any(kw => lowerQuery.Contains(kw)) ||
                   _intentPatterns.Any(p => lowerQuery.Contains(p));
        }

        public string ExtractMainTopic(string query)
        {
            var lowerQuery = query.ToLower();

            // Remove intent patterns
            foreach (var pattern in _intentPatterns)
            {
                if (lowerQuery.Contains(pattern))
                {
                    var topic = lowerQuery.Replace(pattern, "").Trim();
                    if (!string.IsNullOrEmpty(topic))
                        return topic;
                }
            }

            // Remove question words
            var removeWords = new[] { "what", "is", "are", "the", "a", "an", "and", "of", "to", "for", "in", "on", "at" };
            var words = lowerQuery.Split(' ');
            var topicWords = words.Where(w => !removeWords.Contains(w) && w.Length > 2).ToList();

            return topicWords.Any() ? string.Join(" ", topicWords.Take(3)) : lowerQuery;
        }

        private string DetectIntent(string query)
        {
            var lowerQuery = query.ToLower();

            if (lowerQuery.Contains("what is") || lowerQuery.Contains("define") || lowerQuery.Contains("meaning of"))
                return "definition";
            if (lowerQuery.Contains("how to") || lowerQuery.Contains("how do i") || lowerQuery.Contains("steps to"))
                return "procedural";
            if (lowerQuery.Contains("compare") || lowerQuery.Contains("difference between"))
                return "comparison";
            if (lowerQuery.Contains("list") || lowerQuery.Contains("what are") || lowerQuery.Contains("types of"))
                return "list";
            if (lowerQuery.Contains("summarize") || lowerQuery.Contains("summary"))
                return "summary";
            if (lowerQuery.Contains("search") || lowerQuery.Contains("find"))
                return "search";
            if (lowerQuery.Contains("borrow") || lowerQuery.Contains("issue"))
                return "borrow";
            if (lowerQuery.Contains("return"))
                return "return";
            if (lowerQuery.Contains("fine") || lowerQuery.Contains("penalty"))
                return "fine";

            return "general";
        }

        private double CalculateConfidence(string original, string corrected)
        {
            if (original == corrected) return 1.0;
            var score = Fuzz.Ratio(original.ToLower(), corrected.ToLower());
            return score / 100.0;
        }
    }
}
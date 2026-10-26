namespace UniSmartLMS.Models
{
    public class SearchResult
    {
        public string Answer { get; set; }
        public List<Source> Sources { get; set; } = new List<Source>();
        public double Confidence { get; set; }
        public int TotalChunksFound { get; set; }
        public List<string> KeyPoints { get; set; } = new List<string>();
        public string Summary { get; set; }
    }

    public class Source
    {
        public string FileName { get; set; }
        public int PageNumber { get; set; }
        public string Excerpt { get; set; }
        public double RelevanceScore { get; set; }
    }
}
using Microsoft.AspNetCore.Http;

namespace UniSmartLMS.Services
{
    public interface IResearchService
    {
        Task<ResearchAnalysis> AnalyzeFileAsync(IFormFile file);
        //Task<string> SummarizeFileAsync(IFormFile file);

       //// Task<string> SummarizePdfAsync(IFormFile pdfFile);
        string GenerateCitation(string title, string author, int year, string citationStyle = "APA");
     //   Task<string> ExtractKeywordsAsync(string text);
        bool IsSupportedFileType(string fileName);

    }
}
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using System.Text;
using UglyToad.PdfPig;
using Xceed.Words.NET;

namespace UniSmartLMS.Services
{
    public class ResearchAnalysis
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string MainObjective { get; set; }
        public string Methodology { get; set; }
        public List<string> KeyFindings { get; set; } = new List<string>();
        public string Conclusion { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        public int WordCount { get; set; }
        public int PageCount { get; set; }
        public string Summary { get; set; }
    }

    //public interface IResearchService
    //{
    //    Task<ResearchAnalysis> AnalyzeFileAsync(IFormFile file);
    //    string GenerateCitation(string title, string author, int year, string citationStyle = "APA");
    //    bool IsSupportedFileType(string fileName);
    //}

    public class ResearchService : IResearchService
    {
        public bool IsSupportedFileType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext == ".pdf" || ext == ".doc" || ext == ".docx" || ext == ".ppt" || ext == ".pptx" || ext == ".txt";
        }

        public async Task<ResearchAnalysis> AnalyzeFileAsync(IFormFile file)
        {
            var analysis = new ResearchAnalysis
            {
                FileName = file.FileName,
                FileType = Path.GetExtension(file.FileName).ToUpper(),
                KeyFindings = new List<string>(),
                Keywords = new List<string>()
            };

            if (file == null || file.Length == 0)
            {
                analysis.MainObjective = "No file provided.";
                return analysis;
            }

            try
            {
                var extractedText = await ExtractTextFromFileAsync(file);

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    analysis.MainObjective = "Could not extract text from file. File may be image-based or corrupted.";
                    return analysis;
                }

                analysis.WordCount = extractedText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                analysis.PageCount = await GetPageCountAsync(file);

                var structuredAnalysis = GenerateStructuredAnalysis(extractedText);

                analysis.MainObjective = structuredAnalysis.MainObjective;
                analysis.Methodology = structuredAnalysis.Methodology;
                analysis.KeyFindings = structuredAnalysis.KeyFindings;
                analysis.Conclusion = structuredAnalysis.Conclusion;
                analysis.Keywords = structuredAnalysis.Keywords;
                analysis.Summary = structuredAnalysis.Summary;

                return analysis;
            }
            catch (Exception ex)
            {
                analysis.MainObjective = $"Error: {ex.Message}";
                return analysis;
            }
        }

        private async Task<string> ExtractTextFromFileAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();

            return extension switch
            {
                ".pdf" => ExtractTextFromPdf(stream),
                ".docx" => ExtractTextFromDocx(stream),
                ".doc" => ExtractTextFromDoc(stream),
                ".pptx" => ExtractTextFromPptx(stream),
                ".ppt" => ExtractTextFromPpt(stream),
                ".txt" => await ExtractTextFromTxt(stream),
                _ => "Unsupported file format."
            };
        }

        private string ExtractTextFromPdf(Stream stream)
        {
            var text = new StringBuilder();
            using (var pdf = PdfDocument.Open(stream))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.Append(page.Text);
                }
            }
            return text.ToString();
        }

        private string ExtractTextFromDocx(Stream stream)
        {
            var text = new StringBuilder();
            using (var wordDoc = WordprocessingDocument.Open(stream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                foreach (var para in body.Elements<Paragraph>())
                {
                    text.Append(para.InnerText);
                    text.Append(' ');
                }
            }
            return text.ToString();
        }

        private string ExtractTextFromDoc(Stream stream)
        {
            var tempPath = Path.GetTempFileName() + ".doc";
            using (var fileStream = System.IO.File.Create(tempPath))
            {
                stream.CopyTo(fileStream);
            }

            var text = new StringBuilder();
            using (var doc = DocX.Load(tempPath))
            {
                text.Append(doc.Text);
            }

            try { System.IO.File.Delete(tempPath); } catch { }
            return text.ToString();
        }

        private string ExtractTextFromPptx(Stream stream)
        {
            var text = new StringBuilder();
            using (var ppt = PresentationDocument.Open(stream, false))
            {
                var presentationPart = ppt.PresentationPart;
                if (presentationPart != null)
                {
                    foreach (var slidePart in presentationPart.SlideParts)
                    {
                        var slideText = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                            .Select(t => t.Text)
                            .Where(t => !string.IsNullOrWhiteSpace(t));

                        foreach (var t in slideText)
                        {
                            text.Append(t);
                            text.Append(' ');
                        }
                    }
                }
            }
            return text.ToString();
        }

        private string ExtractTextFromPpt(Stream stream)
        {
            return "Legacy PPT format. Please convert to PPTX or PDF for better text extraction.";
        }

        private async Task<string> ExtractTextFromTxt(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private async Task<int> GetPageCountAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf") return 1;

            using var stream = file.OpenReadStream();
            using (var pdf = PdfDocument.Open(stream))
            {
                return await Task.FromResult(pdf.NumberOfPages);
            }
        }

        private StructuredResult GenerateStructuredAnalysis(string text)
        {
            var result = new StructuredResult();

            var objective = ExtractSection(text, new[] { "abstract", "objective", "aim", "purpose", "introduction" }, 200);
            result.MainObjective = string.IsNullOrEmpty(objective) ? ExtractFirstSignificantSentence(text, 150) : objective;

            result.Methodology = ExtractSection(text, new[] { "methodology", "methods", "approach", "experimental" }, 150);
            if (string.IsNullOrEmpty(result.Methodology))
                result.Methodology = "Systematic analysis using established research methods.";

            var findingsText = ExtractSection(text, new[] { "findings", "results", "discussion" }, 500);
            result.KeyFindings = ExtractKeyFindings(findingsText, text);

            result.Conclusion = ExtractSection(text, new[] { "conclusion", "summary" }, 150);
            if (string.IsNullOrEmpty(result.Conclusion))
                result.Conclusion = ExtractLastSignificantSentence(text, 150);

            result.Keywords = ExtractKeywords(text);
            result.Summary = GenerateSummary(result);

            return result;
        }

        private string ExtractSection(string text, string[] sectionNames, int maxLength)
        {
            var lowerText = text.ToLower();
            foreach (var section in sectionNames)
            {
                var index = lowerText.IndexOf(section);
                if (index >= 0)
                {
                    var start = index + section.Length;
                    var end = start + maxLength;
                    if (end > text.Length) end = text.Length;

                    var extracted = text.Substring(start, end - start);
                    var sentences = extracted.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

                    if (sentences.Length > 0)
                    {
                        var result = sentences[0].Trim();
                        if (result.Length > 20 && result.Length < 500)
                            return result + ".";
                    }
                }
            }
            return null;
        }

        private string ExtractFirstSignificantSentence(string text, int maxLength)
        {
            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var sentence in sentences)
            {
                var clean = sentence.Trim();
                if (clean.Length > 40 && clean.Length < maxLength)
                    return clean + ".";
            }
            return "This research presents important findings in its field.";
        }

        private string ExtractLastSignificantSentence(string text, int maxLength)
        {
            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = sentences.Length - 1; i >= 0; i--)
            {
                var clean = sentences[i].Trim();
                if (clean.Length > 40 && clean.Length < maxLength)
                    return clean + ".";
            }
            return "The research contributes valuable insights to the academic community.";
        }

        private List<string> ExtractKeyFindings(string findingsText, string fullText)
        {
            var findings = new List<string>();
            var lines = (findingsText + "\n" + fullText.Substring(0, Math.Min(fullText.Length, 3000))).Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 20 && trimmed.Length < 200)
                {
                    if (trimmed.StartsWith("•") || trimmed.StartsWith("-") || trimmed.StartsWith("*"))
                    {
                        var clean = trimmed.TrimStart('•', '-', '*', ' ', '\t');
                        if (clean.Length > 15 && !findings.Contains(clean))
                            findings.Add(clean);
                    }
                }
            }

            if (findings.Count == 0)
            {
                findings.Add("The research demonstrates significant contributions to the field.");
                findings.Add("Data analysis reveals important patterns and correlations.");
            }

            return findings.Take(5).ToList();
        }

        private List<string> ExtractKeywords(string text)
        {
            var keywords = new List<string>();
            var academicTerms = new[] { "analysis", "research", "study", "data", "method", "approach", "framework",
                "model", "system", "algorithm", "evaluation", "results", "methodology", "implementation", "design" };

            foreach (var term in academicTerms)
            {
                if (text.ToLower().Contains(term) && !keywords.Contains(term))
                {
                    keywords.Add(term);
                    if (keywords.Count >= 7) break;
                }
            }

            if (keywords.Count < 3)
                keywords.AddRange(new[] { "research", "analysis", "findings" });

            return keywords.Take(7).ToList();
        }

        private string GenerateSummary(StructuredResult result)
        {
            var summary = result.MainObjective;
            if (summary.Length > 100)
                summary = summary.Substring(0, 100);

            if (result.KeyFindings.Count > 0)
                summary += " Key findings: " + string.Join(", ", result.KeyFindings.Take(2)) + ".";

            summary += " " + result.Conclusion;

            if (summary.Length > 350)
                summary = summary.Substring(0, 347) + "...";

            return summary;
        }

        public string GenerateCitation(string title, string author, int year, string citationStyle = "APA")
        {
            if (citationStyle.ToUpper() == "APA")
                return $"{author} ({year}). {title}. University Library.";
            else if (citationStyle.ToUpper() == "MLA")
                return $"{author}. \"{title}.\" {year}.";
            else
                return $"{author}, {year}. {title}. Available at: University Library.";
        }

        private class StructuredResult
        {
            public string MainObjective { get; set; }
            public string Methodology { get; set; }
            public List<string> KeyFindings { get; set; } = new List<string>();
            public string Conclusion { get; set; }
            public List<string> Keywords { get; set; } = new List<string>();
            public string Summary { get; set; }
        }
    }
}
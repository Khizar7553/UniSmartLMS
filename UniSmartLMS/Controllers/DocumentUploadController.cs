using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Services;
using UglyToad.PdfPig;
using Xceed.Words.NET;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace UniSmartLMS.Controllers
{
    [Authorize(Roles = "Admin,Librarian")]
    public class DocumentUploadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<AppUser> _userManager;

        public DocumentUploadController(
            ApplicationDbContext context,
            IEmbeddingService embeddingService,
            IWebHostEnvironment environment,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _embeddingService = embeddingService;
            _environment = environment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
            ViewBag.Documents = documents;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Index));
            }

            var supportedExtensions = new[] { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".txt" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!supportedExtensions.Contains(extension))
            {
                TempData["Error"] = $"Unsupported format. Supported: PDF, DOCX, DOC, PPTX, PPT, TXT";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id ?? "1";

            // Save file
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "library");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = new Models.Document
            {
                FileName = file.FileName,
                FilePath = filePath,
                FileSize = (int)new FileInfo(filePath).Length,
                UploadedBy = userId,
                UploadedAt = DateTime.Now,
                IsActive = true
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Extract text
            var extractedText = await ExtractTextFromFileAsync(filePath, file.FileName);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                TempData["Error"] = "No text could be extracted from the file.";
                return RedirectToAction(nameof(Index));
            }

            // Split into chunks
            var textChunks = SplitTextIntoChunks(extractedText, 500, 50);
            var chunks = new List<DocumentChunk>();
            int chunkIndex = 0;

            foreach (var chunkText in textChunks)
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunkText);
                chunks.Add(new DocumentChunk
                {
                    DocumentId = document.Id,
                    FileName = file.FileName,
                    ChunkIndex = chunkIndex++,
                    Content = chunkText,
                    Embedding = JsonConvert.SerializeObject(embedding),
                    CreatedAt = DateTime.Now
                });
            }

            _context.DocumentChunks.AddRange(chunks);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Document '{file.FileName}' uploaded and indexed successfully! ({chunks.Count} chunks)";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                TempData["Error"] = "Document not found.";
                return RedirectToAction(nameof(Index));
            }

            var chunks = await _context.DocumentChunks.Where(c => c.DocumentId == id).ToListAsync();
            _context.DocumentChunks.RemoveRange(chunks);
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            if (System.IO.File.Exists(document.FilePath))
                System.IO.File.Delete(document.FilePath);

            TempData["Success"] = "Document deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // TEXT EXTRACTION METHODS
        // ============================================================

        private async Task<string> ExtractTextFromFileAsync(string filePath, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".pdf":
                    return ExtractTextFromPdf(filePath);
                case ".docx":
                    return ExtractTextFromDocx(filePath);
                case ".doc":
                    return ExtractTextFromDoc(filePath);
                case ".pptx":
                    return ExtractTextFromPptx(filePath);
                case ".txt":
                    return await System.IO.File.ReadAllTextAsync(filePath);
                default:
                    return string.Empty;
            }
        }

        private string ExtractTextFromPdf(string filePath)
        {
            var text = new StringBuilder();
            using (var pdf = PdfDocument.Open(filePath))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.Append(page.Text);
                }
            }
            return text.ToString();
        }

        private string ExtractTextFromDocx(string filePath)
        {
            var text = new StringBuilder();
            using (var wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var body = wordDoc.MainDocumentPart?.Document.Body;
                if (body != null)
                {
                    foreach (var para in body.Elements<Paragraph>())
                    {
                        text.Append(para.InnerText);
                        text.Append(' ');
                    }
                }
            }
            return text.ToString();
        }

        private string ExtractTextFromDoc(string filePath)
        {
            using var doc = DocX.Load(filePath);
            return doc.Text;
        }

        private string ExtractTextFromPptx(string filePath)
        {
            var text = new StringBuilder();
            using (var ppt = PresentationDocument.Open(filePath, false))
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
            return text.ToString().Trim();
        }

        private List<string> SplitTextIntoChunks(string text, int chunkSize, int overlap)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += chunkSize - overlap)
            {
                var chunk = text.Substring(i, Math.Min(chunkSize, text.Length - i));
                if (chunk.Length > 50)
                    chunks.Add(chunk);
            }
            return chunks;
        }
    }
}
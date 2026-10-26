using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Services;

namespace UniSmartLMS.Controllers
{
    [Authorize]
    public class ResearchController : Controller
    {
        private readonly IResearchService _researchService;

        public ResearchController(IResearchService researchService)
        {
            _researchService = researchService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(IFormFile file, string citationStyle = "APA")
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Index));
            }

            if (!_researchService.IsSupportedFileType(file.FileName))
            {
                TempData["Error"] = "Unsupported file type. Please upload PDF, DOC, DOCX, PPT, PPTX, or TXT.";
                return RedirectToAction(nameof(Index));
            }

            if (file.Length > 15 * 1024 * 1024)
            {
                TempData["Error"] = "File size exceeds 15MB limit.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var analysis = await _researchService.AnalyzeFileAsync(file);
                var title = Path.GetFileNameWithoutExtension(file.FileName);
                var author = User.Identity?.Name ?? "Anonymous";
                var year = DateTime.Now.Year;

                ViewBag.Analysis = analysis;
                ViewBag.Title = title;
                ViewBag.Author = author;
                ViewBag.Year = year;
                ViewBag.CitationAPA = _researchService.GenerateCitation(title, author, year, "APA");
                ViewBag.CitationMLA = _researchService.GenerateCitation(title, author, year, "MLA");
                ViewBag.CitationHarvard = _researchService.GenerateCitation(title, author, year, "Harvard");

                TempData["Success"] = "File analyzed successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing file: {ex.Message}";
            }

            return View("Result");
        }
    }
}
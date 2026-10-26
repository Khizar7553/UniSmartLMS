using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionC_Catalog
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LanguageController : Controller
    {
        private readonly IGenericRepository<Language> _repository;

        public LanguageController(IGenericRepository<Language> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var languages = await _repository.GetAllAsync();
            return View(languages);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _repository.SearchAsync("LanguageCategory", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var language = await _repository.GetByIdAsync(id);
            if (language == null) return NotFound();
            return View(language);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Language language)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(language);
                TempData["Success"] = "Language created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var language = await _repository.GetByIdAsync(id);
            if (language == null) return NotFound();
            return View(language);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Language language)
        {
            if (id != language.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(language);
                TempData["Success"] = "Language updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var language = await _repository.GetByIdAsync(id);
            if (language == null) return NotFound();
            return View(language);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Language deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
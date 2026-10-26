using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class FeatureController : Controller
    {
        private readonly IGenericRepository<Feature> _repository;

        public FeatureController(IGenericRepository<Feature> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var features = await _repository.GetAllAsync();
            return View(features);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _repository.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var feature = await _repository.GetByIdAsync(id);
            if (feature == null) return NotFound();
            return View(feature);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feature feature)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(feature);
                TempData["Success"] = "Feature created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(feature);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var feature = await _repository.GetByIdAsync(id);
            if (feature == null) return NotFound();
            return View(feature);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Feature feature)
        {
            if (id != feature.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(feature);
                TempData["Success"] = "Feature updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(feature);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var feature = await _repository.GetByIdAsync(id);
            if (feature == null) return NotFound();
            return View(feature);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Feature deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionF_Fines
{
    [Authorize(Roles = "Admin,Librarian")]
    public class FineCategoryController : Controller
    {
        private readonly IGenericRepository<FineCategory> _repository;

        public FineCategoryController(IGenericRepository<FineCategory> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _repository.GetAllAsync();
            return View(categories);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _repository.SearchAsync("FineCategoryName", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FineCategory category)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(category);
                TempData["Success"] = "Fine Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FineCategory category)
        {
            if (id != category.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(category);
                TempData["Success"] = "Fine Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Fine Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

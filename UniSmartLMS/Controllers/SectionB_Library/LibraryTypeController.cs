using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionB_Library
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LibraryTypeController : Controller
    {
        private readonly IGenericRepository<LibraryType> _repository;

        public LibraryTypeController(IGenericRepository<LibraryType> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var types = await _repository.GetAllAsync();
            return View(types);
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
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LibraryType type)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(type);
                TempData["Success"] = "Library Type created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(type);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LibraryType type)
        {
            if (id != type.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(type);
                TempData["Success"] = "Library Type updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(type);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Library Type deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
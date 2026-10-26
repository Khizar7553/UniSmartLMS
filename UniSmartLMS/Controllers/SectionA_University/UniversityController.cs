using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class UniversityController : Controller
    {
        private readonly IGenericRepository<University> _repository;

        public UniversityController(IGenericRepository<University> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var universities = await _repository.GetAllAsync();
            return View(universities);
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
            var university = await _repository.GetByIdAsync(id);
            if (university == null) return NotFound();
            return View(university);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(University university)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(university);
                TempData["Success"] = "University created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(university);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var university = await _repository.GetByIdAsync(id);
            if (university == null) return NotFound();
            return View(university);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, University university)
        {
            if (id != university.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(university);
                TempData["Success"] = "University updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(university);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var university = await _repository.GetByIdAsync(id);
            if (university == null) return NotFound();
            return View(university);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "University deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
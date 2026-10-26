using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class InstituteController : Controller
    {
        private readonly IGenericRepository<Institute> _repository;

        public InstituteController(IGenericRepository<Institute> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var institutes = await _repository.GetAllAsync();
            return View(institutes);
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
            var institute = await _repository.GetByIdAsync(id);
            if (institute == null) return NotFound();
            return View(institute);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Institute institute)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(institute);
                TempData["Success"] = "Institute created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(institute);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var institute = await _repository.GetByIdAsync(id);
            if (institute == null) return NotFound();
            return View(institute);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Institute institute)
        {
            if (id != institute.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(institute);
                TempData["Success"] = "Institute updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(institute);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var institute = await _repository.GetByIdAsync(id);
            if (institute == null) return NotFound();
            return View(institute);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Institute deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
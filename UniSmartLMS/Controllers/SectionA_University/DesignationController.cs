using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class DesignationController : Controller
    {
        private readonly IGenericRepository<Designation> _repository;

        public DesignationController(IGenericRepository<Designation> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var designations = await _repository.GetAllAsync();
            return View(designations);
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
            var designation = await _repository.GetByIdAsync(id);
            if (designation == null) return NotFound();
            return View(designation);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Designation designation)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(designation);
                TempData["Success"] = "Designation created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(designation);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var designation = await _repository.GetByIdAsync(id);
            if (designation == null) return NotFound();
            return View(designation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Designation designation)
        {
            if (id != designation.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(designation);
                TempData["Success"] = "Designation updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(designation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var designation = await _repository.GetByIdAsync(id);
            if (designation == null) return NotFound();
            return View(designation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Designation deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionC_Catalog
{
    [Authorize(Roles = "Admin,Librarian")]
    public class SubjectHeadingController : Controller
    {
        private readonly IGenericRepository<SubjectHeading> _repository;

        public SubjectHeadingController(IGenericRepository<SubjectHeading> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var subjects = await _repository.GetAllAsync();
            return View(subjects);
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
            var subject = await _repository.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectHeading subject)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(subject);
                TempData["Success"] = "Subject created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _repository.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectHeading subject)
        {
            if (id != subject.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(subject);
                TempData["Success"] = "Subject updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _repository.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Subject deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
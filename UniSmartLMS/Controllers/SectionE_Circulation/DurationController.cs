using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian")]
    public class DurationController : Controller
    {
        private readonly IGenericRepository<Duration> _repository;

        public DurationController(IGenericRepository<Duration> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var durations = await _repository.GetAllAsync();
            return View(durations);
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
            var duration = await _repository.GetByIdAsync(id);
            if (duration == null) return NotFound();
            return View(duration);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Duration duration)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(duration);
                TempData["Success"] = "Duration created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(duration);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var duration = await _repository.GetByIdAsync(id);
            if (duration == null) return NotFound();
            return View(duration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Duration duration)
        {
            if (id != duration.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(duration);
                TempData["Success"] = "Duration updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(duration);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var duration = await _repository.GetByIdAsync(id);
            if (duration == null) return NotFound();
            return View(duration);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Duration deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
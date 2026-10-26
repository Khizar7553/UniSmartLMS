using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class PositionController : Controller
    {
        private readonly IGenericRepository<Position> _repository;

        public PositionController(IGenericRepository<Position> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var positions = await _repository.GetAllAsync();
            return View(positions);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _repository.SearchAsync("Category", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var position = await _repository.GetByIdAsync(id);
            if (position == null) return NotFound();
            return View(position);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Position position)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(position);
                TempData["Success"] = "Position created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(position);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var position = await _repository.GetByIdAsync(id);
            if (position == null) return NotFound();
            return View(position);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Position position)
        {
            if (id != position.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(position);
                TempData["Success"] = "Position updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(position);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var position = await _repository.GetByIdAsync(id);
            if (position == null) return NotFound();
            return View(position);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Position deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

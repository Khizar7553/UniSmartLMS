using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class RoomTypeController : Controller
    {
        private readonly IGenericRepository<RoomType> _repository;

        public RoomTypeController(IGenericRepository<RoomType> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var roomTypes = await _repository.GetAllAsync();
            return View(roomTypes);
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
            var roomType = await _repository.GetByIdAsync(id);
            if (roomType == null) return NotFound();
            return View(roomType);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(roomType);
                TempData["Success"] = "Room Type created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var roomType = await _repository.GetByIdAsync(id);
            if (roomType == null) return NotFound();
            return View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomType roomType)
        {
            if (id != roomType.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(roomType);
                TempData["Success"] = "Room Type updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var roomType = await _repository.GetByIdAsync(id);
            if (roomType == null) return NotFound();
            return View(roomType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Room Type deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
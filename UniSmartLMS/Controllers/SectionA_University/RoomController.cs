using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class RoomController : Controller
    {
        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IGenericRepository<Floor> _floorRepo;

        public RoomController(
            IGenericRepository<Room> roomRepo,
            IGenericRepository<Floor> floorRepo)
        {
            _roomRepo = roomRepo;
            _floorRepo = floorRepo;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomRepo.GetAllAsync();
            foreach (var room in rooms)
            {
                room.Floor = await _floorRepo.GetByIdAsync(room.FloorId);
            }
            return View(rooms);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _roomRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null) return NotFound();
            room.Floor = await _floorRepo.GetByIdAsync(room.FloorId);

            return View(room);
        }

        public async Task<IActionResult> Create()
        {
            await LoadFloorDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            ModelState.Remove("Floor");

            if (ModelState.IsValid)
            {
                await _roomRepo.AddAsync(room);
                TempData["Success"] = "Room created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadFloorDropdown(room.FloorId);
            return View(room);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null) return NotFound();
            await LoadFloorDropdown(room.FloorId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            ModelState.Remove("Floor");

            if (ModelState.IsValid)
            {
                await _roomRepo.UpdateAsync(room);
                TempData["Success"] = "Room updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadFloorDropdown(room.FloorId);
            return View(room);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null) return NotFound();
            room.Floor = await _floorRepo.GetByIdAsync(room.FloorId);

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roomRepo.DeleteAsync(id);
            TempData["Success"] = "Room deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadFloorDropdown(object selectedFloor = null)
        {
            var floors = await _floorRepo.GetAllAsync();
            ViewBag.FloorList = new SelectList(floors, "Id", "Name", selectedFloor);
        }
    }
}
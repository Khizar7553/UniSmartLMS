using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class FloorController : Controller
    {
        private readonly IGenericRepository<Floor> _floorRepo;
        private readonly IGenericRepository<Building> _buildingRepo;

        public FloorController(
            IGenericRepository<Floor> floorRepo,
            IGenericRepository<Building> buildingRepo)
        {
            _floorRepo = floorRepo;
            _buildingRepo = buildingRepo;
        }

        public async Task<IActionResult> Index()
        {
            var floors = await _floorRepo.GetAllAsync();
            foreach (var floor in floors)
            {
                floor.Building = await _buildingRepo.GetByIdAsync(floor.BuildingId);
            }
            return View(floors);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _floorRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var floor = await _floorRepo.GetByIdAsync(id);
            if (floor == null) return NotFound();
            floor.Building = await _buildingRepo.GetByIdAsync(floor.BuildingId);

            return View(floor);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBuildingDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Floor floor)
        {
            ModelState.Remove("Building");

            if (ModelState.IsValid)
            {
                await _floorRepo.AddAsync(floor);
                TempData["Success"] = "Floor created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBuildingDropdown(floor.BuildingId);
            return View(floor);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var floor = await _floorRepo.GetByIdAsync(id);
            if (floor == null) return NotFound();
            await LoadBuildingDropdown(floor.BuildingId);
            return View(floor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Floor floor)
        {
            if (id != floor.Id) return NotFound();

            ModelState.Remove("Building");

            if (ModelState.IsValid)
            {
                await _floorRepo.UpdateAsync(floor);
                TempData["Success"] = "Floor updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBuildingDropdown(floor.BuildingId);
            return View(floor);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var floor = await _floorRepo.GetByIdAsync(id);
            if (floor == null) return NotFound();
            floor.Building = await _buildingRepo.GetByIdAsync(floor.BuildingId);

            return View(floor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _floorRepo.DeleteAsync(id);
            TempData["Success"] = "Floor deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBuildingDropdown(object selectedBuilding = null)
        {
            var buildings = await _buildingRepo.GetAllAsync();
            ViewBag.BuildingList = new SelectList(buildings, "Id", "Name", selectedBuilding);
        }
    }
}
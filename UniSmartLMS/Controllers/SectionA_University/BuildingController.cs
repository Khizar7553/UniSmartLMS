using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class BuildingController : Controller
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly IGenericRepository<Block> _blockRepo;

        public BuildingController(
            IGenericRepository<Building> buildingRepo,
            IGenericRepository<Block> blockRepo)
        {
            _buildingRepo = buildingRepo;
            _blockRepo = blockRepo;
        }

        public async Task<IActionResult> Index()
        {
            var buildings = await _buildingRepo.GetAllAsync();
            foreach (var building in buildings)
            {
                building.Block = await _blockRepo.GetByIdAsync(building.BlockId);
            }
            return View(buildings);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _buildingRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var building = await _buildingRepo.GetByIdAsync(id);
            if (building == null) return NotFound();
            building.Block = await _blockRepo.GetByIdAsync(building.BlockId);

            return View(building);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBlockDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Building building)
        {
            ModelState.Remove("Block");

            if (ModelState.IsValid)
            {
                await _buildingRepo.AddAsync(building);
                TempData["Success"] = "Building created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBlockDropdown(building.BlockId);
            return View(building);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var building = await _buildingRepo.GetByIdAsync(id);
            if (building == null) return NotFound();
            await LoadBlockDropdown(building.BlockId);
            return View(building);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Building building)
        {
            if (id != building.Id) return NotFound();

            ModelState.Remove("Block");

            if (ModelState.IsValid)
            {
                await _buildingRepo.UpdateAsync(building);
                TempData["Success"] = "Building updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBlockDropdown(building.BlockId);
            return View(building);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var building = await _buildingRepo.GetByIdAsync(id);
            if (building == null) return NotFound();
            building.Block = await _blockRepo.GetByIdAsync(building.BlockId);

            return View(building);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _buildingRepo.DeleteAsync(id);
            TempData["Success"] = "Building deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBlockDropdown(object selectedBlock = null)
        {
            var blocks = await _blockRepo.GetAllAsync();
            ViewBag.BlockList = new SelectList(blocks, "Id", "Name", selectedBlock);
        }
    }
}
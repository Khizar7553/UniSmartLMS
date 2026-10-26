using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class BlockController : Controller
    {
        private readonly IGenericRepository<Block> _blockRepo;
        private readonly IGenericRepository<Campus> _campusRepo;

        public BlockController(
            IGenericRepository<Block> blockRepo,
            IGenericRepository<Campus> campusRepo)
        {
            _blockRepo = blockRepo;
            _campusRepo = campusRepo;
        }

        public async Task<IActionResult> Index()
        {
            var blocks = await _blockRepo.GetAllAsync();
            foreach (var block in blocks)
            {
                block.Campus = await _campusRepo.GetByIdAsync(block.CampusId);
            }
            return View(blocks);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _blockRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var block = await _blockRepo.GetByIdAsync(id);
            if (block == null) return NotFound();
            block.Campus = await _campusRepo.GetByIdAsync(block.CampusId);

            return View(block);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCampusDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Block block)
        {
            ModelState.Remove("Campus");

            if (ModelState.IsValid)
            {
                await _blockRepo.AddAsync(block);
                TempData["Success"] = "Block created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadCampusDropdown(block.CampusId);
            return View(block);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var block = await _blockRepo.GetByIdAsync(id);
            if (block == null) return NotFound();
            await LoadCampusDropdown(block.CampusId);
            return View(block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Block block)
        {
            if (id != block.Id) return NotFound();

            ModelState.Remove("Campus");

            if (ModelState.IsValid)
            {
                await _blockRepo.UpdateAsync(block);
                TempData["Success"] = "Block updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadCampusDropdown(block.CampusId);
            return View(block);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var block = await _blockRepo.GetByIdAsync(id);
            if (block == null) return NotFound();
            block.Campus = await _campusRepo.GetByIdAsync(block.CampusId);

            return View(block);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _blockRepo.DeleteAsync(id);
            TempData["Success"] = "Block deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCampusDropdown(object selectedCampus = null)
        {
            var campuses = await _campusRepo.GetAllAsync();
            ViewBag.CampusList = new SelectList(campuses, "Id", "Name", selectedCampus);
        }
    }
}
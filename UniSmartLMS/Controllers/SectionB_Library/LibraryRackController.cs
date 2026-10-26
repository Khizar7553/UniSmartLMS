using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionB_Library
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LibraryRackController : Controller
    {
        private readonly IGenericRepository<LibraryRack> _rackRepo;
        private readonly IGenericRepository<LibraryItemSection> _sectionRepo;

        public LibraryRackController(
            IGenericRepository<LibraryRack> rackRepo,
            IGenericRepository<LibraryItemSection> sectionRepo)
        {
            _rackRepo = rackRepo;
            _sectionRepo = sectionRepo;
        }

        public async Task<IActionResult> Index()
        {
            var racks = await _rackRepo.GetAllAsync();
            foreach (var rack in racks)
            {
                rack.LibraryItemSection = await _sectionRepo.GetByIdAsync(rack.LibraryItemSectionId);
            }
            return View(racks);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _rackRepo.SearchAsync("Code", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var rack = await _rackRepo.GetByIdAsync(id);
            if (rack == null) return NotFound();
            rack.LibraryItemSection = await _sectionRepo.GetByIdAsync(rack.LibraryItemSectionId);
            return View(rack);
        }

        public async Task<IActionResult> Create()
        {
            await LoadSectionDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LibraryRack rack)
        {
            ModelState.Remove("LibraryItemSection");

            if (ModelState.IsValid)
            {
                await _rackRepo.AddAsync(rack);
                TempData["Success"] = "Library Rack created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadSectionDropdown(rack.LibraryItemSectionId);
            return View(rack);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var rack = await _rackRepo.GetByIdAsync(id);
            if (rack == null) return NotFound();
            await LoadSectionDropdown(rack.LibraryItemSectionId);
            return View(rack);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LibraryRack rack)
        {
            if (id != rack.Id) return NotFound();

            ModelState.Remove("LibraryItemSection");

            if (ModelState.IsValid)
            {
                await _rackRepo.UpdateAsync(rack);
                TempData["Success"] = "Library Rack updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadSectionDropdown(rack.LibraryItemSectionId);
            return View(rack);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var rack = await _rackRepo.GetByIdAsync(id);
            if (rack == null) return NotFound();
            rack.LibraryItemSection = await _sectionRepo.GetByIdAsync(rack.LibraryItemSectionId);
            return View(rack);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _rackRepo.DeleteAsync(id);
            TempData["Success"] = "Library Rack deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSectionDropdown(object selected = null)
        {
            var sections = await _sectionRepo.GetAllAsync();
            ViewBag.SectionList = new SelectList(sections, "Id", "Name", selected);
        }
    }
}

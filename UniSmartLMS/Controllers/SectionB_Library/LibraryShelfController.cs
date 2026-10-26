using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionB_Library
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LibraryShelfController : Controller
    {
        private readonly IGenericRepository<LibraryShelf> _shelfRepo;
        private readonly IGenericRepository<LibraryRack> _rackRepo;

        public LibraryShelfController(
            IGenericRepository<LibraryShelf> shelfRepo,
            IGenericRepository<LibraryRack> rackRepo)
        {
            _shelfRepo = shelfRepo;
            _rackRepo = rackRepo;
        }

        public async Task<IActionResult> Index()
        {
            var shelves = await _shelfRepo.GetAllAsync();
            foreach (var shelf in shelves)
            {
                shelf.LibraryRack = await _rackRepo.GetByIdAsync(shelf.LibraryRackId);
            }
            return View(shelves);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _shelfRepo.SearchAsync("Code", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var shelf = await _shelfRepo.GetByIdAsync(id);
            if (shelf == null) return NotFound();
            shelf.LibraryRack = await _rackRepo.GetByIdAsync(shelf.LibraryRackId);
            return View(shelf);
        }

        public async Task<IActionResult> Create()
        {
            await LoadRackDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LibraryShelf shelf)
        {
            ModelState.Remove("LibraryRack");

            if (ModelState.IsValid)
            {
                await _shelfRepo.AddAsync(shelf);
                TempData["Success"] = "Library Shelf created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadRackDropdown(shelf.LibraryRackId);
            return View(shelf);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var shelf = await _shelfRepo.GetByIdAsync(id);
            if (shelf == null) return NotFound();
            await LoadRackDropdown(shelf.LibraryRackId);
            return View(shelf);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LibraryShelf shelf)
        {
            if (id != shelf.Id) return NotFound();

            ModelState.Remove("LibraryRack");

            if (ModelState.IsValid)
            {
                await _shelfRepo.UpdateAsync(shelf);
                TempData["Success"] = "Library Shelf updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadRackDropdown(shelf.LibraryRackId);
            return View(shelf);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var shelf = await _shelfRepo.GetByIdAsync(id);
            if (shelf == null) return NotFound();
            shelf.LibraryRack = await _rackRepo.GetByIdAsync(shelf.LibraryRackId);
            return View(shelf);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _shelfRepo.DeleteAsync(id);
            TempData["Success"] = "Library Shelf deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRackDropdown(object selected = null)
        {
            var racks = await _rackRepo.GetAllAsync();
            ViewBag.RackList = new SelectList(racks, "Id", "Code", selected);
        }
    }
}
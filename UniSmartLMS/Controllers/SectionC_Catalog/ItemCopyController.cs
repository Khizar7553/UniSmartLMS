using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services;
using UniSmartLMS.Services.Implementations;

namespace UniSmartLMS.Controllers.SectionC_Catalog
{
    [Authorize(Roles = "Admin,Librarian")]
    public class ItemCopyController : Controller
    {
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IGenericRepository<ItemInfo> _itemRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;
        private readonly IGenericRepository<LibraryRack> _rackRepo;
        private readonly IGenericRepository<LibraryShelf> _shelfRepo;
        private readonly IAccessionService _accessionService;

        public ItemCopyController(
            IGenericRepository<ItemCopy> copyRepo,
            IGenericRepository<ItemInfo> itemRepo,
            IGenericRepository<LibraryBranch> branchRepo,
            IGenericRepository<LibraryRack> rackRepo,
            IGenericRepository<LibraryShelf> shelfRepo,
            IAccessionService accessionService)
        {
            _copyRepo = copyRepo;
            _itemRepo = itemRepo;
            _branchRepo = branchRepo;
            _rackRepo = rackRepo;
            _shelfRepo = shelfRepo;
            _accessionService = accessionService;
        }

        public async Task<IActionResult> Index()
        {
            var copies = await _copyRepo.GetAllAsync();
            foreach (var copy in copies)
            {
                copy.ItemInfo = await _itemRepo.GetByIdAsync(copy.ItemInfoId);
                if (copy.LibraryBranchId.HasValue)
                    copy.LibraryBranch = await _branchRepo.GetByIdAsync(copy.LibraryBranchId.Value);
                if (copy.LibraryRackId.HasValue)
                    copy.LibraryRack = await _rackRepo.GetByIdAsync(copy.LibraryRackId.Value);
                if (copy.LibraryShelfId.HasValue)
                    copy.LibraryShelf = await _shelfRepo.GetByIdAsync(copy.LibraryShelfId.Value);
            }
            return View(copies);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _copyRepo.SearchAsync("Barcode", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var copy = await _copyRepo.GetByIdAsync(id);
            if (copy == null) return NotFound();
            copy.ItemInfo = await _itemRepo.GetByIdAsync(copy.ItemInfoId);
            if (copy.LibraryBranchId.HasValue)
                copy.LibraryBranch = await _branchRepo.GetByIdAsync(copy.LibraryBranchId.Value);
            if (copy.LibraryRackId.HasValue)
                copy.LibraryRack = await _rackRepo.GetByIdAsync(copy.LibraryRackId.Value);
            if (copy.LibraryShelfId.HasValue)
                copy.LibraryShelf = await _shelfRepo.GetByIdAsync(copy.LibraryShelfId.Value);
            return View(copy);
        }

        public async Task<IActionResult> Create()
        {
            await LoadItemDropdown();
            await LoadBranchDropdown();
            await LoadRackDropdown();
            await LoadShelfDropdown();

            // ✅ Generate accession number automatically
            var accessionNumber = await _accessionService.GenerateAccessionNumberAsync(1);
            ViewBag.GeneratedAccessionNumber = accessionNumber;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCopy copy)
        {
            ModelState.Remove("ItemInfo");
            ModelState.Remove("LibraryBranch");
            ModelState.Remove("LibraryRack");
            ModelState.Remove("LibraryShelf");

            // ✅ Set default values
            copy.IsActive = true;
            copy.ConditionLastChecked = DateTime.Now;

            // ✅ Generate accession number if not provided
            if (string.IsNullOrEmpty(copy.AccessionNumber))
            {
                copy.AccessionNumber = await _accessionService.GenerateAccessionNumberAsync(
                    copy.LibraryBranchId ?? 1);
            }

            if (ModelState.IsValid)
            {
                await _copyRepo.AddAsync(copy);
                TempData["Success"] = $"Book Copy created successfully! Accession No: {copy.AccessionNumber}";
                return RedirectToAction(nameof(Index));
            }

            await LoadItemDropdown(copy.ItemInfoId);
            await LoadBranchDropdown(copy.LibraryBranchId);
            await LoadRackDropdown(copy.LibraryRackId);
            await LoadShelfDropdown(copy.LibraryShelfId);

            var accessionNumber = await _accessionService.GenerateAccessionNumberAsync(1);
            ViewBag.GeneratedAccessionNumber = accessionNumber;

            return View(copy);
        }

        // ✅ ADD BULK CREATE METHOD
        [HttpPost]
        public async Task<JsonResult> BulkCreate(int itemInfoId, int quantity, int libraryBranchId, int libraryRackId, int libraryShelfId, decimal unitCost)
        {
            var createdCopies = new List<string>();

            for (int i = 0; i < quantity; i++)
            {
                var accessionNumber = await _accessionService.GenerateAccessionNumberAsync(libraryBranchId);
                var barcode = $"BAR-{itemInfoId}-{DateTime.Now.Ticks}-{i}";

                var copy = new ItemCopy
                {
                    ItemInfoId = itemInfoId,
                    LibraryBranchId = libraryBranchId,
                    LibraryRackId = libraryRackId,
                    LibraryShelfId = libraryShelfId,
                    Barcode = barcode,
                    AccessionNumber = accessionNumber,
                    UnitCost = unitCost,
                    ConditionRemarks = "New",
                    ConditionLastChecked = DateTime.Now,
                    IsActive = true
                };

                await _copyRepo.AddAsync(copy);
                createdCopies.Add(accessionNumber);
            }

            return Json(new { success = true, message = $"{quantity} copies created", accessionNumbers = createdCopies });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var copy = await _copyRepo.GetByIdAsync(id);
            if (copy == null) return NotFound();
            await LoadItemDropdown(copy.ItemInfoId);
            await LoadBranchDropdown(copy.LibraryBranchId);
            await LoadRackDropdown(copy.LibraryRackId);
            await LoadShelfDropdown(copy.LibraryShelfId);
            return View(copy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemCopy copy)
        {
            if (id != copy.Id) return NotFound();

            ModelState.Remove("ItemInfo");
            ModelState.Remove("LibraryBranch");
            ModelState.Remove("LibraryRack");
            ModelState.Remove("LibraryShelf");

            if (ModelState.IsValid)
            {
                await _copyRepo.UpdateAsync(copy);
                TempData["Success"] = "Book Copy updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadItemDropdown(copy.ItemInfoId);
            await LoadBranchDropdown(copy.LibraryBranchId);
            await LoadRackDropdown(copy.LibraryRackId);
            await LoadShelfDropdown(copy.LibraryShelfId);
            return View(copy);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var copy = await _copyRepo.GetByIdAsync(id);
            if (copy == null) return NotFound();
            copy.ItemInfo = await _itemRepo.GetByIdAsync(copy.ItemInfoId);
            return View(copy);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _copyRepo.DeleteAsync(id);
            TempData["Success"] = "Book Copy deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ GET AVAILABLE COPIES FOR A BOOK
        [HttpGet]
        public async Task<JsonResult> GetAvailableCopies(int itemInfoId)
        {
            var copies = await _copyRepo.GetAllAsync();
            var availableCopies = copies.Where(c => c.ItemInfoId == itemInfoId && c.IsActive);
            return Json(new { count = availableCopies.Count(), copies = availableCopies.Select(c => new { c.Id, c.Barcode }) });
        }

        private async Task LoadItemDropdown(object selected = null)
        {
            var items = await _itemRepo.GetAllAsync();
            ViewBag.ItemList = new SelectList(items, "Id", "Title", selected);
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }

        private async Task LoadRackDropdown(object selected = null)
        {
            var racks = await _rackRepo.GetAllAsync();
            ViewBag.RackList = new SelectList(racks, "Id", "Code", selected);
        }

        private async Task LoadShelfDropdown(object selected = null)
        {
            var shelves = await _shelfRepo.GetAllAsync();
            ViewBag.ShelfList = new SelectList(shelves, "Id", "Code", selected);
        }
    }
}
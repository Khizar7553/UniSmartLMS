using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian")]
    public class ItemRequisitionController : Controller
    {
        private readonly IGenericRepository<ItemRequisition> _requisitionRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;
        private readonly IGenericRepository<ItemInfo> _itemRepo;
        private readonly IGenericRepository<LibraryRack> _rackRepo;

        public ItemRequisitionController(
            IGenericRepository<ItemRequisition> requisitionRepo,
            IGenericRepository<LibraryBranch> branchRepo,
            IGenericRepository<ItemInfo> itemRepo,
            IGenericRepository<LibraryRack> rackRepo)
        {
            _requisitionRepo = requisitionRepo;
            _branchRepo = branchRepo;
            _itemRepo = itemRepo;
            _rackRepo = rackRepo;
        }

        public async Task<IActionResult> Index()
        {
            var requisitions = await _requisitionRepo.GetAllAsync();
            foreach (var req in requisitions)
            {
                req.LibraryBranch = await _branchRepo.GetByIdAsync(req.LibraryBranchId);
                req.ItemInfo = await _itemRepo.GetByIdAsync(req.ItemInfoId);
                req.LibraryRack = await _rackRepo.GetByIdAsync(req.LibraryRackId);
            }
            return View(requisitions);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _requisitionRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null) return NotFound();
            requisition.LibraryBranch = await _branchRepo.GetByIdAsync(requisition.LibraryBranchId);
            requisition.ItemInfo = await _itemRepo.GetByIdAsync(requisition.ItemInfoId);
            requisition.LibraryRack = await _rackRepo.GetByIdAsync(requisition.LibraryRackId);
            return View(requisition);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBranchDropdown();
            await LoadItemDropdown();
            await LoadRackDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemRequisition requisition)
        {
            ModelState.Remove("LibraryBranch");
            ModelState.Remove("ItemInfo");
            ModelState.Remove("LibraryRack");

            if (ModelState.IsValid)
            {
                await _requisitionRepo.AddAsync(requisition);
                TempData["Success"] = "Item Requisition created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(requisition.LibraryBranchId);
            await LoadItemDropdown(requisition.ItemInfoId);
            await LoadRackDropdown(requisition.LibraryRackId);
            return View(requisition);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null) return NotFound();
            await LoadBranchDropdown(requisition.LibraryBranchId);
            await LoadItemDropdown(requisition.ItemInfoId);
            await LoadRackDropdown(requisition.LibraryRackId);
            return View(requisition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemRequisition requisition)
        {
            if (id != requisition.Id) return NotFound();

            ModelState.Remove("LibraryBranch");
            ModelState.Remove("ItemInfo");
            ModelState.Remove("LibraryRack");

            if (ModelState.IsValid)
            {
                await _requisitionRepo.UpdateAsync(requisition);
                TempData["Success"] = "Item Requisition updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(requisition.LibraryBranchId);
            await LoadItemDropdown(requisition.ItemInfoId);
            await LoadRackDropdown(requisition.LibraryRackId);
            return View(requisition);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null) return NotFound();
            requisition.LibraryBranch = await _branchRepo.GetByIdAsync(requisition.LibraryBranchId);
            requisition.ItemInfo = await _itemRepo.GetByIdAsync(requisition.ItemInfoId);
            requisition.LibraryRack = await _rackRepo.GetByIdAsync(requisition.LibraryRackId);
            return View(requisition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _requisitionRepo.DeleteAsync(id);
            TempData["Success"] = "Item Requisition deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }

        private async Task LoadItemDropdown(object selected = null)
        {
            var items = await _itemRepo.GetAllAsync();
            ViewBag.ItemList = new SelectList(items, "Id", "Title", selected);
        }

        private async Task LoadRackDropdown(object selected = null)
        {
            var racks = await _rackRepo.GetAllAsync();
            ViewBag.RackList = new SelectList(racks, "Id", "Code", selected);
        }
    }
}

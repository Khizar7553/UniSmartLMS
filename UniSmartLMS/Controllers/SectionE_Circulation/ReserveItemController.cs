using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian,Staff")]
    public class ReserveItemController : Controller
    {
        private readonly IGenericRepository<ReserveItem> _reserveRepo;
        private readonly IGenericRepository<ItemInfo> _itemRepo;
        private readonly IGenericRepository<Duration> _durationRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;

        public ReserveItemController(
            IGenericRepository<ReserveItem> reserveRepo,
            IGenericRepository<ItemInfo> itemRepo,
            IGenericRepository<Duration> durationRepo,
            IGenericRepository<LibraryMember> memberRepo)
        {
            _reserveRepo = reserveRepo;
            _itemRepo = itemRepo;
            _durationRepo = durationRepo;
            _memberRepo = memberRepo;
        }

        public async Task<IActionResult> Index()
        {
            var reserves = await _reserveRepo.GetAllAsync();
            foreach (var reserve in reserves)
            {
                reserve.ItemInfo = await _itemRepo.GetByIdAsync(reserve.ItemInfoId);
                reserve.Duration = await _durationRepo.GetByIdAsync(reserve.DurationId);
                reserve.LibraryMember = await _memberRepo.GetByIdAsync(reserve.LibraryMemberId);
            }
            return View(reserves);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _reserveRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var reserve = await _reserveRepo.GetByIdAsync(id);
            if (reserve == null) return NotFound();
            reserve.ItemInfo = await _itemRepo.GetByIdAsync(reserve.ItemInfoId);
            reserve.Duration = await _durationRepo.GetByIdAsync(reserve.DurationId);
            reserve.LibraryMember = await _memberRepo.GetByIdAsync(reserve.LibraryMemberId);
            return View(reserve);
        }

        public async Task<IActionResult> Create()
        {
            await LoadItemDropdown();
            await LoadDurationDropdown();
            await LoadMemberDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReserveItem reserve)
        {
            ModelState.Remove("ItemInfo");
            ModelState.Remove("Duration");
            ModelState.Remove("LibraryMember");

            reserve.IsAvailable = false;
            reserve.ExpiryDate = DateTime.Now.AddDays(7); // Default 7 days expiry

            if (ModelState.IsValid)
            {
                await _reserveRepo.AddAsync(reserve);
                TempData["Success"] = "Book reserved successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadItemDropdown(reserve.ItemInfoId);
            await LoadDurationDropdown(reserve.DurationId);
            await LoadMemberDropdown(reserve.LibraryMemberId);
            return View(reserve);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var reserve = await _reserveRepo.GetByIdAsync(id);
            if (reserve == null) return NotFound();
            await LoadItemDropdown(reserve.ItemInfoId);
            await LoadDurationDropdown(reserve.DurationId);
            await LoadMemberDropdown(reserve.LibraryMemberId);
            return View(reserve);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReserveItem reserve)
        {
            if (id != reserve.Id) return NotFound();

            ModelState.Remove("ItemInfo");
            ModelState.Remove("Duration");
            ModelState.Remove("LibraryMember");

            if (ModelState.IsValid)
            {
                await _reserveRepo.UpdateAsync(reserve);
                TempData["Success"] = "Reservation updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadItemDropdown(reserve.ItemInfoId);
            await LoadDurationDropdown(reserve.DurationId);
            await LoadMemberDropdown(reserve.LibraryMemberId);
            return View(reserve);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var reserve = await _reserveRepo.GetByIdAsync(id);
            if (reserve == null) return NotFound();
            reserve.ItemInfo = await _itemRepo.GetByIdAsync(reserve.ItemInfoId);
            reserve.Duration = await _durationRepo.GetByIdAsync(reserve.DurationId);
            reserve.LibraryMember = await _memberRepo.GetByIdAsync(reserve.LibraryMemberId);
            return View(reserve);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reserveRepo.DeleteAsync(id);
            TempData["Success"] = "Reservation cancelled successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadItemDropdown(object selected = null)
        {
            var items = await _itemRepo.GetAllAsync();
            ViewBag.ItemList = new SelectList(items, "Id", "Title", selected);
        }

        private async Task LoadDurationDropdown(object selected = null)
        {
            var durations = await _durationRepo.GetAllAsync();
            ViewBag.DurationList = new SelectList(durations, "Id", "Name", selected);
        }

        private async Task LoadMemberDropdown(object selected = null)
        {
            var members = await _memberRepo.GetAllAsync();
            ViewBag.MemberList = new SelectList(members, "Id", "CardNo", selected);
        }
    }
}

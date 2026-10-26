using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionD_People
{
    [Authorize(Roles = "Admin,Librarian")]
    public class MemberQuotaOverrideController : Controller
    {
        private readonly IGenericRepository<MemberQuotaOverride> _quotaRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;

        public MemberQuotaOverrideController(
            IGenericRepository<MemberQuotaOverride> quotaRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<LibraryBranch> branchRepo)
        {
            _quotaRepo = quotaRepo;
            _memberRepo = memberRepo;
            _branchRepo = branchRepo;
        }

        public async Task<IActionResult> Index()
        {
            var quotas = await _quotaRepo.GetAllAsync();
            foreach (var quota in quotas)
            {
                quota.LibraryMember = await _memberRepo.GetByIdAsync(quota.LibraryMemberId);
                quota.LibraryBranch = await _branchRepo.GetByIdAsync(quota.LibraryBranchId);
            }
            return View(quotas);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _quotaRepo.SearchAsync("LibraryMemberId", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var quota = await _quotaRepo.GetByIdAsync(id);
            if (quota == null) return NotFound();
            quota.LibraryMember = await _memberRepo.GetByIdAsync(quota.LibraryMemberId);
            quota.LibraryBranch = await _branchRepo.GetByIdAsync(quota.LibraryBranchId);
            return View(quota);
        }

        public async Task<IActionResult> Create()
        {
            await LoadMemberDropdown();
            await LoadBranchDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberQuotaOverride quota)
        {
            ModelState.Remove("LibraryMember");
            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _quotaRepo.AddAsync(quota);
                TempData["Success"] = "Member quota override created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(quota.LibraryMemberId);

            await LoadBranchDropdown(quota.LibraryBranchId);
            return View(quota);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var quota = await _quotaRepo.GetByIdAsync(id);
            if (quota == null) return NotFound();
            await LoadMemberDropdown(quota.LibraryMemberId);
            await LoadBranchDropdown(quota.LibraryBranchId);
            return View(quota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MemberQuotaOverride quota)
        {
            if (id != quota.Id) return NotFound();

            ModelState.Remove("LibraryMember");
            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _quotaRepo.UpdateAsync(quota);
                TempData["Success"] = "Member quota override updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(quota.LibraryMemberId);
            await LoadBranchDropdown(quota.LibraryBranchId);
            return View(quota);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var quota = await _quotaRepo.GetByIdAsync(id);
            if (quota == null) return NotFound();
            quota.LibraryMember = await _memberRepo.GetByIdAsync(quota.LibraryMemberId);
            quota.LibraryBranch = await _branchRepo.GetByIdAsync(quota.LibraryBranchId);
            return View(quota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _quotaRepo.DeleteAsync(id);
            TempData["Success"] = "Member quota override deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadMemberDropdown(object selected = null)
        {
            var members = await _memberRepo.GetAllAsync();
            ViewBag.MemberList = new SelectList(members, "Id", "CardNo", selected);
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }
    }
}
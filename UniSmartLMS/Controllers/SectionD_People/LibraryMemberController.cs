using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services;
using UniSmartLMS.Services.Implementations;

namespace UniSmartLMS.Controllers.SectionD_People
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LibraryMemberController : Controller
    {
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;
        private readonly IGenericRepository<Party> _partyRepo;
        private readonly IMemberService _memberService;

        public LibraryMemberController(
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<LibraryBranch> branchRepo,
            IGenericRepository<Party> partyRepo,
            IMemberService memberService)
        {
            _memberRepo = memberRepo;
            _branchRepo = branchRepo;
            _partyRepo = partyRepo;
            _memberService = memberService;
        }

        public async Task<IActionResult> Index()
        {
            var members = await _memberRepo.GetAllAsync();
            foreach (var member in members)
            {
                member.LibraryBranch = await _branchRepo.GetByIdAsync(member.LibraryBranchId);
                member.Party = await _partyRepo.GetByIdAsync(member.PartyId);
            }
            return View(members);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _memberRepo.SearchAsync("CardNo", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return NotFound();
            member.LibraryBranch = await _branchRepo.GetByIdAsync(member.LibraryBranchId);
            member.Party = await _partyRepo.GetByIdAsync(member.PartyId);

            // Get total fines for this member
            var totalFine = await _memberService.GetMemberTotalFineAsync(member.Id);
            ViewBag.TotalFine = totalFine;

            return View(member);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBranchDropdown();
            await LoadPartyDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LibraryMember member)
        {
            ModelState.Remove("LibraryBranch");
            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                // ✅ USE MEMBER SERVICE TO REGISTER
                var newMember = await _memberService.RegisterLibraryMemberAsync(
                    member.PartyId,
                    member.LibraryBranchId,
                    member.CardNo,
                    member.MembershipType);

                TempData["Success"] = "Library Member registered successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(member.LibraryBranchId);
            await LoadPartyDropdown(member.PartyId);
            return View(member);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return NotFound();
            await LoadBranchDropdown(member.LibraryBranchId);
            await LoadPartyDropdown(member.PartyId);
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LibraryMember member)
        {
            if (id != member.Id) return NotFound();

            ModelState.Remove("LibraryBranch");
            ModelState.Remove("Party");

            if (ModelState.IsValid)
            {
                await _memberRepo.UpdateAsync(member);
                TempData["Success"] = "Library Member updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(member.LibraryBranchId);
            await LoadPartyDropdown(member.PartyId);
            return View(member);
        }

        // ✅ ADD RENEW MEMBERSHIP METHOD
        [HttpPost]
        public async Task<JsonResult> RenewMembership(int id)
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null)
                return Json(new { success = false, message = "Member not found" });

            var totalFine = await _memberService.GetMemberTotalFineAsync(member.Id);
            if (totalFine > 100)
                return Json(new { success = false, message = $"Cannot renew. Outstanding fines: Rs. {totalFine:N2}. Please clear fines first." });

            member.ExpiredOn = DateTime.Now.AddYears(1);
            member.IsActive = true;
            await _memberRepo.UpdateAsync(member);

            return Json(new { success = true, message = $"Membership renewed until {member.ExpiredOn:dd MMM yyyy}" });
        }

        // ✅ ADD BLOCK MEMBER METHOD
        [HttpPost]
        public async Task<JsonResult> BlockMember(int id, string reason, int days)
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null)
                return Json(new { success = false, message = "Member not found" });

            await _memberService.BlockMemberAsync(member.Id, reason, days);
            member.IsActive = false;
            await _memberRepo.UpdateAsync(member);

            return Json(new { success = true, message = $"Member blocked for {days} days. Reason: {reason}" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var member = await _memberRepo.GetByIdAsync(id);
            if (member == null) return NotFound();
            member.LibraryBranch = await _branchRepo.GetByIdAsync(member.LibraryBranchId);
            member.Party = await _partyRepo.GetByIdAsync(member.PartyId);
            return View(member);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _memberRepo.DeleteAsync(id);
            TempData["Success"] = "Library Member deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }

        private async Task LoadPartyDropdown(object selected = null)
        {
            var parties = await _partyRepo.GetAllAsync();
            ViewBag.PartyList = new SelectList(parties, "Id", "PartyName", selected);
        }
    }
}
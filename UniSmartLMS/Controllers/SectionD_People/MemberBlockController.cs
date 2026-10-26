using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionD_People
{
    [Authorize(Roles = "Admin,Librarian")]
    public class MemberBlockController : Controller
    {
        private readonly IGenericRepository<MemberBlock> _blockRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;

        public MemberBlockController(
            IGenericRepository<MemberBlock> blockRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<LibraryBranch> branchRepo)
        {
            _blockRepo = blockRepo;
            _memberRepo = memberRepo;
            _branchRepo = branchRepo;
        }

        public async Task<IActionResult> Index()
        {
            var blocks = await _blockRepo.GetAllAsync();
            foreach (var block in blocks)
            {

                block.LibraryMember = await _memberRepo.GetByIdAsync(block.LibraryMemberId);
                block.LibraryBranch = await _branchRepo.GetByIdAsync(block.LibraryBranchId);
            }
            return View(blocks);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _blockRepo.SearchAsync("Reason", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var block = await _blockRepo.GetByIdAsync(id);
            if (block == null) return NotFound();
            block.LibraryMember = await _memberRepo.GetByIdAsync(block.LibraryMemberId);
            block.LibraryBranch = await _branchRepo.GetByIdAsync(block.LibraryBranchId);
            return View(block);
        }

        public async Task<IActionResult> Create()
        {
            await LoadMemberDropdown();
            await LoadBranchDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberBlock block)
        {
            ModelState.Remove("LibraryMember");
            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _blockRepo.AddAsync(block);
                TempData["Success"] = "Member blocked successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(block.LibraryMemberId);
            await LoadBranchDropdown(block.LibraryBranchId);
            return View(block);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var block = await _blockRepo.GetByIdAsync(id);
            if (block == null) return NotFound();
            await LoadMemberDropdown(block.LibraryMemberId);
            await LoadBranchDropdown(block.LibraryBranchId);
            return View(block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MemberBlock block)
        {
            if (id != block.Id) return NotFound();

            ModelState.Remove("LibraryMember");
            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _blockRepo.UpdateAsync(block);
                TempData["Success"] = "Member block updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(block.LibraryMemberId);
            await LoadBranchDropdown(block.LibraryBranchId);
            return View(block);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var block = await _blockRepo.GetByIdAsync(id);
            if (block == null) return NotFound();
            block.LibraryMember = await _memberRepo.GetByIdAsync(block.LibraryMemberId);
            block.LibraryBranch = await _branchRepo.GetByIdAsync(block.LibraryBranchId);
            return View(block);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _blockRepo.DeleteAsync(id);
            TempData["Success"] = "Member unblocked successfully!";
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
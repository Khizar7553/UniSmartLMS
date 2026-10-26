using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionF_Fines
{
    [Authorize(Roles = "Admin,Librarian")]
    public class UserFineController : Controller
    {
        private readonly IGenericRepository<UserFine> _userFineRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<FineDefinition> _definitionRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;

        public UserFineController(
            IGenericRepository<UserFine> userFineRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<FineDefinition> definitionRepo,
            IGenericRepository<ItemCopy> copyRepo)
        {
            _userFineRepo = userFineRepo;
            _memberRepo = memberRepo;
            _definitionRepo = definitionRepo;
            _copyRepo = copyRepo;
        }

        public async Task<IActionResult> Index()
        {
            var fines = await _userFineRepo.GetAllAsync();
            foreach (var fine in fines)
            {
                fine.LibraryMember = await _memberRepo.GetByIdAsync(fine.LibraryMemberId);
                fine.FineDefinition = await _definitionRepo.GetByIdAsync(fine.FineDefinitionId);
                if (fine.ItemCopyId.HasValue)
                    fine.ItemCopy = await _copyRepo.GetByIdAsync(fine.ItemCopyId.Value);
            }
            return View(fines);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _userFineRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var fine = await _userFineRepo.GetByIdAsync(id);
            if (fine == null) return NotFound();
            fine.LibraryMember = await _memberRepo.GetByIdAsync(fine.LibraryMemberId);
            fine.FineDefinition = await _definitionRepo.GetByIdAsync(fine.FineDefinitionId);
            if (fine.ItemCopyId.HasValue)
                fine.ItemCopy = await _copyRepo.GetByIdAsync(fine.ItemCopyId.Value);
            return View(fine);
        }

        public async Task<IActionResult> Create()
        {
            await LoadMemberDropdown();
            await LoadDefinitionDropdown();
            await LoadCopyDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFine userFine)
        {
            ModelState.Remove("LibraryMember");
            ModelState.Remove("FineDefinition");
            ModelState.Remove("ItemCopy");

            if (ModelState.IsValid)
            {
                await _userFineRepo.AddAsync(userFine);
                TempData["Success"] = "User Fine created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(userFine.LibraryMemberId);
            await LoadDefinitionDropdown(userFine.FineDefinitionId);
            await LoadCopyDropdown(userFine.ItemCopyId);
            return View(userFine);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var fine = await _userFineRepo.GetByIdAsync(id);
            if (fine == null) return NotFound();
            await LoadMemberDropdown(fine.LibraryMemberId);
            await LoadDefinitionDropdown(fine.FineDefinitionId);
            await LoadCopyDropdown(fine.ItemCopyId);
            return View(fine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFine userFine)
        {
            if (id != userFine.Id) return NotFound();

            ModelState.Remove("LibraryMember");
            ModelState.Remove("FineDefinition");
            ModelState.Remove("ItemCopy");

            if (ModelState.IsValid)
            {
                await _userFineRepo.UpdateAsync(userFine);
                TempData["Success"] = "User Fine updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(userFine.LibraryMemberId);
            await LoadDefinitionDropdown(userFine.FineDefinitionId);
            await LoadCopyDropdown(userFine.ItemCopyId);
            return View(userFine);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var fine = await _userFineRepo.GetByIdAsync(id);
            if (fine == null) return NotFound();
            fine.LibraryMember = await _memberRepo.GetByIdAsync(fine.LibraryMemberId);
            fine.FineDefinition = await _definitionRepo.GetByIdAsync(fine.FineDefinitionId);
            if (fine.ItemCopyId.HasValue)
                fine.ItemCopy = await _copyRepo.GetByIdAsync(fine.ItemCopyId.Value);
            return View(fine);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userFineRepo.DeleteAsync(id);
            TempData["Success"] = "User Fine deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadMemberDropdown(object selected = null)
        {
            var members = await _memberRepo.GetAllAsync();
            ViewBag.MemberList = new SelectList(members, "Id", "CardNo", selected);
        }

        private async Task LoadDefinitionDropdown(object selected = null)
        {
            var definitions = await _definitionRepo.GetAllAsync();
            ViewBag.DefinitionList = new SelectList(definitions, "Id", "Id", selected);
        }

        private async Task LoadCopyDropdown(object selected = null)
        {
            var copies = await _copyRepo.GetAllAsync();
            ViewBag.CopyList = new SelectList(copies, "Id", "Barcode", selected);
        }
    }
}

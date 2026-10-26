using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionB_Library
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LibraryBranchController : Controller
    {
        private readonly IGenericRepository<LibraryBranch> _branchRepo;
        private readonly IGenericRepository<LibraryType> _libraryTypeRepo;

        public LibraryBranchController(
            IGenericRepository<LibraryBranch> branchRepo,
            IGenericRepository<LibraryType> libraryTypeRepo)
        {
            _branchRepo = branchRepo;
            _libraryTypeRepo = libraryTypeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _branchRepo.GetAllAsync();
            foreach (var branch in branches)
            {
                branch.LibraryType = await _libraryTypeRepo.GetByIdAsync(branch.LibraryTypeId);
                if (branch.ParentId.HasValue)
                    branch.ParentBranch = await _branchRepo.GetByIdAsync(branch.ParentId.Value);
            }
            return View(branches);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _branchRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            if (branch == null) return NotFound();

            branch.LibraryType = await _libraryTypeRepo.GetByIdAsync(branch.LibraryTypeId);
            if (branch.ParentId.HasValue)
                branch.ParentBranch = await _branchRepo.GetByIdAsync(branch.ParentId.Value);

            return View(branch);
        }

        public async Task<IActionResult> Create()
        {
            await LoadLibraryTypesDropdown();
            await LoadParentBranchesDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LibraryBranch branch)
        {
            ModelState.Remove("LibraryType");
            ModelState.Remove("ParentBranch");

            if (ModelState.IsValid)
            {
                await _branchRepo.AddAsync(branch);
                TempData["Success"] = "Library Branch created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadLibraryTypesDropdown(branch.LibraryTypeId);
            await LoadParentBranchesDropdown(branch.ParentId);
            return View(branch);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            if (branch == null) return NotFound();

            await LoadLibraryTypesDropdown(branch.LibraryTypeId);
            await LoadParentBranchesDropdown(branch.ParentId);
            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LibraryBranch branch)
        {
            if (id != branch.Id) return NotFound();

            ModelState.Remove("LibraryType");
            ModelState.Remove("ParentBranch");

            if (ModelState.IsValid)
            {
                await _branchRepo.UpdateAsync(branch);
                TempData["Success"] = "Library Branch updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadLibraryTypesDropdown(branch.LibraryTypeId);
            await LoadParentBranchesDropdown(branch.ParentId);
            return View(branch);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            if (branch == null) return NotFound();

            branch.LibraryType = await _libraryTypeRepo.GetByIdAsync(branch.LibraryTypeId);
            if (branch.ParentId.HasValue)
                branch.ParentBranch = await _branchRepo.GetByIdAsync(branch.ParentId.Value);

            return View(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _branchRepo.DeleteAsync(id);
            TempData["Success"] = "Library Branch deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadLibraryTypesDropdown(object selectedType = null)
        {
            var types = await _libraryTypeRepo.GetAllAsync();
            ViewBag.LibraryTypeList = new SelectList(types, "Id", "Name", selectedType);
        }

        private async Task LoadParentBranchesDropdown(object selectedParent = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.ParentBranchList = new SelectList(branches, "Id", "Name", selectedParent);
        }
    }
}
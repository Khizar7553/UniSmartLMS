using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionB_Library
{
    [Authorize(Roles = "Admin,Librarian")]
    public class LibraryItemSectionController : Controller
    {
        private readonly IGenericRepository<LibraryItemSection> _sectionRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;
        private readonly IGenericRepository<Department> _deptRepo;

        public LibraryItemSectionController(
            IGenericRepository<LibraryItemSection> sectionRepo,
            IGenericRepository<LibraryBranch> branchRepo,
            IGenericRepository<Department> deptRepo)
        {
            _sectionRepo = sectionRepo;
            _branchRepo = branchRepo;
            _deptRepo = deptRepo;
        }

        public async Task<IActionResult> Index()
        {
            var sections = await _sectionRepo.GetAllAsync();
            foreach (var section in sections)
            {
                section.LibraryBranch = await _branchRepo.GetByIdAsync(section.LibraryBranchId);
                section.Department = await _deptRepo.GetByIdAsync(section.DepartmentId);
            }
            return View(sections);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _sectionRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section == null) return NotFound();
            section.LibraryBranch = await _branchRepo.GetByIdAsync(section.LibraryBranchId);
            section.Department = await _deptRepo.GetByIdAsync(section.DepartmentId);
            return View(section);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBranchDropdown();
            await LoadDepartmentDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LibraryItemSection section)
        {
            ModelState.Remove("LibraryBranch");
            ModelState.Remove("Department");

            if (ModelState.IsValid)
            {
                await _sectionRepo.AddAsync(section);
                TempData["Success"] = "Library Section created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(section.LibraryBranchId);
            await LoadDepartmentDropdown(section.DepartmentId);
            return View(section);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section == null) return NotFound();
            await LoadBranchDropdown(section.LibraryBranchId);
            await LoadDepartmentDropdown(section.DepartmentId);
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LibraryItemSection section)
        {
            if (id != section.Id) return NotFound();

            ModelState.Remove("LibraryBranch");
            ModelState.Remove("Department");

            if (ModelState.IsValid)
            {
                await _sectionRepo.UpdateAsync(section);
                TempData["Success"] = "Library Section updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(section.LibraryBranchId);
            await LoadDepartmentDropdown(section.DepartmentId);
            return View(section);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var section = await _sectionRepo.GetByIdAsync(id);
            if (section == null) return NotFound();
            section.LibraryBranch = await _branchRepo.GetByIdAsync(section.LibraryBranchId);
            section.Department = await _deptRepo.GetByIdAsync(section.DepartmentId);
            return View(section);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _sectionRepo.DeleteAsync(id);
            TempData["Success"] = "Library Section deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }

        private async Task LoadDepartmentDropdown(object selected = null)
        {
            var departments = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name", selected);
        }
    }
}
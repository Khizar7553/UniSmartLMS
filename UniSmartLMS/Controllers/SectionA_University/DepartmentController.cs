using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionA_University
{
    [Authorize(Roles = "Admin,Librarian")]
    public class DepartmentController : Controller
    {
        private readonly IGenericRepository<Department> _deptRepo;
        private readonly IGenericRepository<Institute> _instituteRepo;
        private readonly IGenericRepository<Faculty> _facultyRepo;

        public DepartmentController(
            IGenericRepository<Department> deptRepo,
            IGenericRepository<Institute> instituteRepo,
            IGenericRepository<Faculty> facultyRepo)
        {
            _deptRepo = deptRepo;
            _instituteRepo = instituteRepo;
            _facultyRepo = facultyRepo;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _deptRepo.GetAllAsync();
            foreach (var dept in departments)
            {
                dept.Institute = await _instituteRepo.GetByIdAsync(dept.InstituteId);
                dept.Faculty = await _facultyRepo.GetByIdAsync(dept.FacultyId);
                if (dept.ParentId.HasValue)
                    dept.ParentDepartment = await _deptRepo.GetByIdAsync(dept.ParentId.Value);
            }
            return View(departments);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _deptRepo.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();

            dept.Institute = await _instituteRepo.GetByIdAsync(dept.InstituteId);
            dept.Faculty = await _facultyRepo.GetByIdAsync(dept.FacultyId);
            if (dept.ParentId.HasValue)
                dept.ParentDepartment = await _deptRepo.GetByIdAsync(dept.ParentId.Value);

            return View(dept);
        }

        public async Task<IActionResult> Create()
        {
            await LoadInstituteDropdown();
            await LoadFacultyDropdown();
            await LoadParentDepartmentDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            ModelState.Remove("Institute");
            ModelState.Remove("Faculty");
            ModelState.Remove("ParentDepartment");

            if (ModelState.IsValid)
            {
                await _deptRepo.AddAsync(department);
                TempData["Success"] = "Department created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadInstituteDropdown(department.InstituteId);
            await LoadFacultyDropdown(department.FacultyId);
            await LoadParentDepartmentDropdown(department.ParentId);
            return View(department);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();

            await LoadInstituteDropdown(dept.InstituteId);
            await LoadFacultyDropdown(dept.FacultyId);
            await LoadParentDepartmentDropdown(dept.ParentId);
            return View(dept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id) return NotFound();

            ModelState.Remove("Institute");
            ModelState.Remove("Faculty");
            ModelState.Remove("ParentDepartment");

            if (ModelState.IsValid)
            {
                await _deptRepo.UpdateAsync(department);
                TempData["Success"] = "Department updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadInstituteDropdown(department.InstituteId);
            await LoadFacultyDropdown(department.FacultyId);
            await LoadParentDepartmentDropdown(department.ParentId);
            return View(department);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();

            dept.Institute = await _instituteRepo.GetByIdAsync(dept.InstituteId);
            dept.Faculty = await _facultyRepo.GetByIdAsync(dept.FacultyId);
            if (dept.ParentId.HasValue)
                dept.ParentDepartment = await _deptRepo.GetByIdAsync(dept.ParentId.Value);

            return View(dept);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _deptRepo.DeleteAsync(id);
            TempData["Success"] = "Department deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadInstituteDropdown(object selected = null)
        {
            var institutes = await _instituteRepo.GetAllAsync();
            ViewBag.InstituteList = new SelectList(institutes, "Id", "Name", selected);
        }

        private async Task LoadFacultyDropdown(object selected = null)
        {
            var faculties = await _facultyRepo.GetAllAsync();
            ViewBag.FacultyList = new SelectList(faculties, "Id", "Name", selected);
        }

        private async Task LoadParentDepartmentDropdown(object selected = null)
        {
            var departments = await _deptRepo.GetAllAsync();
            ViewBag.ParentDepartmentList = new SelectList(departments, "Id", "Name", selected);
        }
    }
}
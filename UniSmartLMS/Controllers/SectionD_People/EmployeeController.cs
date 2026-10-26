using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionD_People
{
    [Authorize(Roles = "Admin,Librarian")]
    public class EmployeeController : Controller
    {
        private readonly IGenericRepository<Employee> _repository;

        public EmployeeController(IGenericRepository<Employee> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _repository.GetAllAsync();
            return View(employees);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _repository.SearchAsync("Name", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _repository.AddAsync(employee);
                TempData["Success"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(employee);
                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Employee deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
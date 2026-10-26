using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian")]
    public class WithdrawalController : Controller
    {
        private readonly IGenericRepository<Withdrawal> _withdrawalRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IGenericRepository<Employee> _employeeRepo;

        public WithdrawalController(
            IGenericRepository<Withdrawal> withdrawalRepo,
            IGenericRepository<ItemCopy> copyRepo,
            IGenericRepository<Employee> employeeRepo)
        {
            _withdrawalRepo = withdrawalRepo;
            _copyRepo = copyRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var withdrawals = await _withdrawalRepo.GetAllAsync();
            foreach (var withdrawal in withdrawals)
            {
                withdrawal.ItemCopy = await _copyRepo.GetByIdAsync(withdrawal.ItemCopyId);
                withdrawal.Employee = await _employeeRepo.GetByIdAsync(withdrawal.EmployeeId);
            }
            return View(withdrawals);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _withdrawalRepo.SearchAsync("Reason", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var withdrawal = await _withdrawalRepo.GetByIdAsync(id);
            if (withdrawal == null) return NotFound();
            withdrawal.ItemCopy = await _copyRepo.GetByIdAsync(withdrawal.ItemCopyId);
            withdrawal.Employee = await _employeeRepo.GetByIdAsync(withdrawal.EmployeeId);
            return View(withdrawal);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCopyDropdown();
            await LoadEmployeeDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Withdrawal withdrawal)
        {
            ModelState.Remove("ItemCopy");
            ModelState.Remove("Employee");

            // Set withdrawal date to now if not provided
            if (withdrawal.Date == default)
            {
                withdrawal.Date = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                await _withdrawalRepo.AddAsync(withdrawal);

                // Mark the item copy as inactive (withdrawn)
                var itemCopy = await _copyRepo.GetByIdAsync(withdrawal.ItemCopyId);
                if (itemCopy != null)
                {
                    itemCopy.IsActive = false;
                    await _copyRepo.UpdateAsync(itemCopy);
                }

                TempData["Success"] = "Book withdrawn successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadCopyDropdown(withdrawal.ItemCopyId);
            await LoadEmployeeDropdown(withdrawal.EmployeeId);
            return View(withdrawal);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var withdrawal = await _withdrawalRepo.GetByIdAsync(id);
            if (withdrawal == null) return NotFound();
            await LoadCopyDropdown(withdrawal.ItemCopyId);
            await LoadEmployeeDropdown(withdrawal.EmployeeId);
            return View(withdrawal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Withdrawal withdrawal)
        {
            if (id != withdrawal.Id) return NotFound();

            ModelState.Remove("ItemCopy");
            ModelState.Remove("Employee");

            if (ModelState.IsValid)
            {
                await _withdrawalRepo.UpdateAsync(withdrawal);
                TempData["Success"] = "Withdrawal record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadCopyDropdown(withdrawal.ItemCopyId);
            await LoadEmployeeDropdown(withdrawal.EmployeeId);
            return View(withdrawal);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var withdrawal = await _withdrawalRepo.GetByIdAsync(id);
            if (withdrawal == null) return NotFound();
            withdrawal.ItemCopy = await _copyRepo.GetByIdAsync(withdrawal.ItemCopyId);
            withdrawal.Employee = await _employeeRepo.GetByIdAsync(withdrawal.EmployeeId);
            return View(withdrawal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var withdrawal = await _withdrawalRepo.GetByIdAsync(id);
            if (withdrawal != null)
            {
                // Restore the item copy status
                var itemCopy = await _copyRepo.GetByIdAsync(withdrawal.ItemCopyId);
                if (itemCopy != null)
                {
                    itemCopy.IsActive = true;
                    await _copyRepo.UpdateAsync(itemCopy);
                }
            }

            await _withdrawalRepo.DeleteAsync(id);
            TempData["Success"] = "Withdrawal record deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCopyDropdown(object selected = null)
        {
            var copies = await _copyRepo.GetAllAsync();
            ViewBag.CopyList = new SelectList(copies, "Id", "Barcode", selected);
        }

        private async Task LoadEmployeeDropdown(object selected = null)
        {
            var employees = await _employeeRepo.GetAllAsync();
            ViewBag.EmployeeList = new SelectList(employees, "Id", "Name", selected);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers.SectionF_Fines
{
    [Authorize(Roles = "Admin,Librarian")]
    public class FineTransactionController : Controller
    {
        private readonly IGenericRepository<FineTransaction> _transactionRepo;
        private readonly IGenericRepository<LibraryBranch> _branchRepo;

        public FineTransactionController(
            IGenericRepository<FineTransaction> transactionRepo,
            IGenericRepository<LibraryBranch> branchRepo)
        {
            _transactionRepo = transactionRepo;
            _branchRepo = branchRepo;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _transactionRepo.GetAllAsync();
            foreach (var transaction in transactions)
            {
                transaction.LibraryBranch = await _branchRepo.GetByIdAsync(transaction.LibraryBranchId);
            }
            return View(transactions);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _transactionRepo.SearchAsync("PaymentRef", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            transaction.LibraryBranch = await _branchRepo.GetByIdAsync(transaction.LibraryBranchId);
            return View(transaction);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBranchDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FineTransaction transaction)
        {
            ModelState.Remove("LibraryBranch");

            // Set dates if not provided
            if (transaction.FineAllocationDate == default)
            {
                transaction.FineAllocationDate = DateTime.Now;
            }
            if (transaction.PaidOn == default)
            {
                transaction.PaidOn = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                await _transactionRepo.AddAsync(transaction);
                TempData["Success"] = "Fine Transaction created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(transaction.LibraryBranchId);
            return View(transaction);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            await LoadBranchDropdown(transaction.LibraryBranchId);
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FineTransaction transaction)
        {
            if (id != transaction.Id) return NotFound();

            ModelState.Remove("LibraryBranch");

            if (ModelState.IsValid)
            {
                await _transactionRepo.UpdateAsync(transaction);
                TempData["Success"] = "Fine Transaction updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadBranchDropdown(transaction.LibraryBranchId);
            return View(transaction);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            transaction.LibraryBranch = await _branchRepo.GetByIdAsync(transaction.LibraryBranchId);
            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _transactionRepo.DeleteAsync(id);
            TempData["Success"] = "Fine Transaction deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBranchDropdown(object selected = null)
        {
            var branches = await _branchRepo.GetAllAsync();
            ViewBag.BranchList = new SelectList(branches, "Id", "Description", selected);
        }
    }
}
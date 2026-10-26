using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian,Staff")]
    public class ReturnItemController : Controller
    {
        private readonly IGenericRepository<ReturnItem> _returnRepo;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IFineService _fineService;

        public ReturnItemController(
            IGenericRepository<ReturnItem> returnRepo,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<ItemCopy> copyRepo,
            IFineService fineService)
        {
            _returnRepo = returnRepo;
            _issuanceRepo = issuanceRepo;
            _copyRepo = copyRepo;
            _fineService = fineService;
        }

        public async Task<IActionResult> Index()
        {
            var returns = await _returnRepo.GetAllAsync();
            foreach (var returnItem in returns)
            {
                returnItem.Issuance = await _issuanceRepo.GetByIdAsync(returnItem.IssuanceId);
                if (returnItem.Issuance != null)
                {
                    returnItem.Issuance.ItemCopy = await _copyRepo.GetByIdAsync(returnItem.Issuance.ItemCopyId);
                }
            }
            return View(returns);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _returnRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var returnItem = await _returnRepo.GetByIdAsync(id);
            if (returnItem == null) return NotFound();
            returnItem.Issuance = await _issuanceRepo.GetByIdAsync(returnItem.IssuanceId);
            if (returnItem.Issuance != null)
            {
                returnItem.Issuance.ItemCopy = await _copyRepo.GetByIdAsync(returnItem.Issuance.ItemCopyId);
            }
            return View(returnItem);
        }

        public async Task<IActionResult> Create()
        {
            await LoadIssuanceDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReturnItem returnItem)
        {
            ModelState.Remove("Issuance");

            if (returnItem.ReturnDate == default)
            {
                returnItem.ReturnDate = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                var issuance = await _issuanceRepo.GetByIdAsync(returnItem.IssuanceId);
                if (issuance == null)
                {
                    ModelState.AddModelError("IssuanceId", "Issuance record not found.");
                    await LoadIssuanceDropdown(returnItem.IssuanceId);
                    return View(returnItem);
                }

                // ✅ Calculate fine
                var fineAmount = await _fineService.CalculateFineAsync(issuance, returnItem.ReturnDate);

                if (fineAmount > 0)
                {
                    await _fineService.ApplyFineAsync(issuance, fineAmount);
                    var dueDate = issuance.IssueDate.AddDays(issuance.DurationNumber);
                    var daysLate = (returnItem.ReturnDate.Date - dueDate.Date).Days;
                    TempData["FineMessage"] = $"⚠️ Book is {daysLate} day(s) late. Fine of Rs. {fineAmount:N2} applied.";
                }
                else
                {
                    TempData["Success"] = "✅ Book returned on time! No fine applied.";
                }

                // ✅ Update copy status (soft delete approach)
                var copy = await _copyRepo.GetByIdAsync(issuance.ItemCopyId);
                if (copy != null)
                {
                    copy.IsActive = true;
                    await _copyRepo.UpdateAsync(copy);
                }

                // ✅ Close issuance
                issuance.IsActive = false;
                await _issuanceRepo.UpdateAsync(issuance);

                // ✅ Add return record
                await _returnRepo.AddAsync(returnItem);

                if (fineAmount == 0 && string.IsNullOrEmpty(TempData["Success"]?.ToString()))
                {
                    TempData["Success"] = "Book returned successfully!";
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadIssuanceDropdown(returnItem.IssuanceId);
            return View(returnItem);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var returnItem = await _returnRepo.GetByIdAsync(id);
            if (returnItem == null) return NotFound();
            await LoadIssuanceDropdown(returnItem.IssuanceId);
            return View(returnItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReturnItem returnItem)
        {
            if (id != returnItem.Id) return NotFound();

            ModelState.Remove("Issuance");

            if (ModelState.IsValid)
            {
                await _returnRepo.UpdateAsync(returnItem);
                TempData["Success"] = "Return record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadIssuanceDropdown(returnItem.IssuanceId);
            return View(returnItem);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var returnItem = await _returnRepo.GetByIdAsync(id);
            if (returnItem == null) return NotFound();
            returnItem.Issuance = await _issuanceRepo.GetByIdAsync(returnItem.IssuanceId);
            if (returnItem.Issuance != null)
            {
                returnItem.Issuance.ItemCopy = await _copyRepo.GetByIdAsync(returnItem.Issuance.ItemCopyId);
            }
            return View(returnItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _returnRepo.DeleteAsync(id);
            TempData["Success"] = "Return record deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadIssuanceDropdown(object selected = null)
        {
            var issuances = await _issuanceRepo.GetAllAsync();
            var activeIssuances = issuances.Where(i => i.IsActive).ToList();
            ViewBag.IssuanceList = new SelectList(activeIssuances, "Id", "Id", selected);
        }
    }
}
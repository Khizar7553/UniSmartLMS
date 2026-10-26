using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services;
using UniSmartLMS.Services.Implementations;

namespace UniSmartLMS.Controllers.SectionE_Circulation
{
    [Authorize(Roles = "Admin,Librarian,Staff")]
    public class IssuanceController : Controller
    {
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IGenericRepository<Duration> _durationRepo;
        private readonly ICirculationService _circulationService;  // ✅ ADD THIS

        public IssuanceController(
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<ItemCopy> copyRepo,
            IGenericRepository<Duration> durationRepo,
            ICirculationService circulationService)  // ✅ ADD THIS
        {
            _issuanceRepo = issuanceRepo;
            _memberRepo = memberRepo;
            _copyRepo = copyRepo;
            _durationRepo = durationRepo;
            _circulationService = circulationService;  // ✅ ADD THIS
        }

        public async Task<IActionResult> Index()
        {
            var issuances = await _issuanceRepo.GetAllAsync();
            foreach (var issuance in issuances)
            {
                issuance.LibraryMember = await _memberRepo.GetByIdAsync(issuance.LibraryMemberId);
                issuance.ItemCopy = await _copyRepo.GetByIdAsync(issuance.ItemCopyId);
                issuance.Duration = await _durationRepo.GetByIdAsync(issuance.DurationId);
            }
            return View(issuances);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction(nameof(Index));

            var results = await _issuanceRepo.SearchAsync("Id", searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(id);
            if (issuance == null) return NotFound();
            issuance.LibraryMember = await _memberRepo.GetByIdAsync(issuance.LibraryMemberId);
            issuance.ItemCopy = await _copyRepo.GetByIdAsync(issuance.ItemCopyId);
            issuance.Duration = await _durationRepo.GetByIdAsync(issuance.DurationId);
            return View(issuance);
        }

        public async Task<IActionResult> Create()
        {
            await LoadMemberDropdown();
            await LoadCopyDropdown();
            await LoadDurationDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Issuance issuance)
        {
            ModelState.Remove("LibraryMember");
            ModelState.Remove("ItemCopy");
            ModelState.Remove("Duration");

            // Validate member can borrow
            var member = await _memberRepo.GetByIdAsync(issuance.LibraryMemberId);
            if (member == null || !member.IsActive)
            {
                ModelState.AddModelError("LibraryMemberId", "Member is not active or not found.");
                await LoadMemberDropdown(issuance.LibraryMemberId);
                await LoadCopyDropdown(issuance.ItemCopyId);
                await LoadDurationDropdown(issuance.DurationId);
                return View(issuance);
            }

            // Validate copy is available
            var copy = await _copyRepo.GetByIdAsync(issuance.ItemCopyId);
            if (copy == null || !copy.IsActive)
            {
                ModelState.AddModelError("ItemCopyId", "Book copy is not available for issue.");
                await LoadMemberDropdown(issuance.LibraryMemberId);
                await LoadCopyDropdown(issuance.ItemCopyId);
                await LoadDurationDropdown(issuance.DurationId);
                return View(issuance);
            }

            // Calculate due date based on Duration
            if (issuance.DurationId > 0)
            {
                var duration = await _durationRepo.GetByIdAsync(issuance.DurationId);
                if (duration != null && int.TryParse(duration.Code, out int days))
                {
                    issuance.DurationNumber = days;
                }
                else
                {
                    issuance.DurationNumber = 14; // Default 14 days
                }
            }

            issuance.IssueDate = DateTime.Now;
            issuance.FineAccrued = 0;
            issuance.RenewalsUsed = 0;
            issuance.IsActive = true;

            if (ModelState.IsValid)
            {
                // ✅ Mark copy as issued (set IsActive = false)
                copy.IsActive = false;
                await _copyRepo.UpdateAsync(copy);

                // ✅ Add issuance
                await _issuanceRepo.AddAsync(issuance);

                var dueDate = issuance.IssueDate.AddDays(issuance.DurationNumber);
                TempData["Success"] = $"Book issued successfully! Due date: {dueDate:dd MMM yyyy}";
                return RedirectToAction(nameof(Index));
            }

            await LoadMemberDropdown(issuance.LibraryMemberId);
            await LoadCopyDropdown(issuance.ItemCopyId);
            await LoadDurationDropdown(issuance.DurationId);
            return View(issuance);
        }

        // ✅ ADD RENEW METHOD
        [HttpPost]
        public async Task<JsonResult> Renew(int id)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(id);
            if (issuance == null)
                return Json(new { success = false, message = "Loan record not found" });

            if (!issuance.IsActive)
                return Json(new { success = false, message = "This loan has already been returned" });

            if (issuance.RenewalsUsed >= 2)
                return Json(new { success = false, message = "Maximum renewals (2) reached for this book" });

            // Check if overdue
            var dueDate = issuance.IssueDate.AddDays(issuance.DurationNumber);
            if (dueDate.Date < DateTime.Now.Date)
                return Json(new { success = false, message = "Cannot renew overdue books. Please return and borrow again." });

            // Renew: add 14 more days
            issuance.RenewalsUsed++;
            issuance.DurationNumber += 14;
            await _issuanceRepo.UpdateAsync(issuance);

            var newDueDate = issuance.IssueDate.AddDays(issuance.DurationNumber);
            return Json(new { success = true, message = $"Book renewed! New due date: {newDueDate:dd MMM yyyy}" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(id);
            if (issuance == null) return NotFound();
            await LoadMemberDropdown(issuance.LibraryMemberId);
            await LoadCopyDropdown(issuance.ItemCopyId);
            await LoadDurationDropdown(issuance.DurationId);
            return View(issuance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Issuance issuance)
        {
            if (id != issuance.Id) return NotFound();

            ModelState.Remove("LibraryMember");
            ModelState.Remove("ItemCopy");
            ModelState.Remove("Duration");

            if (ModelState.IsValid)
            {
                await _issuanceRepo.UpdateAsync(issuance);
                TempData["Success"] = "Issuance updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await LoadMemberDropdown(issuance.LibraryMemberId);
            await LoadCopyDropdown(issuance.ItemCopyId);
            await LoadDurationDropdown(issuance.DurationId);
            return View(issuance);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(id);
            if (issuance == null) return NotFound();
            issuance.LibraryMember = await _memberRepo.GetByIdAsync(issuance.LibraryMemberId);
            issuance.ItemCopy = await _copyRepo.GetByIdAsync(issuance.ItemCopyId);
            issuance.Duration = await _durationRepo.GetByIdAsync(issuance.DurationId);
            return View(issuance);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _issuanceRepo.DeleteAsync(id);
            TempData["Success"] = "Issuance deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadMemberDropdown(object selected = null)
        {
            var members = await _memberRepo.GetAllAsync();
            var activeMembers = members.Where(m => m.IsActive);
            ViewBag.MemberList = new SelectList(activeMembers, "Id", "CardNo", selected);
        }

        private async Task LoadCopyDropdown(object selected = null)
        {
            var copies = await _copyRepo.GetAllAsync();
            var availableCopies = copies.Where(c => c.IsActive);
            ViewBag.CopyList = new SelectList(availableCopies, "Id", "Barcode", selected);
        }

        private async Task LoadDurationDropdown(object selected = null)
        {
            var durations = await _durationRepo.GetAllAsync();
            ViewBag.DurationList = new SelectList(durations, "Id", "Name", selected);
        }
    }
}
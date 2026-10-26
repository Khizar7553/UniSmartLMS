using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionH_Digital;

namespace UniSmartLMS.Controllers
{
    [Authorize]
    public class BorrowRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BorrowRequestController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== STUDENT/VISITOR/STAFF/FACULTY ====================

        // GET: Submit Request
        [Authorize(Roles = "Student,Visitor,Staff,Faculty")]
        public async Task<IActionResult> Create()
        {

            var copies = await _context.ItemCopies
                .Include(c => c.ItemInfo)
                .Where(c => c.IsActive)
                .ToListAsync();
            ViewBag.CopyList = new SelectList(copies, "Id", "Barcode");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Student,Visitor,Staff,Faculty")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int itemCopyId, string memberRemarks)
        {
            if (itemCopyId == 0)
            {
                TempData["Error"] = "Please select a valid book copy.";
                return RedirectToAction("Create");
            }
            var user = await _userManager.GetUserAsync(User);
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyName == user.FullName);
            if (party == null)
            {
                TempData["Error"] = "Profile not found. Contact librarian.";
                return RedirectToAction("Create");
            }

            var member = await _context.LibraryMembers.FirstOrDefaultAsync(m => m.PartyId == party.Id);
            if (member == null)
            {
                TempData["Error"] = "No library membership. Please register at library counter.";
                return RedirectToAction("Create");
            }

            var existing = await _context.BorrowRequests
                .FirstOrDefaultAsync(r => r.MemberId == member.Id && r.ItemCopyId == itemCopyId && r.Status == "Pending");
            if (existing != null)
            {
                TempData["Error"] = "You already have a pending request for this book.";
                return RedirectToAction("MyRequests");
            }

            var request = new BorrowRequest
            {
                MemberId = member.Id,
                ItemCopyId = itemCopyId,
                RequestDate = DateTime.Now,
                Status = "Pending",
                MemberRemarks = memberRemarks ?? "",
                IsActive = true
            };

            _context.BorrowRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Request submitted! Visit library counter with your ID card to complete borrowing.";
            return RedirectToAction("MyRequests");
        }

        // GET: My Requests
        [Authorize(Roles = "Student,Visitor,Staff,Faculty")]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyName == user.FullName);
            if (party == null) return View(new List<BorrowRequest>());

            var member = await _context.LibraryMembers.FirstOrDefaultAsync(m => m.PartyId == party.Id);
            if (member == null) return View(new List<BorrowRequest>());

            var requests = await _context.BorrowRequests
                .Include(r => r.ItemCopy)
                    .ThenInclude(c => c.ItemInfo)
                .Where(r => r.MemberId == member.Id)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // ==================== LIBRARIAN/ADMIN ====================

        // GET: All Pending Requests
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Index(string status = "Pending")
        {
            var requests = await _context.BorrowRequests
                .Include(r => r.LibraryMember)
                    .ThenInclude(m => m.Party)
                .Include(r => r.ItemCopy)
                    .ThenInclude(c => c.ItemInfo)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewBag.PendingCount = await _context.BorrowRequests.CountAsync(r => r.Status == "Pending");
            ViewBag.ApprovedCount = await _context.BorrowRequests.CountAsync(r => r.Status == "Approved");
            ViewBag.RejectedCount = await _context.BorrowRequests.CountAsync(r => r.Status == "Rejected");
            ViewBag.CurrentStatus = status;

            return View(requests);
        }

        // POST: Approve (when member visits physically)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string adminRemarks)
        {
            var request = await _context.BorrowRequests
                .Include(r => r.ItemCopy)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            var staff = await _context.Employees.FirstOrDefaultAsync(e => e.Name == user.FullName);

            request.Status = "Approved";
            request.AdminRemarks = adminRemarks ?? "Member verified physically.";
            request.ApprovedByStaffId = staff?.Id;
            request.ApprovedDate = DateTime.Now;
            request.UpdatedAt = DateTime.Now;

            // Create issuance record automatically
            var issuance = new Issuance
            {
                LibraryMemberId = request.MemberId,
                ItemCopyId = request.ItemCopyId,
                DurationId = 1,
                IssueDate = DateTime.Now,
                DurationNumber = 14,
                FineAccrued = 0,
                RenewalsUsed = 0,
                IsActive = true
            };
            _context.Issuances.Add(issuance);

            // Mark copy as issued
            var copy = await _context.ItemCopies.FindAsync(request.ItemCopyId);
            if (copy != null) copy.IsActive = false;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Request approved! Book issued to member.";
            return RedirectToAction("Index");
        }

        // POST: Reject
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string adminRemarks)
        {
            var request = await _context.BorrowRequests.FindAsync(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            request.Status = "Rejected";
            request.AdminRemarks = adminRemarks ?? "Request rejected.";
            request.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Request rejected.";
            return RedirectToAction("Index");
        }
    }
}


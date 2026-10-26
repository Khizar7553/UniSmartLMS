using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers
{
    [Authorize(Roles = "Admin,Librarian")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<ItemInfo> _bookRepo;

        public ReportController(
            ApplicationDbContext context,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<ItemInfo> bookRepo)
        {
            _context = context;
            _issuanceRepo = issuanceRepo;
            _fineRepo = fineRepo;
            _memberRepo = memberRepo;
            _bookRepo = bookRepo;
        }

        // GET: Report/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: Report/PopularBooks
        public async Task<IActionResult> PopularBooks(DateTime? fromDate, DateTime? toDate, int top = 10)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-6);
            var to = toDate ?? DateTime.Now;

            var popularBooks = await _context.Issuances
                .Include(i => i.ItemCopy)
                .ThenInclude(c => c.ItemInfo)
                .Where(i => i.IssueDate >= from && i.IssueDate <= to)
                .GroupBy(i => i.ItemCopy.ItemInfoId)
                .Select(g => new PopularBookViewModel
                {
                    BookId = g.Key,
                    Title = g.FirstOrDefault().ItemCopy.ItemInfo.Title,
                    ISBN = g.FirstOrDefault().ItemCopy.ItemInfo.ISBN,
                    Author = g.FirstOrDefault().ItemCopy.ItemInfo.AuthorItemInfos.FirstOrDefault().Author.Name,
                    TotalIssued = g.Count(),
                    AvailableCopies = _context.ItemCopies.Count(c => c.ItemInfoId == g.Key && c.IsActive),
                    TotalCopies = _context.ItemCopies.Count(c => c.ItemInfoId == g.Key)
                })
                .OrderByDescending(x => x.TotalIssued)
                .Take(top)
                .ToListAsync();

            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");
            ViewBag.TopCount = top;

            return View(popularBooks);
        }

        // GET: Report/MemberActivity
        public async Task<IActionResult> MemberActivity(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;

            var topMembers = await _context.Issuances
                .Include(i => i.LibraryMember)
                .ThenInclude(m => m.Party)
                .Where(i => i.IssueDate >= from && i.IssueDate <= to)
                .GroupBy(i => i.LibraryMemberId)
                .Select(g => new MemberActivityViewModel
                {
                    MemberId = g.Key,
                    CardNo = g.FirstOrDefault().LibraryMember.CardNo,
                    MemberName = g.FirstOrDefault().LibraryMember.Party.PartyName,
                    TotalBooksBorrowed = g.Count(),
                    TotalFines = _context.UserFines.Where(f => f.LibraryMemberId == g.Key && f.IsActive).Sum(f => f.FineAccrued),
                    LastActiveDate = g.Max(i => i.IssueDate)
                })
                .OrderByDescending(x => x.TotalBooksBorrowed)
                .Take(20)
                .ToListAsync();

            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");

            return View(topMembers);
        }

        // GET: Report/FineCollection
        public async Task<IActionResult> FineCollection(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-3);
            var to = toDate ?? DateTime.Now;

            var fines = await _context.UserFines
                .Include(f => f.LibraryMember)
                .ThenInclude(m => m.Party)
                .Where(f => f.CreatedAt >= from && f.CreatedAt <= to)
                .OrderByDescending(f => f.FineAccrued)
                .ToListAsync();

            var totalFine = fines.Sum(f => f.FineAccrued);
            var paidFine = fines.Where(f => !f.IsActive).Sum(f => f.FineAccrued);
            var pendingFine = fines.Where(f => f.IsActive).Sum(f => f.FineAccrued);

            ViewBag.TotalFine = totalFine;
            ViewBag.PaidFine = paidFine;
            ViewBag.PendingFine = pendingFine;
            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");

            return View(fines);
        }

        // GET: Report/DepartmentUsage
        // GET: Report/DepartmentUsage - SIMPLIFIED VERSION
        // GET: Report/DepartmentUsage - FIXED FOR YOUR MODEL
        // GET: Report/DepartmentUsage - SIMPLIFIED (No DegreePrograms)
        public async Task<IActionResult> DepartmentUsage(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-6);
            var to = toDate ?? DateTime.Now;

            // Get all issuances with member and party info
            var issuances = await _context.Issuances
                .Include(i => i.LibraryMember)
                .ThenInclude(m => m.Party)
                .Where(i => i.IssueDate >= from && i.IssueDate <= to)
                .ToListAsync();

            // Get all students with party info
            var students = await _context.Students
                .Include(s => s.Party)
                .ToListAsync();

            // Map party to student (to identify which members are students)
            var partyIsStudentMap = students.Select(s => s.PartyId).ToHashSet();

            // Calculate statistics
            var stats = new Dictionary<string, (int Issuances, HashSet<int> Members)>();

            foreach (var issuance in issuances)
            {
                var partyId = issuance.LibraryMember?.PartyId ?? 0;

                // Determine user type/category
                string category = "Unknown";
                if (partyIsStudentMap.Contains(partyId))
                {
                    category = "Student";
                }
                else
                {
                    category = "Faculty/Staff/Public";
                }

                if (!stats.ContainsKey(category))
                {
                    stats[category] = (0, new HashSet<int>());
                }

                var stat = stats[category];
                stat.Issuances++;
                stat.Members.Add(issuance.LibraryMemberId);
                stats[category] = stat;
            }

            var result = stats.Select(s => new DepartmentUsageViewModel
            {
                DepartmentName = s.Key,
                TotalIssuances = s.Value.Issuances,
                UniqueMembers = s.Value.Members.Count
            })
            .OrderByDescending(x => x.TotalIssuances)
            .ToList();

            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");

            return View(result);
        }


        // GET: Report/ExportPopularBooksExcel
        public async Task<IActionResult> ExportPopularBooksExcel(DateTime? fromDate, DateTime? toDate, int top = 10)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-6);
            var to = toDate ?? DateTime.Now;

            var popularBooks = await _context.Issuances
                .Include(i => i.ItemCopy)
                .ThenInclude(c => c.ItemInfo)
                .Where(i => i.IssueDate >= from && i.IssueDate <= to)
                .GroupBy(i => i.ItemCopy.ItemInfoId)
                .Select(g => new
                {
                    Title = g.FirstOrDefault().ItemCopy.ItemInfo.Title,
                    ISBN = g.FirstOrDefault().ItemCopy.ItemInfo.ISBN,
                    TotalIssued = g.Count()
                })
                .OrderByDescending(x => x.TotalIssued)
                .Take(top)
                .ToListAsync();

            // Build CSV
            var csv = "Title,ISBN,Total Issued\n";
            foreach (var book in popularBooks)
            {
                csv += $"\"{book.Title}\",{book.ISBN},{book.TotalIssued}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"PopularBooks_{DateTime.Now:yyyyMMdd}.csv");
        }
    }

    // ViewModels
    public class PopularBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string Author { get; set; }
        public int TotalIssued { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
    }

    public class MemberActivityViewModel
    {
        public int MemberId { get; set; }
        public string CardNo { get; set; }
        public string MemberName { get; set; }
        public int TotalBooksBorrowed { get; set; }
        public decimal TotalFines { get; set; }
        public DateTime LastActiveDate { get; set; }
    }

    public class DepartmentUsageViewModel
    {
        public string DepartmentName { get; set; }
        public int TotalIssuances { get; set; }
        public int UniqueMembers { get; set; }
    }
}
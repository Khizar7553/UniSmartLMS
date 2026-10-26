using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models.SectionC_Catalog;

namespace UniSmartLMS.Controllers
{
    [Authorize(Roles = "Visitor")]
    public class VisitorCatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitorCatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Books(string searchTerm)
        {
            var query = _context.ItemCopies
                .Include(c => c.ItemInfo)
                    .ThenInclude(i => i.PublisherInfo)
                .Include(c => c.ItemInfo)
                    .ThenInclude(i => i.AuthorItemInfos)
                    .ThenInclude(a => a.Author)
                .Include(c => c.LibraryBranch)
                .Include(c => c.LibraryRack)
                    .ThenInclude(r => r.LibraryItemSection)
                .Include(c => c.LibraryShelf)
                .Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.ItemInfo.Title.Contains(searchTerm) ||
                                         c.ItemInfo.ISBN.Contains(searchTerm));
            }

            var copies = await query.ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            return View(copies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var copy = await _context.ItemCopies
                .Include(c => c.ItemInfo)
                    .ThenInclude(i => i.PublisherInfo)
                .Include(c => c.ItemInfo)
                    .ThenInclude(i => i.AuthorItemInfos)
                    .ThenInclude(a => a.Author)
                .Include(c => c.LibraryBranch)
                .Include(c => c.LibraryRack)
                    .ThenInclude(r => r.LibraryItemSection)
                .Include(c => c.LibraryShelf)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (copy == null) return NotFound();
            return View(copy);
        }
    }
}
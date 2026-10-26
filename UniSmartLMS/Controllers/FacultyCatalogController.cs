using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionH_Digital;

namespace UniSmartLMS.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyCatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacultyCatalogController(ApplicationDbContext context)
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
                                         c.ItemInfo.ISBN.Contains(searchTerm) ||
                                         c.ItemInfo.AuthorItemInfos.Any(a => a.Author.Name.Contains(searchTerm)));
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

        public async Task<IActionResult> ResearchResources()
        {
            var digitalResources = await _context.DigitalResources
                .Include(d => d.ItemInfo)
                .ToListAsync();
            return View(digitalResources);
        }
    }
}
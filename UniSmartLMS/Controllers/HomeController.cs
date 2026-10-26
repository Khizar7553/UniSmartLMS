using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UniSmartLMS.Data;
using UniSmartLMS.Models;

namespace UniSmartLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ GET: Home/GetStatistics (For homepage stats)
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalBooks = await _context.ItemInfos.CountAsync();
                var totalMembers = await _context.LibraryMembers.CountAsync(m => m.IsActive);
                var totalStaff = await _context.Employees.CountAsync();

                return Json(new
                {
                    totalBooks,
                    totalMembers,
                    totalStaff
                });
            }
            catch (Exception ex)
            {
                return Json(new { totalBooks = 0, totalMembers = 0, totalStaff = 0, error = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
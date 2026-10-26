using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models;
using UniSmartLMS.Services;

namespace UniSmartLMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStudentDashboardService _studentService;
        private readonly IFacultyDashboardService _facultyService;
        private readonly ILibrarianDashboardService _librarianService;
        private readonly IAdminDashboardService _adminService;
        private readonly IVisitorDashboardService _visitorService;

        public DashboardController(
            UserManager<AppUser> userManager,
            IStudentDashboardService studentService,
            IFacultyDashboardService facultyService,
            ILibrarianDashboardService librarianService,
            IAdminDashboardService adminService,
            IVisitorDashboardService visitorService)
        {
            _userManager = userManager;
            _studentService = studentService;
            _facultyService = facultyService;
            _librarianService = librarianService;
            _adminService = adminService;
            _visitorService = visitorService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            switch (role)
            {
                case "Student":
                    var studentData = await _studentService.GetDashboardDataAsync(user);
                    return View("StudentDashboard", studentData);

                case "Faculty":
                    var facultyData = await _facultyService.GetDashboardDataAsync(user);
                    return View("FacultyDashboard", facultyData);

                case "Staff":
                    var staffData = await _facultyService.GetDashboardDataAsync(user);
                    return View("StaffDashboard", staffData);

                case "Librarian":
                    var librarianData = await _librarianService.GetDashboardDataAsync();
                    return View("LibrarianDashboard", librarianData);

                case "Admin":
                    var adminData = await _adminService.GetDashboardDataAsync();
                    return View("AdminDashboard", adminData);

                case "Visitor":
                    var visitorData = await _visitorService.GetDashboardDataAsync();
                    return View("VisitorDashboard", visitorData);

                default:
                    return View("GuestDashboard");
            }
        }
    }
}
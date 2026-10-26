using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminDashboardService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync(AppUser user = null)
        {
            var viewModel = new AdminDashboardViewModel();

            viewModel.TotalUniversities = await _context.Universities.CountAsync();
            viewModel.TotalCampuses = await _context.Campuses.CountAsync();
            viewModel.TotalDepartments = await _context.Departments.CountAsync();
            viewModel.TotalBooks = await _context.ItemInfos.CountAsync();
            viewModel.TotalCopies = await _context.ItemCopies.CountAsync();
            viewModel.TotalMembers = await _context.LibraryMembers.CountAsync();
            viewModel.TotalUsers = await _userManager.Users.CountAsync();
            viewModel.ActiveLoans = await _context.Issuances.CountAsync(i => i.IsActive);
            viewModel.TodaysIssuances = await _context.Issuances.CountAsync(i => i.IssueDate.Date == DateTime.Today);
            viewModel.TotalFineAmount = await _context.UserFines.SumAsync(f => (decimal?)f.FineAccrued) ?? 0;
            viewModel.TotalReservations = await _context.ReserveItems.CountAsync(r => r.IsActive);

            var users = await _userManager.Users.ToListAsync();
            viewModel.StudentCount = 0;
            viewModel.FacultyCount = 0;
            viewModel.StaffCount = 0;
            viewModel.LibrarianCount = 0;
            viewModel.AdminCount = 0;
            viewModel.VisitorCount = 0;

            foreach (var appUser in users)
            {
                var roles = await _userManager.GetRolesAsync(appUser);
                var role = roles.FirstOrDefault();
                switch (role)
                {
                    case "Student": viewModel.StudentCount++; break;
                    case "Faculty": viewModel.FacultyCount++; break;
                    case "Staff": viewModel.StaffCount++; break;
                    case "Librarian": viewModel.LibrarianCount++; break;
                    case "Admin": viewModel.AdminCount++; break;
                    case "Visitor": viewModel.VisitorCount++; break;
                }
            }

            return viewModel;
        }
    }
}
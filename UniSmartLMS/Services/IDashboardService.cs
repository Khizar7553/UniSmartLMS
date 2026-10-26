using UniSmartLMS.Models;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    // Generic interface for all dashboards
    public interface IDashboardService<T> where T : class
    {
        Task<T> GetDashboardDataAsync(AppUser user = null);
    }

    // Specific interfaces (optional - for clarity)
    public interface IStudentDashboardService : IDashboardService<StudentDashboardViewModel> { }
    public interface IFacultyDashboardService : IDashboardService<FacultyDashboardViewModel> { }
    public interface IStaffDashboardService : IDashboardService<StaffDashboardViewModel> { }
    public interface ILibrarianDashboardService : IDashboardService<LibrarianDashboardViewModel> { }
    public interface IAdminDashboardService : IDashboardService<AdminDashboardViewModel> { }
    public interface IVisitorDashboardService : IDashboardService<VisitorDashboardViewModel> { }
}

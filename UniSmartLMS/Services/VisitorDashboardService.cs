using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    public class VisitorDashboardService : IVisitorDashboardService
    {
        private readonly ApplicationDbContext _context;

        public VisitorDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VisitorDashboardViewModel> GetDashboardDataAsync(AppUser user = null)
        {
            var viewModel = new VisitorDashboardViewModel();

            viewModel.TotalBooks = await _context.ItemInfos.CountAsync();
            viewModel.TotalDigitalResources = await _context.DigitalResources.CountAsync();
            viewModel.TotalAuthors = await _context.AuthorInfos.CountAsync();

            viewModel.LibraryTimings = "Monday - Saturday: 8:00 AM - 8:00 PM";
            viewModel.LibraryPhone = "+92 42 1234 5678";
            viewModel.LibraryEmail = "library@unismartlms.edu.pk";
            viewModel.LibraryAddress = "Main Library, Ground Floor, MNS-UAM";

            return viewModel;
        }
    }
}
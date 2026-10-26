using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    public class LibrarianDashboardService : ILibrarianDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;

        public LibrarianDashboardService(
            ApplicationDbContext context,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<LibraryMember> memberRepo)
        {
            _context = context;
            _issuanceRepo = issuanceRepo;
            _fineRepo = fineRepo;
            _memberRepo = memberRepo;
        }

        public async Task<LibrarianDashboardViewModel> GetDashboardDataAsync(AppUser user = null)
        {
            var viewModel = new LibrarianDashboardViewModel();
            var today = DateTime.Today;

            var allIssuances = await _issuanceRepo.GetAllAsync();

            viewModel.TodaysIssuances = allIssuances.Count(i => i.IssueDate.Date == today);

            viewModel.OverdueLoans = 0;
            foreach (var i in allIssuances)
            {
                if (i.IsActive && i.IssueDate.AddDays(i.DurationNumber).Date < today)
                    viewModel.OverdueLoans++;
            }

            var allFines = await _fineRepo.GetAllAsync();
            var pendingFines = allFines.Where(f => f.IsActive && f.FineAccrued > 0);
            viewModel.PendingFinesCount = pendingFines.Count();
            viewModel.PendingFinesAmount = pendingFines.Sum(f => f.FineAccrued);

            viewModel.ActiveMembers = (await _memberRepo.GetAllAsync()).Count(m => m.IsActive);
            viewModel.TotalBooks = await _context.ItemInfos.CountAsync();
            viewModel.TotalCopies = await _context.ItemCopies.CountAsync();

            return viewModel;
        }
    }
}
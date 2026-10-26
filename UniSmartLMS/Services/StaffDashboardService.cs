using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Models.ViewModels;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    public class StaffDashboardService : IStaffDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<ReserveItem> _reserveRepo;
        private readonly IGenericRepository<ItemInfo> _bookRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;

        public StaffDashboardService(
            ApplicationDbContext context,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<ReserveItem> reserveRepo,
            IGenericRepository<ItemInfo> bookRepo,
            IGenericRepository<LibraryMember> memberRepo)
        {
            _context = context;
            _issuanceRepo = issuanceRepo;
            _fineRepo = fineRepo;
            _reserveRepo = reserveRepo;
            _bookRepo = bookRepo;
            _memberRepo = memberRepo;
        }

        public async Task<StaffDashboardViewModel> GetDashboardDataAsync(AppUser user)
        {
            var viewModel = new StaffDashboardViewModel();

            // Get Party and LibraryMember
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyName == user.Email);
            var libraryMember = party != null ? await _context.LibraryMembers.FirstOrDefaultAsync(m => m.PartyId == party.Id) : null;

            viewModel.MemberName = party?.PartyName ?? user.FullName ?? user.Email;
            viewModel.CardNumber = libraryMember?.CardNo ?? "Not Issued";
            viewModel.HasMembership = libraryMember != null;
            viewModel.MaxLoanLimit = 8;
            viewModel.LoanDays = 21;

            if (libraryMember == null)
            {
                viewModel.Message = "No library membership found. Please contact librarian.";
                return viewModel;
            }

            // Current Loans
            var allIssuances = await _issuanceRepo.GetAllAsync();
            var currentLoans = allIssuances
                .Where(l => l.LibraryMemberId == libraryMember.Id && l.IsActive)
                .ToList();

            foreach (var loan in currentLoans)
            {
                loan.ItemCopy = await _context.ItemCopies.FindAsync(loan.ItemCopyId);
                if (loan.ItemCopy?.ItemInfoId > 0)
                {
                    loan.ItemCopy.ItemInfo = await _context.ItemInfos.FindAsync(loan.ItemCopy.ItemInfoId);
                }
            }

            viewModel.CurrentLoans = currentLoans;
            viewModel.CurrentLoansCount = currentLoans.Count;

            // Overdue Count
            var today = DateTime.Today;
            viewModel.OverdueCount = currentLoans.Count(l => l.IssueDate.AddDays(l.DurationNumber).Date < today);

            // Fines
            var allFines = await _fineRepo.GetAllAsync();
            viewModel.Fines = allFines
                .Where(f => f.LibraryMemberId == libraryMember.Id && f.FineAccrued > 0)
                .ToList();
            viewModel.TotalFine = viewModel.Fines.Sum(f => f.FineAccrued);

            // Digital Resources Access
            viewModel.DigitalResourcesCount = await _context.DigitalResources.CountAsync();
            viewModel.RecentDigitalAccess = await _context.OnlineAccessLogs
                .Where(l => l.LibraryMemberId == libraryMember.Id)
                .OrderByDescending(l => l.AccessDate)
                .Take(5)
                .ToListAsync();

            return viewModel;
        }
    }
}
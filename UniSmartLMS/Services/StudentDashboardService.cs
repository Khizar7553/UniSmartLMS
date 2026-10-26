using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<ReserveItem> _reserveRepo;
        private readonly IGenericRepository<ItemInfo> _bookRepo;

        public StudentDashboardService(
            ApplicationDbContext context,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<ReserveItem> reserveRepo,
            IGenericRepository<ItemInfo> bookRepo)
        {
            _context = context;
            _issuanceRepo = issuanceRepo;
            _fineRepo = fineRepo;
            _reserveRepo = reserveRepo;
            _bookRepo = bookRepo;
        }

        public async Task<StudentDashboardViewModel> GetDashboardDataAsync(AppUser user = null)
        {
            var viewModel = new StudentDashboardViewModel();

            if (user == null) return viewModel;

            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyName == user.Email);
            var libraryMember = party != null ? await _context.LibraryMembers.FirstOrDefaultAsync(m => m.PartyId == party.Id) : null;

            viewModel.MemberName = party?.PartyName ?? user.FullName ?? user.Email;
            viewModel.CardNumber = libraryMember?.CardNo ?? "Not Issued";
            viewModel.HasMembership = libraryMember != null;

            if (libraryMember == null)
            {
                viewModel.Message = "No library membership found. Please contact librarian.";
                return viewModel;
            }

            var allIssuances = await _issuanceRepo.GetAllAsync();
            var currentLoans = allIssuances.Where(l => l.LibraryMemberId == libraryMember.Id && l.IsActive).ToList();

            foreach (var loan in currentLoans)
            {
                loan.ItemCopy = await _context.ItemCopies.FindAsync(loan.ItemCopyId);
                if (loan.ItemCopy?.ItemInfoId > 0)
                    loan.ItemCopy.ItemInfo = await _context.ItemInfos.FindAsync(loan.ItemCopy.ItemInfoId);
            }

            viewModel.CurrentLoans = currentLoans;
            viewModel.CurrentLoansCount = currentLoans.Count;

            var today = DateTime.Today;
            viewModel.OverdueCount = currentLoans.Count(l => l.IssueDate.AddDays(l.DurationNumber).Date < today);

            viewModel.LoanHistory = allIssuances.Where(l => l.LibraryMemberId == libraryMember.Id && !l.IsActive).ToList();
            viewModel.TotalBooksRead = viewModel.LoanHistory.Count();

            var allFines = await _fineRepo.GetAllAsync();
            viewModel.Fines = allFines.Where(f => f.LibraryMemberId == libraryMember.Id && f.FineAccrued > 0).ToList();
            viewModel.TotalFine = viewModel.Fines.Sum(f => f.FineAccrued);

            var allReservations = await _reserveRepo.GetAllAsync();
            viewModel.Reservations = allReservations.Where(r => r.LibraryMemberId == libraryMember.Id && r.IsActive).ToList();

            foreach (var res in viewModel.Reservations)
                res.ItemInfo = await _bookRepo.GetByIdAsync(res.ItemInfoId);

            viewModel.RecommendedBooks = (await _bookRepo.GetAllAsync()).Take(6).ToList();

            return viewModel;
        }
    }
}
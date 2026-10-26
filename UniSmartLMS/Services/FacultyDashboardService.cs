using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionH_Digital;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.ViewModels;

namespace UniSmartLMS.Services
{
    public class FacultyDashboardService : IFacultyDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<ReserveItem> _reserveRepo;
        private readonly IGenericRepository<ItemInfo> _bookRepo;
        private readonly IGenericRepository<DigitalResource> _digitalRepo;

        public FacultyDashboardService(
            ApplicationDbContext context,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<ReserveItem> reserveRepo,
            IGenericRepository<ItemInfo> bookRepo,
            IGenericRepository<DigitalResource> digitalRepo)
        {
            _context = context;
            _issuanceRepo = issuanceRepo;
            _reserveRepo = reserveRepo;
            _bookRepo = bookRepo;
            _digitalRepo = digitalRepo;
        }

        public async Task<FacultyDashboardViewModel> GetDashboardDataAsync(AppUser user = null)
        {
            var viewModel = new FacultyDashboardViewModel();

            if (user == null) return viewModel;

            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyName == user.Email);
            var libraryMember = party != null ? await _context.LibraryMembers.FirstOrDefaultAsync(m => m.PartyId == party.Id) : null;

            viewModel.MemberName = party?.PartyName ?? user.FullName ?? user.Email;
            viewModel.MemberType = "Faculty";
            viewModel.MaxLoanLimit = 15;
            viewModel.LoanDays = 60;
            viewModel.CardNumber = libraryMember?.CardNo ?? "Not Issued";

            if (libraryMember != null)
            {
                var allIssuances = await _issuanceRepo.GetAllAsync();
                viewModel.CurrentLoansCount = allIssuances.Count(l => l.LibraryMemberId == libraryMember.Id && l.IsActive);
                viewModel.CourseReservesCount = (await _reserveRepo.GetAllAsync()).Count(r => r.LibraryMemberId == libraryMember.Id && r.IsActive);
            }

            // ✅ FIXED - Removed ProfessorPublications
            viewModel.ResearchPapersCount = await _context.ItemInfos.CountAsync();
            viewModel.EBooksCount = (await _digitalRepo.GetAllAsync()).Count();
            viewModel.JournalsCount = 45;
            viewModel.DatabasesCount = 12;
            viewModel.MyPublicationsCount = 0;
            viewModel.PendingRecommendations = 3;

            return viewModel;
        }
    }
}
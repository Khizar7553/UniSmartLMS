using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Services
{
    public class FineService : IFineService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<CirculationRule> _ruleRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<FineDefinition> _definitionRepo;

        public FineService(
            ApplicationDbContext context,
            IGenericRepository<CirculationRule> ruleRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<FineDefinition> definitionRepo)
        {
            _context = context;
            _ruleRepo = ruleRepo;
            _fineRepo = fineRepo;
            _issuanceRepo = issuanceRepo;
            _definitionRepo = definitionRepo;
        }

        public async Task<decimal> CalculateFineAsync(Issuance issuance, DateTime returnDate)
        {
            // Calculate due date
            var dueDate = issuance.IssueDate.AddDays(issuance.DurationNumber);

            // If returned on or before due date, no fine
            if (returnDate.Date <= dueDate.Date)
                return 0;

            // Get circulation rule for this member
            var member = await _context.LibraryMembers
                .Include(m => m.Party)
                .FirstOrDefaultAsync(m => m.Id == issuance.LibraryMemberId);

            if (member == null) return 0;

            // Check for quota override
            var overrideRule = await _context.MemberQuotaOverrides
                .FirstOrDefaultAsync(q => q.LibraryMemberId == member.Id && q.IsActive);

            decimal finePerDay;
            decimal maxFinePerItem;

            if (overrideRule != null)
            {
                finePerDay = overrideRule.OverdueFinePerDay;
                maxFinePerItem = overrideRule.MaxFinePerItem;
            }
            else
            {
                // Get default circulation rule
                var rule = await _ruleRepo.GetAllAsync();
                var defaultRule = rule.FirstOrDefault(r => r.PartyId == member.PartyId);

                finePerDay = defaultRule?.OverdueFinePerDay ?? 5;
                maxFinePerItem = defaultRule?.MaxFinePerItem ?? 500;
            }

            // Calculate days late
            var daysLate = (returnDate.Date - dueDate.Date).Days;

            // Calculate fine
            var fine = daysLate * finePerDay;

            // Apply maximum cap
            return Math.Min(fine, maxFinePerItem);
        }

        public async Task<UserFine> ApplyFineAsync(Issuance issuance, decimal fineAmount)
        {
            if (fineAmount <= 0) return null;

            // Get or create fine definition
            var fineDefinition = await _definitionRepo.GetAllAsync();
            var definition = fineDefinition.FirstOrDefault();

            if (definition == null)
            {
                definition = new FineDefinition
                {
                    FineConditionId = 1,
                    ItemCopyId = issuance.ItemCopyId,
                    FineCategoryId = 1,
                    IsActive = true
                };
                await _definitionRepo.AddAsync(definition);
            }

            // Create user fine
            var userFine = new UserFine
            {
                FineAllocationId = 0,
                LibraryMemberId = issuance.LibraryMemberId,
                FineDefinitionId = definition.Id,
                FineAccrued = fineAmount,
                TaxPercentagePerDay = 0,
                ItemCopyId = issuance.ItemCopyId,
                IsActive = true
            };

            var result = await _fineRepo.AddAsync(userFine);

            // Update issuance with fine amount
            issuance.FineAccrued = fineAmount;
            await _issuanceRepo.UpdateAsync(issuance);

            return result;
        }

        public async Task<decimal> GetMemberTotalFineAsync(int libraryMemberId)
        {
            var fines = await _fineRepo.GetAllAsync();
            return fines
                .Where(f => f.LibraryMemberId == libraryMemberId && f.IsActive)
                .Sum(f => f.FineAccrued);
        }

        public async Task<bool> HasOverdueBooksAsync(int libraryMemberId)
        {
            var issuances = await _issuanceRepo.GetAllAsync();
            var today = DateTime.Today;

            return issuances.Any(i => i.LibraryMemberId == libraryMemberId &&
                                       i.IsActive &&
                                       i.IssueDate.AddDays(i.DurationNumber).Date < today);
        }

        public async Task<List<Issuance>> GetOverdueLoansAsync(int libraryMemberId)
        {
            var issuances = await _issuanceRepo.GetAllAsync();
            var today = DateTime.Today;

            return issuances
                .Where(i => i.LibraryMemberId == libraryMemberId &&
                           i.IsActive &&
                           i.IssueDate.AddDays(i.DurationNumber).Date < today)
                .ToList();
        }
    }
}
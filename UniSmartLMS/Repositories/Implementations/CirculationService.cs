using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services.Interfaces;

namespace UniSmartLMS.Services.Implementations
{
    public class CirculationService : ICirculationService
    {
        private readonly IGenericRepository<Issuance> _issuanceRepo;
        private readonly IGenericRepository<ReturnItem> _returnRepo;
        private readonly IGenericRepository<ReserveItem> _reserveRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;
        private readonly IGenericRepository<FineTransaction> _transactionRepo;
        private readonly IGenericRepository<CirculationRule> _ruleRepo;
        private readonly IGenericRepository<ItemCopy> _copyRepo;
        private readonly IGenericRepository<MemberBlock> _blockRepo;

        public CirculationService(
            IGenericRepository<Issuance> issuanceRepo,
            IGenericRepository<ReturnItem> returnRepo,
            IGenericRepository<ReserveItem> reserveRepo,
            IGenericRepository<UserFine> fineRepo,
            IGenericRepository<FineTransaction> transactionRepo,
            IGenericRepository<CirculationRule> ruleRepo,
            IGenericRepository<ItemCopy> copyRepo,
            IGenericRepository<MemberBlock> blockRepo)
        {
            _issuanceRepo = issuanceRepo;
            _returnRepo = returnRepo;
            _reserveRepo = reserveRepo;
            _fineRepo = fineRepo;
            _transactionRepo = transactionRepo;
            _ruleRepo = ruleRepo;
            _copyRepo = copyRepo;
            _blockRepo = blockRepo;
        }

        public async Task<Issuance> IssueBookAsync(int libraryMemberId, int itemCopyId, int durationId)
        {
            // Get circulation rules for member type
            var rule = (await _ruleRepo.GetAllAsync()).FirstOrDefault();
            var dueDate = DateTime.Now.AddDays(rule?.LoanDays ?? 14);

            var issuance = new Issuance
            {
                LibraryMemberId = libraryMemberId,
                ItemCopyId = itemCopyId,
                DurationId = durationId,
                IssueDate = DateTime.Now,
                DurationNumber = rule?.LoanDays ?? 14,
                FineAccrued = 0,
                RenewalsUsed = 0,
                IsActive = true
            };

            var newIssuance = await _issuanceRepo.AddAsync(issuance);

            // Update copy status
            var copy = await _copyRepo.GetByIdAsync(itemCopyId);
            if (copy != null) copy.Status = "Issued";
            await _copyRepo.UpdateAsync(copy);

            return newIssuance;
        }

        public async Task<ReturnItem> ReturnBookAsync(int issuanceId, decimal damageFine = 0)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(issuanceId);
            if (issuance == null) throw new Exception("Issuance not found");

            var rule = (await _ruleRepo.GetAllAsync()).FirstOrDefault();
            var daysLate = (DateTime.Now - issuance.DueDate).Days;
            var fineAmount = 0m;

            if (daysLate > 0)
            {
                fineAmount = daysLate * (rule?.OverdueFinePerDay ?? 5);
                fineAmount = Math.Min(fineAmount, rule?.MaxFinePerItem ?? 500);
                fineAmount += damageFine;

                // Auto-create UserFine
                await _fineRepo.AddAsync(new UserFine
                {
                    LibraryMemberId = issuance.LibraryMemberId,
                    FineDefinitionId = 1,
                    FineAccrued = fineAmount,
                    IsActive = true
                });
            }

            var returnItem = new ReturnItem
            {
                IssuanceId = issuanceId,
                ReturnDate = DateTime.Now,
                IsActive = true
            };
            var newReturn = await _returnRepo.AddAsync(returnItem);

            // Close issuance
            issuance.IsActive = false;
            await _issuanceRepo.UpdateAsync(issuance);

            // Update copy status back to available
            var copy = await _copyRepo.GetByIdAsync(issuance.ItemCopyId);
            if (copy != null) copy.Status = "Available";
            await _copyRepo.UpdateAsync(copy);

            // Check for reservations
            var reservation = (await _reserveRepo.GetAllAsync())
                .FirstOrDefault(r => r.ItemInfoId == copy?.ItemInfoId && r.IsActive);
            if (reservation != null)
            {
                reservation.IsAvailable = true;
                await _reserveRepo.UpdateAsync(reservation);
            }

            return newReturn;
        }

        public async Task<ReserveItem> ReserveBookAsync(int libraryMemberId, int itemInfoId, int holdDays = 7)
        {
            var existing = (await _reserveRepo.GetAllAsync())
                .FirstOrDefault(r => r.LibraryMemberId == libraryMemberId && r.ItemInfoId == itemInfoId && r.IsActive);
            if (existing != null) throw new Exception("Book already reserved");

            var reservation = new ReserveItem
            {
                ItemInfoId = itemInfoId,
                LibraryMemberId = libraryMemberId,
                DurationId = 1,
                NumberDuration = holdDays.ToString(),
                IsAvailable = false,
                ExpiryDate = DateTime.Now.AddDays(holdDays),
                IsActive = true
            };

            return await _reserveRepo.AddAsync(reservation);
        }

        public async Task<decimal> CalculateFineAsync(int issuanceId)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(issuanceId);
            if (issuance == null || !issuance.IsActive) return 0;

            var rule = (await _ruleRepo.GetAllAsync()).FirstOrDefault();
            var daysLate = (DateTime.Now - issuance.DueDate).Days;
            if (daysLate <= 0) return 0;

            var fine = daysLate * (rule?.OverdueFinePerDay ?? 5);
            return Math.Min(fine, rule?.MaxFinePerItem ?? 500);
        }

        public async Task<UserFine> PayFineAsync(int userFineId, decimal amount, string paymentRef)
        {
            var fine = await _fineRepo.GetByIdAsync(userFineId);
            if (fine == null) throw new Exception("Fine not found");

            var transaction = new FineTransaction
            {
                FineAllocationDate = DateTime.Now,
                LibraryBranchId = 1,
                Reason = "Fine Payment",
                Amount = amount,
                BankAccount = "Online",
                PaidOn = DateTime.Now,
                PaymentRef = paymentRef,
                IsActive = true
            };
            await _transactionRepo.AddAsync(transaction);

            fine.IsActive = false;
            await _fineRepo.UpdateAsync(fine);

            return fine;
        }

        public async Task BlockMemberAsync(int libraryMemberId, string reason, int days)
        {
            var block = new MemberBlock
            {
                LibraryMemberId = libraryMemberId,
                LibraryBranchId = 1,
                Reason = reason,
                ActiveFrom = DateTime.Now,
                ActiveTo = DateTime.Now.AddDays(days),
                IsActive = true
            };
            await _blockRepo.AddAsync(block);
        }

        public async Task<bool> IsMemberBlockedAsync(int libraryMemberId)
        {
            var blocks = await _blockRepo.GetAllAsync();
            return blocks.Any(b => b.LibraryMemberId == libraryMemberId && b.IsActive && b.ActiveTo > DateTime.Now);
        }

        public async Task<Issuance?> GetByIdAsync(int id) => await _issuanceRepo.GetByIdAsync(id);
        public async Task<IEnumerable<Issuance>> GetAllAsync() => await _issuanceRepo.GetAllAsync();
        public async Task<Issuance> AddAsync(Issuance entity) => await _issuanceRepo.AddAsync(entity);
        public async Task UpdateAsync(Issuance entity) => await _issuanceRepo.UpdateAsync(entity);
        public async Task DeleteAsync(int id) => await _issuanceRepo.DeleteAsync(id);
        public async Task<bool> ExistsAsync(int id) => await _issuanceRepo.ExistsAsync(id);
    }

    public interface ICirculationService : IService<Issuance>
    {
        Task<Issuance> IssueBookAsync(int libraryMemberId, int itemCopyId, int durationId);
        Task<ReturnItem> ReturnBookAsync(int issuanceId, decimal damageFine = 0);
        Task<ReserveItem> ReserveBookAsync(int libraryMemberId, int itemInfoId, int holdDays = 7);
        Task<decimal> CalculateFineAsync(int issuanceId);
        Task<UserFine> PayFineAsync(int userFineId, decimal amount, string paymentRef);
        Task BlockMemberAsync(int libraryMemberId, string reason, int days);
        Task<bool> IsMemberBlockedAsync(int libraryMemberId);
    }
}
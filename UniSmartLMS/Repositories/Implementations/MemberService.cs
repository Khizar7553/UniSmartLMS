using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services.Interfaces;

namespace UniSmartLMS.Services.Implementations
{
    public class MemberService : IMemberService
    {
        private readonly IGenericRepository<Party> _partyRepo;
        private readonly IGenericRepository<LibraryMember> _memberRepo;
        private readonly IGenericRepository<Employee> _employeeRepo;
        private readonly IGenericRepository<EmployeeRelationHistory> _historyRepo;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<UserFine> _fineRepo;        // ✅ ADD THIS
        private readonly IGenericRepository<MemberBlock> _blockRepo;    // ✅ ADD THIS

        public MemberService(
            IGenericRepository<Party> partyRepo,
            IGenericRepository<LibraryMember> memberRepo,
            IGenericRepository<Employee> employeeRepo,
            IGenericRepository<EmployeeRelationHistory> historyRepo,
            IGenericRepository<Student> studentRepo,
            IGenericRepository<UserFine> fineRepo,        // ✅ ADD THIS
            IGenericRepository<MemberBlock> blockRepo)    // ✅ ADD THIS
        {
            _partyRepo = partyRepo;
            _memberRepo = memberRepo;
            _employeeRepo = employeeRepo;
            _historyRepo = historyRepo;
            _studentRepo = studentRepo;
            _fineRepo = fineRepo;        // ✅ ADD THIS
            _blockRepo = blockRepo;      // ✅ ADD THIS
        }

        public async Task<LibraryMember> RegisterLibraryMemberAsync(int partyId, int libraryBranchId, string cardNo, int membershipType)
        {
            var existing = (await _memberRepo.GetAllAsync()).FirstOrDefault(m => m.PartyId == partyId);
            if (existing != null) return existing;

            var member = new LibraryMember
            {
                PartyId = partyId,
                LibraryBranchId = libraryBranchId,
                CardNo = cardNo,
                IssuedOn = DateTime.Now,
                ExpiredOn = DateTime.Now.AddYears(1),
                MembershipType = membershipType,
                IsActive = true
            };
            return await _memberRepo.AddAsync(member);
        }

        public async Task<Employee> AddEmployeeWithHistoryAsync(Employee employee, EmployeeRelationHistory history)
        {
            var newEmployee = await _employeeRepo.AddAsync(employee);
            history.EmployeeId = newEmployee.Id;
            history.IsCurrent = true;
            history.EffectiveTo = DateTime.Now.AddYears(1);
            await _historyRepo.AddAsync(history);

            newEmployee.CurrentRelationHistoryId = history.Id;
            await _employeeRepo.UpdateAsync(newEmployee);

            return newEmployee;
        }

        public async Task UpdateEmployeeHistoryAsync(int employeeId, EmployeeRelationHistory newHistory)
        {
            var oldHistory = (await _historyRepo.GetAllAsync())
                .FirstOrDefault(h => h.EmployeeId == employeeId && h.IsCurrent);
            if (oldHistory != null)
            {
                oldHistory.IsCurrent = false;
                oldHistory.EffectiveTo = DateTime.Now;
                await _historyRepo.UpdateAsync(oldHistory);
            }

            newHistory.EmployeeId = employeeId;
            newHistory.IsCurrent = true;
            await _historyRepo.AddAsync(newHistory);
        }

        public async Task<Student> RegisterStudentAsync(Student student) => await _studentRepo.AddAsync(student);
        public async Task<Party> AddPartyAsync(Party party) => await _partyRepo.AddAsync(party);

        public async Task<Party?> GetByIdAsync(int id) => await _partyRepo.GetByIdAsync(id);
        public async Task<IEnumerable<Party>> GetAllAsync() => await _partyRepo.GetAllAsync();
        public async Task<Party> AddAsync(Party entity) => await _partyRepo.AddAsync(entity);
        public async Task UpdateAsync(Party entity) => await _partyRepo.UpdateAsync(entity);
        public async Task DeleteAsync(int id) => await _partyRepo.DeleteAsync(id);
        public async Task<bool> ExistsAsync(int id) => await _partyRepo.ExistsAsync(id);

        public async Task<decimal> GetMemberTotalFineAsync(int libraryMemberId)
        {
            var fines = await _fineRepo.GetAllAsync();
            return fines
                .Where(f => f.LibraryMemberId == libraryMemberId && f.IsActive)
                .Sum(f => f.FineAccrued);
        }

        public async Task<bool> IsMemberBlockedAsync(int libraryMemberId)
        {
            var blocks = await _blockRepo.GetAllAsync();
            return blocks.Any(b => b.LibraryMemberId == libraryMemberId && b.IsActive && b.ActiveTo > DateTime.Now);
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
    }
}
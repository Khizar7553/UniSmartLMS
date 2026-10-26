using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Services.Interfaces;

public interface IMemberService : IService<Party>
{
    Task<LibraryMember> RegisterLibraryMemberAsync(int partyId, int libraryBranchId, string cardNo, int membershipType);
    Task<Employee> AddEmployeeWithHistoryAsync(Employee employee, EmployeeRelationHistory history);
    Task UpdateEmployeeHistoryAsync(int employeeId, EmployeeRelationHistory newHistory);
    Task<Student> RegisterStudentAsync(Student student);
    Task<Party> AddPartyAsync(Party party);

    // ✅ ADD THESE METHODS
    Task<decimal> GetMemberTotalFineAsync(int libraryMemberId);
    Task<bool> IsMemberBlockedAsync(int libraryMemberId);
    Task BlockMemberAsync(int libraryMemberId, string reason, int days);
}
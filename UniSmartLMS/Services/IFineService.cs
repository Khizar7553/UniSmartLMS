using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;

namespace UniSmartLMS.Services
{
    public interface IFineService
    {
        Task<decimal> CalculateFineAsync(Issuance issuance, DateTime returnDate);
        Task<UserFine> ApplyFineAsync(Issuance issuance, decimal fineAmount);
        Task<decimal> GetMemberTotalFineAsync(int libraryMemberId);
        Task<bool> HasOverdueBooksAsync(int libraryMemberId);
        Task<List<Issuance>> GetOverdueLoansAsync(int libraryMemberId);
    }
}
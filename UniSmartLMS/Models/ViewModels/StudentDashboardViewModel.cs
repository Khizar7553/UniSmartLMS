using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;

namespace UniSmartLMS.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string MemberName { get; set; }
        public string CardNumber { get; set; }
        public string Message { get; set; }
        public bool HasMembership { get; set; }

        public IEnumerable<Issuance> CurrentLoans { get; set; }
        public int CurrentLoansCount { get; set; }
        public int OverdueCount { get; set; }

        public IEnumerable<Issuance> LoanHistory { get; set; }
        public int TotalBooksRead { get; set; }

        public IEnumerable<UserFine> Fines { get; set; }
        public decimal TotalFine { get; set; }

        public IEnumerable<ReserveItem> Reservations { get; set; }
        public IEnumerable<ItemInfo> RecommendedBooks { get; set; }
    }
}
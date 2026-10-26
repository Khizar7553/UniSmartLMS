using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionE_Circulation;
using UniSmartLMS.Models.SectionF_Fines;
using UniSmartLMS.Models.SectionH_Digital;

namespace UniSmartLMS.ViewModels
{
    public class StaffDashboardViewModel
    {
        public string MemberName { get; set; }
        public string CardNumber { get; set; }
        public string Message { get; set; }
        public bool HasMembership { get; set; }
        public int MaxLoanLimit { get; set; }
        public int LoanDays { get; set; }

        public IEnumerable<Issuance> CurrentLoans { get; set; }
        public int CurrentLoansCount { get; set; }
        public int OverdueCount { get; set; }

        public IEnumerable<UserFine> Fines { get; set; }
        public decimal TotalFine { get; set; }

        public int DigitalResourcesCount { get; set; }
        public IEnumerable<OnlineAccessLog> RecentDigitalAccess { get; set; }
    }
}
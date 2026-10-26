namespace UniSmartLMS.ViewModels
{
    public class FacultyDashboardViewModel
    {
        public string MemberName { get; set; }
        public string MemberType { get; set; }
        public string CardNumber { get; set; }
        public int MaxLoanLimit { get; set; }
        public int LoanDays { get; set; }
        public int CurrentLoansCount { get; set; }
        public int CourseReservesCount { get; set; }

        // Research Resources
        public int ResearchPapersCount { get; set; }
        public int EBooksCount { get; set; }
        public int JournalsCount { get; set; }
        public int DatabasesCount { get; set; }

        // Publications
        public int MyPublicationsCount { get; set; }
        public int PendingRecommendations { get; set; }
    }
}
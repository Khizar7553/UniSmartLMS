namespace UniSmartLMS.ViewModels
{
    public class LibrarianDashboardViewModel
    {
        public int TodaysIssuances { get; set; }
        public int OverdueLoans { get; set; }
        public int PendingFinesCount { get; set; }
        public decimal PendingFinesAmount { get; set; }
        public int ActiveMembers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalCopies { get; set; }
        public int TotalMembers { get; set; }
        public int TodaysReturns { get; set; }
        public int PendingReservations { get; set; }
    }
}
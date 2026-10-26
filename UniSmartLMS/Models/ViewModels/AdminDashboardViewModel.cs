namespace UniSmartLMS.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Statistics
        public int TotalUniversities { get; set; }
        public int TotalCampuses { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalBooks { get; set; }
        public int TotalCopies { get; set; }
        public int TotalMembers { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveLoans { get; set; }
        public int TodaysIssuances { get; set; }
        public decimal TotalFineAmount { get; set; }
        public int TotalReservations { get; set; }

        // Role counts
        public int StudentCount { get; set; }
        public int FacultyCount { get; set; }
        public int StaffCount { get; set; }
        public int LibrarianCount { get; set; }
        public int AdminCount { get; set; }
        public int VisitorCount { get; set; }

        // System Health
        public string DatabaseStatus { get; set; }
        public string LastBackupDate { get; set; }
        public int TotalAuditLogs { get; set; }
    }
}
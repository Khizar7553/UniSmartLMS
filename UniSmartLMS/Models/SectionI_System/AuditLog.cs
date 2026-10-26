using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int? RecordId { get; set; }
        public string Action { get; set; } // INSERT, UPDATE, DELETE
        public string OldValues { get; set; } // JSON
        public string NewValues { get; set; } // JSON
        public int? PerformedByStaffId { get; set; }
        public string PerformedByName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
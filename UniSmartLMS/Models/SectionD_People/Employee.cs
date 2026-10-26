using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("Employees")]
    public class Employee : BaseAudit
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Code { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public int CurrentRelationHistoryId { get; set; }
    }
}
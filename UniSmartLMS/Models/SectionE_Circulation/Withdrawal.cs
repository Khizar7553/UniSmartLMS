using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("Withdrawals")]
    public class Withdrawal : BaseAudit
    {
        public int ItemCopyId { get; set; }
        public DateTime Date { get; set; }
        public int LibraryId { get; set; }
        public string Reason { get; set; }
        public int EmployeeId { get; set; }

        [ForeignKey("ItemCopyId")] public virtual ItemCopy ItemCopy { get; set; }
        [ForeignKey("EmployeeId")] public virtual Employee Employee { get; set; }
    }
}
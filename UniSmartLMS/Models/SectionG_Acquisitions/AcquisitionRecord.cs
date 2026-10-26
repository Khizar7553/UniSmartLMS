using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionG_Acquisitions
{
    [Table("AcquisitionRecords")]
    public class AcquisitionRecord : BaseAudit
    {
        public string Type { get; set; }
        public int PartyId { get; set; }
        public string Invoice { get; set; }
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [ForeignKey("PartyId")] public virtual Party Party { get; set; }
    }
}
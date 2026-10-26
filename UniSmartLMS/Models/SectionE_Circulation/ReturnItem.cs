using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("ReturnItems")]
    public class ReturnItem : BaseAudit
    {
        public int IssuanceId { get; set; }
        public DateTime ReturnDate { get; set; }
        [ForeignKey("IssuanceId")] public virtual Issuance Issuance { get; set; }
    }
}
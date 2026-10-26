using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("Issuances")]
    public class Issuance : BaseAudit
    {
        public int LibraryMemberId { get; set; }
        public int ItemCopyId { get; set; }
        public int DurationId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DurationNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal FineAccrued { get; set; }
        public int RenewalsUsed { get; set; }

        [ForeignKey("LibraryMemberId")] public virtual LibraryMember LibraryMember { get; set; }
        [ForeignKey("ItemCopyId")] public virtual ItemCopy ItemCopy { get; set; }
        [ForeignKey("DurationId")] public virtual Duration Duration { get; set; }
    }
}
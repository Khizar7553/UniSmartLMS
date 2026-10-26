using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionF_Fines
{
    [Table("UserFines")]
    public class UserFine : BaseAudit
    {
        public int FineAllocationId { get; set; }
        public int LibraryMemberId { get; set; }
        public int FineDefinitionId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxPercentagePerDay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FineAccrued { get; set; }
        public int? ItemCopyId { get; set; }

        [ForeignKey("LibraryMemberId")] public virtual LibraryMember LibraryMember { get; set; }
        [ForeignKey("FineDefinitionId")] public virtual FineDefinition FineDefinition { get; set; }
        [ForeignKey("ItemCopyId")] public virtual ItemCopy ItemCopy { get; set; }
    }
}
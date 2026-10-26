using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionG_Acquisitions
{
    [Table("ItemQuotations")]
    public class ItemQuotation : BaseAudit
    {
        public int PartyId { get; set; }
        public int LibraryRackId { get; set; }
        public int QuotationDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [ForeignKey("PartyId")] public virtual Party Party { get; set; }
        [ForeignKey("LibraryRackId")] public virtual LibraryRack LibraryRack { get; set; }
    }
}
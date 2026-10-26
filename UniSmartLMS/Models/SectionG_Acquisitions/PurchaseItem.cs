using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionG_Acquisitions
{
    [Table("PurchaseItems")]
    public class PurchaseItem : BaseAudit
    {
        public int ItemQuotationId { get; set; }
        public DateTime PurchaseItemDate { get; set; }
        public int LibraryItemId { get; set; }
        public string InvoiceNumber { get; set; }
        public int PartyId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [ForeignKey("ItemQuotationId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemQuotation ItemQuotation { get; set; }

        [ForeignKey("LibraryItemId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemInfo ItemInfo { get; set; }

        [ForeignKey("PartyId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Party Party { get; set; }
    }
}
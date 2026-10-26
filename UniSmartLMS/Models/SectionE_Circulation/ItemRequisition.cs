using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionC_Catalog;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("ItemRequisitions")]
    public class ItemRequisition : BaseAudit
    {
        public int LibraryBranchId { get; set; }
        public int ItemInfoId { get; set; }
        public int Quantity { get; set; }
        public int LibraryRackId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimateCost { get; set; }
        [ForeignKey("LibraryBranchId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryBranch LibraryBranch { get; set; }

        [ForeignKey("ItemInfoId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemInfo ItemInfo { get; set; }

        [ForeignKey("LibraryRackId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryRack LibraryRack { get; set; }
    }
}
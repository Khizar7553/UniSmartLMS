using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("ItemCopies")]
    public class ItemCopy : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public int? LibraryBranchId { get; set; }
        public int? LibraryRackId { get; set; }
        public int? LibraryShelfId { get; set; }
        public string Status { get; set; } = "Available";
        public string Barcode { get; set; }
        public string AccessionNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitCost { get; set; }
        public string ConditionRemarks { get; set; }
        public DateTime? ConditionLastChecked { get; set; }

        [ForeignKey("ItemInfoId")] public virtual ItemInfo ItemInfo { get; set; }
        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
        [ForeignKey("LibraryRackId")] public virtual LibraryRack LibraryRack { get; set; }
        [ForeignKey("LibraryShelfId")] public virtual LibraryShelf LibraryShelf { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("ItemCategories")]
    public class ItemCategory : BaseAudit
    {
        public string ItemCategoryName { get; set; }
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")] public virtual ItemCategory ParentCategory { get; set; }
        public virtual ICollection<ItemCategory> ChildCategories { get; set; } = new List<ItemCategory>();
    }
}
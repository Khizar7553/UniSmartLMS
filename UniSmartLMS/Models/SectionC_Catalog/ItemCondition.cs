using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("ItemConditions")]
    public class ItemCondition : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public int ItemCopyId { get; set; }
        public int ItemCategoryId { get; set; }
        public string ConditionDescription { get; set; }

        [ForeignKey("ItemInfoId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemInfo ItemInfo { get; set; }

        [ForeignKey("ItemCopyId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemCopy ItemCopy { get; set; }

        [ForeignKey("ItemCategoryId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemCategory ItemCategory { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("AuthorItemInfos")]
    public class AuthorItemInfo : BaseAudit
    {
        public int AuthorId { get; set; }
        public int ItemInfoId { get; set; }
        [ForeignKey("AuthorId")] public virtual AuthorInfo Author { get; set; }
        [ForeignKey("ItemInfoId")] public virtual ItemInfo ItemInfo { get; set; }
    }
}
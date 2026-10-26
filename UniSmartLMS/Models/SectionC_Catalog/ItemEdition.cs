using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("ItemEditions")]
    public class ItemEdition : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public int Year { get; set; }
        public string EditionNumber { get; set; }
        [ForeignKey("ItemInfoId")] public virtual ItemInfo ItemInfo { get; set; }
    }
}
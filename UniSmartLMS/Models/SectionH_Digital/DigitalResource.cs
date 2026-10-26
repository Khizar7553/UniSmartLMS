using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;

namespace UniSmartLMS.Models.SectionH_Digital
{
    [Table("DigitalResources")]
    public class DigitalResource : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public string FilePath { get; set; }
        public string FileFormat { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal FileSizeMb { get; set; }
        public int TotalDownloads { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("ItemInfoId")] public virtual ItemInfo ItemInfo { get; set; }
    }
}
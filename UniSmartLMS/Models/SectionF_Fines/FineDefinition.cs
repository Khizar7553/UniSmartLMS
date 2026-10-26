using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;

namespace UniSmartLMS.Models.SectionF_Fines
{
    [Table("FineDefinitions")]
    public class FineDefinition : BaseAudit
    {
        public int FineConditionId { get; set; }
        public int ItemCopyId { get; set; }
        public int FineCategoryId { get; set; }

        [ForeignKey("ItemCopyId")] public virtual ItemCopy ItemCopy { get; set; }
        [ForeignKey("FineCategoryId")] public virtual FineCategory FineCategory { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("ItemSubjects")]
    public class ItemSubject : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public int SubjectId { get; set; }
        public bool IsPrimary { get; set; } = false;

        [ForeignKey("ItemInfoId")]
        public virtual ItemInfo ItemInfo { get; set; }

        [ForeignKey("SubjectId")]
        public virtual SubjectHeading Subject { get; set; }
    }
}
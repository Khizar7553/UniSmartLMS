using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("ReserveItems")]
    public class ReserveItem : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public int DurationId { get; set; }
        public string NumberDuration { get; set; }
        public int LibraryMemberId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey("ItemInfoId")] public virtual ItemInfo ItemInfo { get; set; }
        [ForeignKey("DurationId")] public virtual Duration Duration { get; set; }
        [ForeignKey("LibraryMemberId")] public virtual LibraryMember LibraryMember { get; set; }
    }
}
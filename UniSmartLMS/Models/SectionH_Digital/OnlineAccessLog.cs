using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionH_Digital
{
    [Table("OnlineAccessLogs")]
    public class OnlineAccessLog : BaseAudit
    {
        public int DigitalResourceId { get; set; }
        public int LibraryMemberId { get; set; }
        public DateTime AccessDate { get; set; }
        public string AccessType { get; set; }

        [ForeignKey("DigitalResourceId")]
        public virtual DigitalResource DigitalResource { get; set; }

        [ForeignKey("LibraryMemberId")]
        public virtual LibraryMember LibraryMember { get; set; }
    }
}
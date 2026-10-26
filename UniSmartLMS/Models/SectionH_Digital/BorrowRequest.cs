using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionH_Digital
{
    [Table("BorrowRequests")]
    public class BorrowRequest : BaseAudit
    {
        [Required]
        public int MemberId { get; set; }

        [Required]
        public int ItemCopyId { get; set; }

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string MemberRemarks { get; set; } = "";

        [StringLength(500)]
        public string AdminRemarks { get; set; } = "";

        public int? ApprovedByStaffId { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [ForeignKey("MemberId")]
        public virtual LibraryMember LibraryMember { get; set; }
        
        [ForeignKey("ItemCopyId")]
        public virtual ItemCopy ItemCopy { get; set; }
    }
}
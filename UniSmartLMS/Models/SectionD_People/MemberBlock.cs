using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("MemberBlocks")]
    public class MemberBlock : BaseAudit
    {
        public int LibraryMemberId { get; set; }
        public int LibraryBranchId { get; set; }
        public string Reason { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }

        [ForeignKey("LibraryMemberId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryMember LibraryMember { get; set; }

        [ForeignKey("LibraryBranchId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryBranch LibraryBranch { get; set; }
    }
}
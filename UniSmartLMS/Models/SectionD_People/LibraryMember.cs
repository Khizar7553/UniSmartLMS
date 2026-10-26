using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("LibraryMembers")]
    public class LibraryMember : BaseAudit
    {
        public int LibraryBranchId { get; set; }
        public int PartyId { get; set; }
        public string CardNo { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiredOn { get; set; }
        public int MembershipType { get; set; }

        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
        [ForeignKey("PartyId")] public virtual Party Party { get; set; }
    }
}
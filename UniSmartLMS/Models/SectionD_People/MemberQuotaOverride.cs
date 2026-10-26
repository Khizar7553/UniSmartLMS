using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("MemberQuotaOverrides")]
    public class MemberQuotaOverride : BaseAudit
    {
        public int LibraryMemberId { get; set; }
        public int LibraryBranchId { get; set; }
        public int MaxCurrentLoans { get; set; }
        public int MaxCurrentReserves { get; set; }
        public int LoanDays { get; set; }
        public int GraceDay { get; set; }
        public int MaxRenewals { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal OverdueFinePerDay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxFinePerItem { get; set; }

        [ForeignKey("LibraryMemberId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryMember LibraryMember { get; set; }

        [ForeignKey("LibraryBranchId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryBranch LibraryBranch { get; set; }
    }
}
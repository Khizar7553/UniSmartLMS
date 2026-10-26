using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("CirculationRules")]
    public class CirculationRule : BaseAudit
    {
        public int PartyId { get; set; }
        public int MaxCurrentLoans { get; set; }
        public int MaxCurrentReserves { get; set; }
        public int LoanDays { get; set; }
        public int GraceDay { get; set; }
        public int MaxRenewals { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal OverdueFinePerDay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxFinePerItem { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DamageValue { get; set; }
        public int LibraryBranchId { get; set; }
        public string LostChargesItem { get; set; }
        [ForeignKey("PartyId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Party Party { get; set; }
        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
    }
}
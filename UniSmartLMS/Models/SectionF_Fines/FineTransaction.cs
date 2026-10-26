using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionF_Fines
{
    [Table("FineTransactions")]
    public class FineTransaction : BaseAudit
    {
        public DateTime FineAllocationDate { get; set; }
        public int LibraryBranchId { get; set; }
        public string Reason { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string BankAccount { get; set; }
        public DateTime PaidOn { get; set; }
        public string PaymentRef { get; set; }

        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
    }
}
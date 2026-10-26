using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;
using UniSmartLMS.Models.SectionC_Catalog;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionE_Circulation
{
    [Table("LibraryRequisitions")]
    public class LibraryRequisition : BaseAudit
    {
        public int ItemInfoId { get; set; }
        public int LibrarianInfoId { get; set; }
        public int LibraryBranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string Purpose { get; set; }

        [ForeignKey("ItemInfoId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual ItemInfo ItemInfo { get; set; }

        [ForeignKey("LibrarianInfoId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibrarianInfo LibrarianInfo { get; set; }

        [ForeignKey("LibraryBranchId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryBranch LibraryBranch { get; set; }

        [ForeignKey("EmployeeId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Employee Employee { get; set; }
    }
}
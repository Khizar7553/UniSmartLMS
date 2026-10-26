using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionB_Library
{
    [Table("LibraryBranchManagements")]
    public class LibraryBranchManagement : BaseAudit
    {
        public int LibraryBranchId { get; set; }
        public int LibraryManagementSectionId { get; set; }
        public string Code { get; set; }
        public DateTime EstablishedIn { get; set; }
        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
        [ForeignKey("LibraryManagementSectionId")] public virtual LibraryManagementSection LibraryManagementSection { get; set; }
    }
}
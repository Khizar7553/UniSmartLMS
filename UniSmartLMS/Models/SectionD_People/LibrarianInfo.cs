using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("LibrarianInfos")]
    public class LibrarianInfo : BaseAudit
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public int? LibraryBranchId { get; set; }

        [ForeignKey("LibraryBranchId")]
        public virtual LibraryBranch LibraryBranch { get; set; }
    }
}
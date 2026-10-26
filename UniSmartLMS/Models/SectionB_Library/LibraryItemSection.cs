using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionA_University;

namespace UniSmartLMS.Models.SectionB_Library
{
    [Table("LibraryItemSections")]
    public class LibraryItemSection : BaseNameCode
    {
        public int LibraryBranchId { get; set; }
        public int DepartmentId { get; set; }
        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
        [ForeignKey("DepartmentId")] public virtual Department Department { get; set; }
        public virtual ICollection<LibraryRack> LibraryRacks { get; set; } = new List<LibraryRack>();
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionB_Library
{
    [Table("LibraryBranches")]
    public class LibraryBranch : BaseNameCodeDescription  // ✅ Changed from BaseAudit
    {
        public int LibraryTypeId { get; set; }
        public int? ParentId { get; set; }

        [ForeignKey("LibraryTypeId")]
        public virtual LibraryType LibraryType { get; set; }

        [ForeignKey("ParentId")]
        public virtual LibraryBranch ParentBranch { get; set; }
    }
}
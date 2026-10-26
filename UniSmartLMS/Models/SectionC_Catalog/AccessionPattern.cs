using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionB_Library;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("AccessionPatterns")]
    public class AccessionPattern : BaseAudit
    {
        public int LibraryBranchId { get; set; }
        public string Pattern { get; set; }
        public int NextSequence { get; set; }
        [ForeignKey("LibraryBranchId")] public virtual LibraryBranch LibraryBranch { get; set; }
    }
}
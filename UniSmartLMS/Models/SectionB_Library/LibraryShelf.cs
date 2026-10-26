using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionB_Library
{
    [Table("LibraryShelves")]
    public class LibraryShelf : BaseAudit
    {
        public string Code { get; set; }
        public int LibraryRackId { get; set; }
        public int? MaxCapacity { get; set; }
        public int? NoOfItems { get; set; }
        [ForeignKey("LibraryRackId")] public virtual LibraryRack LibraryRack { get; set; }
    }
}
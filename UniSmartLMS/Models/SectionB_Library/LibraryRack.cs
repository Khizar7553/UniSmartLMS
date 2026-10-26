using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionB_Library
{
    [Table("LibraryRacks")]
    public class LibraryRack : BaseAudit
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int? LibrarySectionId { get; set; }
        public int LibraryItemSectionId { get; set; }
        [ForeignKey("LibraryItemSectionId")] public virtual LibraryItemSection LibraryItemSection { get; set; }
        public virtual ICollection<LibraryShelf> LibraryShelves { get; set; } = new List<LibraryShelf>();
    }
}
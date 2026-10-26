using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("ItemInfos")]
    public class ItemInfo : BaseAudit
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ISBN { get; set; }
        public DateTime? PublishDate { get; set; }
        public int? SubjectHeadingId { get; set; }
        public int? TotalPages { get; set; }
        public string BindingType { get; set; }
        public DateTime? AddedToLibraryDate { get; set; }
        public string Keywords { get; set; }
        public int? LanguageId { get; set; }
        public int? PublisherInfoId { get; set; }

        [ForeignKey("SubjectHeadingId")] public virtual SubjectHeading SubjectHeading { get; set; }
        [ForeignKey("LanguageId")] public virtual Language Language { get; set; }
        [ForeignKey("PublisherInfoId")] public virtual PublisherInfo PublisherInfo { get; set; }

        public virtual ICollection<AuthorItemInfo> AuthorItemInfos { get; set; } = new List<AuthorItemInfo>();
        public virtual ICollection<ItemCopy> ItemCopies { get; set; } = new List<ItemCopy>();
        public virtual ICollection<ItemEdition> ItemEditions { get; set; } = new List<ItemEdition>();
    }
}
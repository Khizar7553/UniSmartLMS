using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("PublisherInfos")]
    public class PublisherInfo : BaseAudit
    {
        public string Name { get; set; }
    }
}
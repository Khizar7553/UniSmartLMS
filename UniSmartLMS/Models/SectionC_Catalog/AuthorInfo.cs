using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("AuthorInfos")]
    public class AuthorInfo : BaseAudit
    {
        public string Name { get; set; }
    }
}
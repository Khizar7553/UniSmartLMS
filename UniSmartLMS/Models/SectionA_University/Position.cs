using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Positions")]
    public class Position : BaseAudit
    {
        public string Category { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}

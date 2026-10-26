using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Blocks")]
    public class Block : BaseNameCode
    {
        public int CampusId { get; set; }
        [ForeignKey("CampusId")] public virtual Campus Campus { get; set; }
        public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}
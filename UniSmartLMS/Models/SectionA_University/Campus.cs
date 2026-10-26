using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Campuses")]
    public class Campus : BaseNameCodeDescription
    {
        public int UniversityId { get; set; }
        [ForeignKey("UniversityId")] public virtual University University { get; set; }
        public virtual ICollection<Block> Blocks { get; set; } = new List<Block>();
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Faculties")]
    public class Faculty : BaseNameCodeDescription
    {
        public DateTime? EstablishedIn { get; set; }
        public int CampusId { get; set; }
        public int InstituteId { get; set; }
        [ForeignKey("CampusId")] public virtual Campus Campus { get; set; }
        [ForeignKey("InstituteId")] public virtual Institute Institute { get; set; }
    }
}
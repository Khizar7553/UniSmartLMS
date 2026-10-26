using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("StructureUnits")]
    public class StructureUnit : BaseNameCode
    {
        public int Level { get; set; }
        public int StructureTypeId { get; set; }
        [ForeignKey("StructureTypeId")] public virtual StructureType StructureType { get; set; }
    }
}
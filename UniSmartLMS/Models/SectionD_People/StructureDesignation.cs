using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models.SectionD_People  // ← ADD THIS
{
    [Table("StructureDesignations")]
    public class StructureDesignation : BaseAudit
    {
        public int DesignationId { get; set; }
        public int StructureUnitId { get; set; }

        [ForeignKey("DesignationId")]
        public virtual Designation Designation { get; set; }

        [ForeignKey("StructureUnitId")]
        public virtual StructureUnit StructureUnit { get; set; }
    }
}
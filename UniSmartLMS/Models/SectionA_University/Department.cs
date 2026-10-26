using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Departments")]
    public class Department : BaseNameCode
    {
        public int? ParentId { get; set; }
        public int InstituteId { get; set; }
        public int FacultyId { get; set; }

        [ForeignKey("ParentId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]  // ✅ ADD THIS
        public virtual Department ParentDepartment { get; set; }

        [ForeignKey("InstituteId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]  // ✅ ADD THIS
        public virtual Institute Institute { get; set; }

        [ForeignKey("FacultyId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]  // ✅ ADD THIS
        public virtual Faculty Faculty { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionA_University;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("EmployeeRelationHistories")]
    public class EmployeeRelationHistory : BaseAudit
    {
        public int EmployeeId { get; set; }
        public DateTime EffectiveTo { get; set; }
        public bool IsCurrent { get; set; }
        public string EmployeeType { get; set; }
        public int EmployeeGrade { get; set; }
        public int DepartmentId { get; set; }
        public int DesignationId { get; set; }
        public int CampusId { get; set; }
        public int InstituteId { get; set; }
        public int FacultyId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryPayable { get; set; }
        [ForeignKey("EmployeeId")] public virtual Employee Employee { get; set; }
        [ForeignKey("DepartmentId")] public virtual Department Department { get; set; }
        [ForeignKey("DesignationId")] public virtual Designation Designation { get; set; }
    }
}
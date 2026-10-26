using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("Students")]
    public class Student : BaseAudit
    {
        public string RegistrationNumber { get; set; }
        public int CandidateId { get; set; }
        public int DegreeProgramId { get; set; }
        public int DegreeLevelId { get; set; }
        public int PartyId { get; set; }
        public string Quota { get; set; }
        public int TimingId { get; set; }
        public int ProgramSessionId { get; set; }
        [ForeignKey("PartyId")] public virtual Party Party { get; set; }
    }
}
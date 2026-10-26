using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionA_University;

namespace UniSmartLMS.Models.SectionD_People
{
    [Table("Parties")]
    public class Party : BaseAudit
    {
        public string PartyName { get; set; }
        public string PartyContact { get; set; }
        public string Email { get; set; }  // ✅ ADD THIS
        public int? PartyPositionId { get; set; }
        public int? PartyCompanyId { get; set; }
        public int? PartyTypeId { get; set; }
        [ForeignKey("PartyPositionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Position Position { get; set; }
    }
}
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;
using UniSmartLMS.Models.SectionA_University;

namespace UniSmartLMS.Models.SectionB_Library
{
    [Table("LibraryBranchLocations")]
    public class LibraryBranchLocation : BaseAudit
    {
        public int LibraryBranchId { get; set; }
        public int BuildingId { get; set; }
        public int FloorId { get; set; }
        public int RoomId { get; set; }
        public string Description { get; set; }

        [ForeignKey("LibraryBranchId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual LibraryBranch LibraryBranch { get; set; }

        [ForeignKey("BuildingId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Building Building { get; set; }

        [ForeignKey("FloorId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Floor Floor { get; set; }

        [ForeignKey("RoomId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Room Room { get; set; }
    }
}
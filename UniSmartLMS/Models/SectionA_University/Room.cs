using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Rooms")]
    public class Room : BaseNameCodeDescription
    {
        public int FloorId { get; set; }
        public DateTime? EstablishedIn { get; set; }
        [ForeignKey("FloorId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Floor Floor { get; set; }
    }
}
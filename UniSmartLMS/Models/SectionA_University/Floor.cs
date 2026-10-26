using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Floors")]
    public class Floor : BaseNameCode
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Building Building { get; set; }
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
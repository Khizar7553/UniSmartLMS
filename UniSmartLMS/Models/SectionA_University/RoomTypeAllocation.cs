using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("RoomTypeAllocations")]
    public class RoomTypeAllocation
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }
        [ForeignKey("RoomId")] public virtual Room Room { get; set; }
        [ForeignKey("RoomTypeId")] public virtual RoomType RoomType { get; set; }
    }
}
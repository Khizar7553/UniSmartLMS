using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("RoomFeatureAllocations")]
    public class RoomFeatureAllocation
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int FeatureId { get; set; }
        [ForeignKey("RoomId")] public virtual Room Room { get; set; }
        [ForeignKey("FeatureId")] public virtual Feature Feature { get; set; }
    }
}
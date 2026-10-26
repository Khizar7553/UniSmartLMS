using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionA_University
{
    [Table("Buildings")]
    public class Building : BaseNameCodeDescription
    {
        public int BlockId { get; set; }
        [ForeignKey("BlockId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Block Block { get; set; }
        public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
    }
}
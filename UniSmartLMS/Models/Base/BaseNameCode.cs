using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace UniSmartLMS.Models.Base
{
    public class BaseNameCode : BaseAudit
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }
    }
}
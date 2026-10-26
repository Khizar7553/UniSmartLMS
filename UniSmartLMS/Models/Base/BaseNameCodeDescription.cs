using System.ComponentModel.DataAnnotations;

namespace UniSmartLMS.Models.Base
{
    public class BaseNameCodeDescription : BaseNameCode
    {
        [StringLength(500)]
        public string Description { get; set; }
    }
}
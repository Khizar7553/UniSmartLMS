using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionF_Fines
{
    [Table("FineCategories")]
    public class FineCategory : BaseAudit
    {
        public string FineCategoryName { get; set; }
    }
}
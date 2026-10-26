using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.Base;

namespace UniSmartLMS.Models.SectionC_Catalog
{
    [Table("Languages")]
    public class Language : BaseAudit
    {
        public string LanguageCategory { get; set; }  // Urdu, English, Arabic
        public string LanguageCode { get; set; }      // UR, EN, AR, PN, SD, PS, BL
    }
}
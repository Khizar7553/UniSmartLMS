using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models
{
    [Table("Documents")]
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        public int? FileSize { get; set; }

        public int? PageCount { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

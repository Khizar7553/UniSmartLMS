using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models
{
    [Table("DocumentChunks")]
    public class DocumentChunk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public int ChunkIndex { get; set; }

        public int? PageNumber { get; set; }

        [Required]
        public string Content { get; set; }

        public string Embedding { get; set; }  // JSON array for vector

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; }
    }
}
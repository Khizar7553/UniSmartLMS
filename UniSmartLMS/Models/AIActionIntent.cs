using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models
{
    [Table("AIActionIntents")]
    public class AIActionIntent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string IntentKeyword { get; set; }  // "create university", "add book"

        [Required]
        [StringLength(20)]
        public string ActionType { get; set; }  // "Redirect", "OpenForm", "Execute"

        [StringLength(100)]
        public string Controller { get; set; }  // "University", "ItemInfo"

        [StringLength(100)]
        public string Action { get; set; }  // "Create", "Index"

        [StringLength(50)]
        public string RequiredRole { get; set; }  // "Admin", "Librarian", "Student"

        [StringLength(200)]
        public string SuccessMessage { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
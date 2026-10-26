using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models
{
    [Table("ChatConversations")]
    public class ChatConversation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

}
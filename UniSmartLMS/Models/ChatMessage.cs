using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniSmartLMS.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string Role { get; set; } // "User" or "Assistant"
        public string Content { get; set; }
        public List<string>? Sources { get; set; } // JSON array of source filenames
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ConversationId")]
        public virtual ChatConversation Conversation { get; set; }
    }
}
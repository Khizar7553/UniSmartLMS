namespace UniSmartLMS.Models
{
    public class ConversationLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Query { get; set; }
        public string Response { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
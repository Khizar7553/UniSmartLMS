namespace UniSmartLMS.Models.Base
{
    public class BaseAudit
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
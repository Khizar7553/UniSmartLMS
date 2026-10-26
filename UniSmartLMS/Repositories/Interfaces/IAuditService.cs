using UniSmartLMS.Models;


namespace UniSmartLMS.Services.Interfaces
{
    public interface IAuditService
    {
        Task<List<AuditLog>> GetAllAsync();
    }
}
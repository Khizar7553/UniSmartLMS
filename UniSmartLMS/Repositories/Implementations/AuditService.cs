using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Services.Interfaces;
using UniSmartLMS.Models;

namespace UniSmartLMS.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(500)
                .ToListAsync();

            return logs.Select(log => new AuditLog
            {
                Id = log.Id,
                TableName = log.TableName,
                RecordId = log.RecordId,
                Action = log.Action,
                OldValues = log.OldValues?.Length > 100 ? log.OldValues.Substring(0, 100) + "..." : log.OldValues,
                NewValues = log.NewValues?.Length > 100 ? log.NewValues.Substring(0, 100) + "..." : log.NewValues,
                PerformedByName = log.PerformedByName ?? "System",
                Timestamp = log.Timestamp
            }).ToList();
        }
    }
}
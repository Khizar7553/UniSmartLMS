using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;

namespace UniSmartLMS.Services.AI
{
    public interface IIntentCacheService
    {
        Task LoadIntentsAsync();
        List<AIActionIntent> GetIntents();
        AIActionIntent GetMatchingIntent(string query, string role);
    }

    public class IntentCacheService : IIntentCacheService
    {
        private readonly IServiceProvider _serviceProvider;
        private static List<AIActionIntent> _cachedIntents = new();

        public IntentCacheService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task LoadIntentsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _cachedIntents = await context.AIActionIntents
                .Where(i => i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public List<AIActionIntent> GetIntents() => _cachedIntents;

        public AIActionIntent GetMatchingIntent(string query, string role)
        {
            var lowerQuery = query.ToLower();

            return _cachedIntents.FirstOrDefault(intent =>
                lowerQuery.Contains(intent.IntentKeyword.ToLower()) &&
                intent.RequiredRole.Contains(role));  // ✅ Check if role is in comma-separated list
        }
    }
}
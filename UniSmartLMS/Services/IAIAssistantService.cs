using UniSmartLMS.Models;

namespace UniSmartLMS.Services
{
    public interface IAIAssistantService
    {
        Task<string> GetResponseAsync(string userQuery, AppUser user);
        Task<string> GetGreetingAsync(AppUser user);
        Task<string> GetRoleBasedHelpAsync(AppUser user);
    }
}
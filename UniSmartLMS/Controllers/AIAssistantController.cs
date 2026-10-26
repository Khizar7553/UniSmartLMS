using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniSmartLMS.Models;
using UniSmartLMS.Services;

namespace UniSmartLMS.Controllers
{
    [Authorize]
    public class AIAssistantController : Controller
    {
        private readonly IAIAssistantService _aiService;
        private readonly UserManager<AppUser> _userManager;

        public AIAssistantController(
            IAIAssistantService aiService,
            UserManager<AppUser> userManager)
        {
            _aiService = aiService;
            _userManager = userManager;
        }


        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Query))
                return Json(new { response = "Please ask me something!" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { response = "Please log in to use the AI assistant." });

            var response = await _aiService.GetResponseAsync(request.Query, user);

            // ✅ DEBUG: Log what is being returned
            System.Diagnostics.Debug.WriteLine($"AI Response: {response}");

            return Json(new { response });
        }

        [HttpGet]
        public async Task<IActionResult> GetGreeting()
        {
            var user = await _userManager.GetUserAsync(User);
            var greeting = await _aiService.GetGreetingAsync(user);
            return Json(new { greeting });
        }
    }

    public class ChatRequest
    {
        public string Query { get; set; }
    }
}

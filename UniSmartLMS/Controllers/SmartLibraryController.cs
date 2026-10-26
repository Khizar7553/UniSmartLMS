using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Services;

namespace UniSmartLMS.Controllers
{
    [Authorize]
    public class SmartLibraryController : Controller
    {
        private readonly IDeepThinkEngine _deepThinkEngine;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SmartLibraryController(IDeepThinkEngine deepThinkEngine, UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _deepThinkEngine = deepThinkEngine;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Helper: Strip HTML tags from content for database storage
        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        // Helper: Extract plain text from HTML response
        private string ExtractPlainText(string htmlAnswer)
        {
            if (string.IsNullOrEmpty(htmlAnswer)) return "";
            // Remove all HTML tags
            var plainText = Regex.Replace(htmlAnswer, "<.*?>", "");
            // Clean up extra whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ");
            return plainText.Trim();
        }

        // Get current user's conversations only
        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var userId = _userManager.GetUserId(User);
            var conversations = await _context.ChatConversations
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new { c.Id, c.Title, c.UpdatedAt })
                .ToListAsync();
            return Json(conversations);
        }

        // Get messages for a specific conversation (verify ownership)
        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var userId = _userManager.GetUserId(User);
            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

            if (conversation == null)
                return Json(new { success = false, error = "Unauthorized" });

            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new { m.Role, m.Content })
                .ToListAsync();
            return Json(messages);
        }

        // Delete conversation (verify ownership)
        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            var userId = _userManager.GetUserId(User);
            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

            if (conversation != null)
            {
                conversation.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        // Ask question – save to user's conversation
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] string question)
        {
            var userId = _userManager.GetUserId(User);

            // Get or create conversation for this user only
            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (conversation == null)
            {
                conversation = new ChatConversation
                {
                    UserId = userId,
                    Title = question.Length > 50 ? question.Substring(0, 47) + "..." : question,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.ChatConversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            // Save user message (plain text)
            _context.ChatMessages.Add(new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = "user",
                Content = question,  // Plain text
                CreatedAt = DateTime.Now
            });

            // Get AI response (contains HTML formatting)
            var response = await _deepThinkEngine.ThinkAsync(question, userId);

            // Save AI response as PLAIN TEXT (remove HTML tags)
            var plainTextAnswer = ExtractPlainText(response.FinalAnswer);

            _context.ChatMessages.Add(new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = plainTextAnswer,  // Store plain text, not HTML
                CreatedAt = DateTime.Now
            });

            conversation.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Return HTML answer for display (not saved to history)
            return Json(new
            {
                success = true,
                answer = response.FinalAnswer,  // HTML for display
                conversationId = conversation.Id,
                sources = response.Sources,
                confidence = response.Confidence
            });
        }
    }
}
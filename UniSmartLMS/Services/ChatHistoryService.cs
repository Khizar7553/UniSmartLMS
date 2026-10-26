using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;

namespace UniSmartLMS.Services
{
    public interface IChatHistoryService
    {
        Task<List<ChatConversation>> GetUserConversationsAsync(string userId);
        Task<ChatConversation> CreateConversationAsync(string userId, string title);
        Task AddMessageAsync(int conversationId, string role, string content);
        Task<List<ChatMessage>> GetMessagesAsync(int conversationId);
        Task DeleteConversationAsync(int conversationId);
        Task UpdateConversationTitleAsync(int conversationId, string title);
    }

    public class ChatHistoryService : IChatHistoryService
    {
        private readonly ApplicationDbContext _context;

        public ChatHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatConversation>> GetUserConversationsAsync(string userId)
        {
            return await _context.ChatConversations
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ChatConversation> CreateConversationAsync(string userId, string title)
        {
            var conversation = new ChatConversation
            {
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true
            };
            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task AddMessageAsync(int conversationId, string role, string content)
        {
            var message = new ChatMessage
            {
                ConversationId = conversationId,
                Role = role,
                Content = content,
                CreatedAt = DateTime.Now
            };
            _context.ChatMessages.Add(message);

            // Update conversation updated time
            var conversation = await _context.ChatConversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.UpdatedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(int conversationId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteConversationAsync(int conversationId)
        {
            var conversation = await _context.ChatConversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateConversationTitleAsync(int conversationId, string title)
        {
            var conversation = await _context.ChatConversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.Title = title;
                await _context.SaveChangesAsync();
            }
        }
    }
}
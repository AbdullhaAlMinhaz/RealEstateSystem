using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;

namespace RealEstateSystem.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        private int? GetUserId()
        {
            return Context.GetHttpContext()?.Session.GetInt32("UserId");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                Context.Abort();
                return;
            }

            // Put each user in their own group for direct messaging
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId.Value}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId != null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId.Value}");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int toUserId, string text)
        {
            var fromUserId = GetUserId();
            if (fromUserId == null) return;

            text = (text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            // Normalize pair
            var a = Math.Min(fromUserId.Value, toUserId);
            var b = Math.Max(fromUserId.Value, toUserId);

            var convo = await _db.ChatConversations
                .FirstOrDefaultAsync(c => c.UserAId == a && c.UserBId == b);

            if (convo == null)
            {
                convo = new ChatConversation { UserAId = a, UserBId = b };
                _db.ChatConversations.Add(convo);
                await _db.SaveChangesAsync();
            }

            var msg = new ChatMessage
            {
                ConversationId = convo.ConversationId,
                SenderUserId = fromUserId.Value,
                ReceiverUserId = toUserId,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            _db.ChatMessages.Add(msg);
            convo.LastMessageAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Send to both users in real-time
            await Clients.Group($"user-{fromUserId.Value}")
                .SendAsync("ReceiveMessage", new
                {
                    conversationId = convo.ConversationId,
                    fromUserId = fromUserId.Value,
                    toUserId,
                    text = msg.Text,
                    sentAt = msg.SentAt
                });

            await Clients.Group($"user-{toUserId}")
                .SendAsync("ReceiveMessage", new
                {
                    conversationId = convo.ConversationId,
                    fromUserId = fromUserId.Value,
                    toUserId,
                    text = msg.Text,
                    sentAt = msg.SentAt
                });
        }
    }
}

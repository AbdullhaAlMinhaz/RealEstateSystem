using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;

namespace RealEstateSystem.Controllers
{
    [Route("chat")]
    public class ChatApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ChatApiController(ApplicationDbContext db) { _db = db; }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        [HttpGet("contacts")]
        public IActionResult Contacts(string? q)
        {
            if (UserId == null) return Unauthorized();
            int me = UserId.Value;

            // 1️⃣ Recent conversations first
            var recentChats = _db.ChatConversations
                .Where(c => c.UserAId == me || c.UserBId == me)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    OtherUserId = c.UserAId == me ? c.UserBId : c.UserAId,
                    c.LastMessageAt,
                    LastMessage = _db.ChatMessages
                        .Where(m => m.ConversationId == c.ConversationId)
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Text)
                        .FirstOrDefault()
                })
                .ToList();

            // 2️⃣ Join user data
            var result = recentChats
                .Join(_db.Users,
                      rc => rc.OtherUserId,
                      u => u.UserId,
                      (rc, u) => new
                      {
                          userId = u.UserId,
                          name = (u.FirstName + " " + u.LastName).Trim(),
                          role = u.Role.ToString(),
                          photo = u.ProfilePhoto,
                          lastMessage = rc.LastMessage ?? "",
                          lastMessageAt = rc.LastMessageAt
                      })
                .ToList();

            // 3️⃣ Search (fallback to all users if searching)
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.ToLower();

                var searchedUsers = _db.Users
                    .Where(u => u.UserId != me &&
                                (u.FirstName + " " + u.LastName).ToLower().Contains(q))
                    .Select(u => new
                    {
                        userId = u.UserId,
                        name = (u.FirstName + " " + u.LastName).Trim(),
                        role = u.Role.ToString(),
                        photo = u.ProfilePhoto,
                        lastMessage = "",
                        lastMessageAt = (DateTime?)null
                    });

                return Json(searchedUsers.ToList());
            }

            return Json(result);
        }

        [HttpGet("allusers")]
        public IActionResult AllUsers(string? q)
        {
            if (UserId == null) return Unauthorized();
            int me = UserId.Value;

            var query = _db.Users
                .Where(u => u.IsActive && u.UserId != me);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(q) ||
                    u.Email.ToLower().Contains(q));
            }

            var users = query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Select(u => new
                {
                    userId = u.UserId,
                    name = (u.FirstName + " " + u.LastName).Trim(),
                    role = u.Role.ToString(),
                    photo = u.ProfilePhoto
                })
                .ToList();

            return Json(users);
        }


        [HttpGet("history")]
        public IActionResult History(int withUserId)
        {
            if (UserId == null) return Unauthorized();

            var a = Math.Min(UserId.Value, withUserId);
            var b = Math.Max(UserId.Value, withUserId);

            var convo = _db.ChatConversations
                .FirstOrDefault(c => c.UserAId == a && c.UserBId == b);

            if (convo == null) return Json(Array.Empty<object>());

            var messages = _db.ChatMessages
                .Where(m => m.ConversationId == convo.ConversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    messageId = m.MessageId,
                    fromUserId = m.SenderUserId,
                    toUserId = m.ReceiverUserId,
                    text = m.Text,
                    sentAt = m.SentAt
                })
                .ToList();

            return Json(messages);
        }
    }
}

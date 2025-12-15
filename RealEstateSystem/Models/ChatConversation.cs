using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class ChatConversation
    {
        [Key]
        public int ConversationId { get; set; }

        // Store as ordered pair so (1,5) == (5,1)
        public int UserAId { get; set; }
        public int UserBId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}

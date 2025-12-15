using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        public int ConversationId { get; set; }
        public ChatConversation Conversation { get; set; }

        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}

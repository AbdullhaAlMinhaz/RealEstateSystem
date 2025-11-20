using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public NotificationType NotificationType { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

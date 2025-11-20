using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class SystemActivity
    {
        [Key]
        public int ActivityId { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        public ActivityType ActivityType { get; set; }

        public string Description { get; set; }

        public string EntityType { get; set; }

        public int? EntityId { get; set; }

        public DateTime ActivityDate { get; set; } = DateTime.Now;

        public int? PerformedBy { get; set; }
        public User PerformedByUser { get; set; }
    }
}

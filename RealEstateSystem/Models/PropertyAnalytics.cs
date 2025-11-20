using System;
using System.ComponentModel.DataAnnotations;   // 👈 IMPORTANT

namespace RealEstateSystem.Models
{
    public class PropertyAnalytics
    {
        [Key]                      // 👈 FORCE EF TO TREAT THIS AS PK
        public int AnalyticsId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public int ViewCount { get; set; }

        public int InquiryCount { get; set; }

        public int OfferCount { get; set; }

        public DateTime RecordedDate { get; set; } = DateTime.Now;
    }
}

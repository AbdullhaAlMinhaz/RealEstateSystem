using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSystem.Models
{
    public class Property
    {
        public int PropertyId { get; set; }

        public int SellerId { get; set; }
        public Seller Seller { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public PropertyType PropertyType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string AreaOrLocation { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        [MaxLength(20)]
        public string ZipCode { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }

        public int AreaSqft { get; set; }

        public int? YearBuilt { get; set; }

        public PropertyStatus Status { get; set; } = PropertyStatus.Available;

        public PropertyApprovalStatus ApprovalStatus { get; set; } = PropertyApprovalStatus.Pending;

        // 🔥 THIS IS IMPORTANT PART
        public int? ApprovedBy { get; set; }

        [ForeignKey("ApprovedBy")]
        public User ApprovedByUser { get; set; }
        // 🔥 END

        public DateTime? ApprovalDate { get; set; }

        public int ViewCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        // Navigation collections
        public ICollection<PropertyImage> Images { get; set; }
        public ICollection<PropertyDocument> Documents { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; }
        public ICollection<Inquiry> Inquiries { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Offer> Offers { get; set; }
        public ICollection<PropertyAnalytics> AnalyticsRecords { get; set; }
    }
}

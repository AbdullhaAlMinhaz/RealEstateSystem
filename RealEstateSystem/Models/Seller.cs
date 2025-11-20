using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSystem.Models
{
    public class Seller
    {
        public int SellerId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        // Seller.cs
        [MaxLength(200)]
        public string? AgencyName { get; set; }      // was: string

        [MaxLength(100)]
        public string? AgencyLicense { get; set; }   // was: string

        [MaxLength(250)]
        public string? AgencyAddress { get; set; }   // was: string


        public SellerType SellerType { get; set; }

        public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Pending;

        public int? ApprovedBy { get; set; }

        [ForeignKey("ApprovedBy")]       // 👈 important: bind FK to ApprovedBy
        public User ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Property> Properties { get; set; }
        public ICollection<Inquiry> Inquiries { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Offer> Offers { get; set; }
    }
}

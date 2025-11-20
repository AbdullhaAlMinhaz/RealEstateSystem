using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class Buyer
    {
        [Key]
        public int BuyerId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public string? SavedSearchPreferences { get; set; }   // was: string


        // Navigation
        public ICollection<Wishlist> Wishlists { get; set; }
        public ICollection<Inquiry> Inquiries { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Offer> Offers { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<SearchHistory> SearchHistories { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public Gender? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(250)]
        public string? ProfilePhoto { get; set; }   // <-- make it nullable


        public UserRole Role { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public Seller SellerProfile { get; set; }
        public Buyer BuyerProfile { get; set; }

        public ICollection<SystemActivity> SystemActivities { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Report> ReportsGenerated { get; set; }
    }
}

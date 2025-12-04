using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;  // 👈 for IFormFile
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class AdminProfileViewModel
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Present address")]
        public string PresentAddress { get; set; }

        [Display(Name = "Active status")]
        public bool IsActive { get; set; }

        [Display(Name = "Registered on")]
        public DateTime RegisteredOn { get; set; }

        public string RoleName => "Admin";

        // Path stored in DB
        public string ProfilePhoto { get; set; }

        // File coming from form (device)
        [Display(Name = "Profile image")]
        public IFormFile ProfileImageFile { get; set; }

        // ===== SECURITY / PASSWORD CHANGE =====
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; }
    }
}

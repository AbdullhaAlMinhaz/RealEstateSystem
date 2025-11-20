using System;
using System.ComponentModel.DataAnnotations;
using RealEstateSystem.Models;   // For UserRole, Gender enums

namespace RealEstateSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public UserRole Role { get; set; }   // Buyer / Seller

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        [Required]
        public Gender Gender { get; set; }   // enum: Male / Female

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required, EmailAddress, StringLength(150)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}

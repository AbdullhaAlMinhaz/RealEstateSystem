using System.ComponentModel.DataAnnotations;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class AdminUserEditViewModel
    {
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required, MaxLength(150)]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [MaxLength(20)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}

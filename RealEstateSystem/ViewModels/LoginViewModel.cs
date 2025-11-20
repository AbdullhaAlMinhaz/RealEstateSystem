using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress, StringLength(150)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

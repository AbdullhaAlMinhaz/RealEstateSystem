using System;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class BuyerProfileViewModel
    {
        public int UserId { get; set; }

        // editable fields
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }          // shown, but we won't change it for now
        public string PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // read-only info
        public bool IsActive { get; set; }
        public DateTime RegisteredOn { get; set; }

        // profile photo
        public string ProfilePhoto { get; set; }

        // helpers for display
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string GenderDisplay =>
            Gender?.ToString() ?? "Not specified";

        public string DateOfBirthDisplay =>
            DateOfBirth.HasValue ? DateOfBirth.Value.ToString("dd MMM yyyy") : "Not specified";

        public string RegisteredOnDisplay =>
            RegisteredOn.ToString("dd MMM yyyy");
    }
}

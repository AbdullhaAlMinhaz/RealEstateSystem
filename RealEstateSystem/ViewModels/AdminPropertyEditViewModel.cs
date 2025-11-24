using System.ComponentModel.DataAnnotations;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class AdminPropertyEditViewModel
    {
        public int PropertyId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public PropertyType PropertyType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
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

        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }

        public int AreaSqft { get; set; }

        public int? YearBuilt { get; set; }

        [Required]
        public PropertyStatus Status { get; set; }

        [Required]
        public PropertyApprovalStatus ApprovalStatus { get; set; }

        public bool IsFeatured { get; set; }
    }
}

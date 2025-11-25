using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class SellerPropertyListViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }

        public IEnumerable<Property> Properties { get; set; }
    }

    public class SellerPropertyCreateViewModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public PropertyType PropertyType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, MaxLength(250)]
        public string Address { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; }

        
        [Required, MaxLength(100)]
        public string AreaOrLocation { get; set; }

        
        [Required, MaxLength(100)]
        public string State { get; set; }

        [Required, MaxLength(20)]
        public string ZipCode { get; set; }

        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }

        public int AreaSqft { get; set; }
    }

}

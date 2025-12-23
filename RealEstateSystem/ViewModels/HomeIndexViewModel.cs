using System.Collections.Generic;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class HomeIndexViewModel
    {
        // Search filters
        public string Location { get; set; }
        public PropertyType? PropertyType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalPages { get; set; } = 1;

        // Result cards
        public List<HomePropertyCardViewModel> Properties { get; set; } = new List<HomePropertyCardViewModel>();
    }

    public class HomePropertyCardViewModel
    {
        public int PropertyId { get; set; }
        public decimal Price { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string AreaOrLocation { get; set; }
        public string PrimaryImageUrl { get; set; }
    }
}

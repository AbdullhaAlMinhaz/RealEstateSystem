using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class BuyerPropertyBrowseViewModel
    {
        // Search filters
        public string Location { get; set; }
        public string PropertyType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinBedrooms { get; set; }

        // Results
        public List<BuyerPropertyBrowseItemViewModel> Properties { get; set; }
            = new List<BuyerPropertyBrowseItemViewModel>();

        public int TotalCount => Properties?.Count ?? 0;

        public bool HasFilters =>
            !string.IsNullOrWhiteSpace(Location) ||
            !string.IsNullOrWhiteSpace(PropertyType) ||
            MinPrice.HasValue || MaxPrice.HasValue || MinBedrooms.HasValue;
    }

    public class BuyerPropertyBrowseItemViewModel
    {
        public int PropertyId { get; set; }

        public string Title { get; set; }
        public string SellerName { get; set; }

        public string City { get; set; }
        public string AreaOrLocation { get; set; }
        public string PropertyType { get; set; }

        public decimal Price { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int AreaSqft { get; set; }

        // Main image
        public string PrimaryImageUrl { get; set; }

        // All images (used on details page)
        public List<string> ImageUrls { get; set; } = new List<string>();
        public bool IsInWishlist { get; set; }

    }
}

using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class AdminListingsDetailsViewModel
    {
        public string Range { get; set; } = "30";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalListings { get; set; }

        public List<ListingRow> Listings { get; set; } = new();
    }

    public class ListingRow
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = "";
        public string City { get; set; } = "";
        public string Address { get; set; } = "";

        public string SellerName { get; set; } = "";

        public decimal Price { get; set; }
        public string ApprovalStatus { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }
}

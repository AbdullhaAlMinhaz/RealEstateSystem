using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class AdminSalesDetailsViewModel
    {
        public string Range { get; set; } = "30";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalSoldCount { get; set; }
        public decimal TotalSoldValue { get; set; }

        public List<SalesPropertyRow> SoldProperties { get; set; } = new();
    }

    public class SalesPropertyRow
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = "";
        public string City { get; set; } = "";
        public string Address { get; set; } = "";

        public string SellerName { get; set; } = "";

        public decimal Price { get; set; }
        public DateTime SoldDate { get; set; }   // UpdatedDate fallback
    }
}

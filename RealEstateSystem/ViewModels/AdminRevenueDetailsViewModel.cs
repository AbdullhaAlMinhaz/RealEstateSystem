using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class AdminRevenueDetailsViewModel
    {
        public string Range { get; set; } = "30";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<RevenueInvoiceRow> Invoices { get; set; } = new();
    }

    public class RevenueInvoiceRow
    {
        public int CommissionInvoiceId { get; set; }

        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = "";
        public string PropertyCity { get; set; } = "";
        public string PropertyAddress { get; set; } = "";

        public string SellerName { get; set; } = "";

        public decimal ListingPrice { get; set; }
        public int CommissionRatePercent { get; set; }
        public decimal CommissionAmount { get; set; }

        public DateTime? VerifiedDate { get; set; }
    }
}

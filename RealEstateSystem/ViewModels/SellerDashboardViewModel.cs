using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class SellerDashboardViewModel
    {
        public string SellerName { get; set; }

        public int ActiveListings { get; set; }
        public int PendingApproval { get; set; }
        public int SoldOrRented { get; set; }
        public int TotalInquiriesLast30Days { get; set; }

        public List<SellerDashboardInquiryItem> LatestInquiries { get; set; } =
            new List<SellerDashboardInquiryItem>();

        public List<SellerDashboardOfferItem> RecentOffers { get; set; } =
            new List<SellerDashboardOfferItem>();

        public List<SellerDashboardTopPropertyItem> TopProperties { get; set; } =
            new List<SellerDashboardTopPropertyItem>();

        public List<SellerDashboardMonthlyStat> MonthlyStats { get; set; } =
            new List<SellerDashboardMonthlyStat>();
    }

    public class SellerDashboardInquiryItem
    {
        public string PropertyTitle { get; set; }
        public string BuyerName { get; set; }
        public DateTime InquiryDate { get; set; }

        // For the colored pill
        public string StatusText { get; set; }
        public string StatusCssClass { get; set; }
    }

    public class SellerDashboardOfferItem
    {
        public string PropertyTitle { get; set; }
        public string BuyerName { get; set; }
        public decimal OfferAmount { get; set; }

        // For the colored badge
        public string StatusText { get; set; }
        public string StatusBadgeClass { get; set; }
    }

    public class SellerDashboardTopPropertyItem
    {
        public string PropertyTitle { get; set; }
        public int Views { get; set; }
        public int Inquiries { get; set; }
        public int Offers { get; set; }
    }

    public class SellerDashboardMonthlyStat
    {
        public string MonthLabel { get; set; }   // e.g. "Jan"
        public int Views { get; set; }
        public int Inquiries { get; set; }
        public int Offers { get; set; }
    }
}

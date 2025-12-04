using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class BuyerDashboardViewModel
    {
        public string BuyerName { get; set; }

        // top cards
        public int SavedPropertiesCount { get; set; }
        public int ActiveOffersCount { get; set; }
        public int UpcomingVisitsCount { get; set; }

        // middle cards
        public NextVisitViewModel NextVisit { get; set; }
        public LatestOfferViewModel LatestOffer { get; set; }

        // bottom cards
        public List<BuyerDashboardPropertyItemViewModel> Properties { get; set; }
            = new List<BuyerDashboardPropertyItemViewModel>();

        public List<BuyerDashboardActivityItemViewModel> RecentActivities { get; set; }
            = new List<BuyerDashboardActivityItemViewModel>();
    }

    public class BuyerDashboardPropertyItemViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public int Bedrooms { get; set; }
        public int AreaSqft { get; set; }
        public decimal Price { get; set; }
        public bool IsInWishlist { get; set; }
        public int ThumbIndex { get; set; }   // 1,2,3 -> thumb-1, thumb-2, thumb-3
    }

    public class BuyerDashboardActivityItemViewModel
    {
        public string Text { get; set; }
        public string Meta { get; set; }
        public string Color { get; set; }     // "blue", "green", "yellow"
    }

    public class NextVisitViewModel
    {
        public bool HasVisit { get; set; }
        public int AppointmentId { get; set; }
        public string PropertyTitle { get; set; }
        public string Location { get; set; }
        public DateTime? VisitDateTime { get; set; }
        public string SellerName { get; set; }
    }

    public class LatestOfferViewModel
    {
        public bool HasOffer { get; set; }
        public int OfferId { get; set; }
        public string PropertyTitle { get; set; }
        public decimal OfferAmount { get; set; }
        public string Status { get; set; }
        public bool HasReceipt { get; set; }
    }
}

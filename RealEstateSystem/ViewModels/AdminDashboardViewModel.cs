using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalSellers { get; set; }
        public int ActiveListings { get; set; }
        public int SelectedMonths { get; set; } = 6;
        public string RangeLabel { get; set; } = "Last 6 months";


        // Pending approvals table
        public List<PendingPropertyApprovalViewModel> PendingApprovals { get; set; } =
            new List<PendingPropertyApprovalViewModel>();

        // =========================
        // NEW: Dynamic charts data
        // =========================

        // Last 6 months: bars for New listings vs Sold listings
        public List<MonthlyListingSalesPoint> ListingSalesLast6Months { get; set; } =
            new List<MonthlyListingSalesPoint>();

        // Pie legend rows
        public List<PropertyCategorySlice> CategoryDistribution { get; set; } =
            new List<PropertyCategorySlice>();

        // Conic gradient string for pie chart background
        public string CategoryPieGradientCss { get; set; } = "";
    }

    public class MonthlyListingSalesPoint
    {
        public string MonthLabel { get; set; } = "";
        public int NewListings { get; set; }
        public int SoldListings { get; set; }

        // heights in px (calculated in controller)
        public int NewHeightPx { get; set; }
        public int SoldHeightPx { get; set; }
    }

    public class PropertyCategorySlice
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
        public int Percent { get; set; } // integer percent like 45
        public string ColorHex { get; set; } = "#cccccc"; // used for pie gradient
        public string CssClass { get; set; } = "";        // used for legend color dot
    }
}

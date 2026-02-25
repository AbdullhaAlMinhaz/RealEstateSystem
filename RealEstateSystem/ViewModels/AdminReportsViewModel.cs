using System;
using System.Collections.Generic;

namespace RealEstateSystem.ViewModels
{
    public class AdminReportsViewModel
    {
        // Filter info
        public string Range { get; set; } = "30";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //  NEW (tab)
        public string ActiveReport { get; set; } = "sales";

        // Top cards
        public decimal TotalRevenue { get; set; }
        public int CompletedSales { get; set; }
        public int NewListings { get; set; }

        //  NEW (trends)
        public decimal RevenueChangePercent { get; set; } = 0;
        public int SalesChangeValue { get; set; } = 0;
        public decimal ListingsChangePercent { get; set; } = 0;

        // Table rows
        public List<AdminReportRow> Rows { get; set; } = new();

        // Quick insights
        public string BestPeriodLabel { get; set; } = "-";
        public decimal BestPeriodConversion { get; set; } = 0;

        public int AvgTimeToSellDays { get; set; } = 0;
        public string TopCategory { get; set; } = "-";

        // Activity breakdown
        public int ActivityNew { get; set; }
        public int ActivitySold { get; set; }
        public int ActivityCancelled { get; set; }
    }

    public class AdminReportRow
    {
        public string PeriodLabel { get; set; } = "";
        public int NewListings { get; set; }
        public int Sold { get; set; }
        public int Cancelled { get; set; }
        public int ConversionPercent { get; set; }
    }
}

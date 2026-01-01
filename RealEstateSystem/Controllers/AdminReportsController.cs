using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateSystem.Controllers
{
    public class AdminReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminReports
        public async Task<IActionResult> Index(string range = "30", DateTime? startDate = null, DateTime? endDate = null, string report = "sales")
        {
            // range: "7", "30", "90", "365" or "custom"
            // report: "revenue" | "sales" | "listings"

            // 1) Resolve date range
            DateTime end = (endDate ?? DateTime.Today).Date.AddDays(1).AddTicks(-1); // end-of-day
            DateTime start;

            if (range == "custom" && startDate.HasValue && endDate.HasValue)
            {
                start = startDate.Value.Date;
                end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                int days = 30;
                if (int.TryParse(range, out var parsedDays))
                    days = parsedDays;

                start = DateTime.Today.AddDays(-days + 1).Date;
            }

            // 2) Pull data from DB
            // Revenue = Paid CommissionInvoices sum (VerifiedDate in range)
            var paidInvoicesQuery = _context.CommissionInvoices
                .Where(i => i.Status == CommissionInvoiceStatus.Paid && i.VerifiedDate != null)
                .Where(i => i.VerifiedDate >= start && i.VerifiedDate <= end);

            decimal totalRevenue = await paidInvoicesQuery.SumAsync(i => (decimal?)i.CommissionAmount) ?? 0m;

            // Completed sales = Properties marked Sold in range (we use UpdatedDate as sold date fallback)
            var soldPropsQuery = _context.Properties
                .Where(p => p.Status == PropertyStatus.Sold)
                .Where(p =>
                    (p.UpdatedDate != null && p.UpdatedDate >= start && p.UpdatedDate <= end) ||
                    (p.UpdatedDate == null && p.CreatedDate >= start && p.CreatedDate <= end)
                );

            int completedSales = await soldPropsQuery.CountAsync();

            // New listings = Properties created in range
            int newListings = await _context.Properties
                .Where(p => p.CreatedDate >= start && p.CreatedDate <= end)
                .CountAsync();

            // Cancelled listings = Rejected in range (if your system uses Rejected as cancelled)
            int cancelled = await _context.Properties
                .Where(p => p.ApprovalStatus == PropertyApprovalStatus.Rejected)
                .Where(p => (p.UpdatedDate ?? p.CreatedDate) >= start && (p.UpdatedDate ?? p.CreatedDate) <= end)
                .CountAsync();

            // 3) Build weekly rows (7-day buckets from start to end)
            var rows = new List<AdminReportRow>();
            DateTime cursor = start.Date;
            int bucketIndex = 1;

            while (cursor <= end.Date)
            {
                DateTime bucketStart = cursor;
                DateTime bucketEnd = cursor.AddDays(6).Date.AddDays(1).AddTicks(-1);
                if (bucketEnd > end) bucketEnd = end;

                int bucketNew = await _context.Properties
                    .Where(p => p.CreatedDate >= bucketStart && p.CreatedDate <= bucketEnd)
                    .CountAsync();

                int bucketSold = await _context.Properties
                    .Where(p => p.Status == PropertyStatus.Sold)
                    .Where(p =>
                        (p.UpdatedDate != null && p.UpdatedDate >= bucketStart && p.UpdatedDate <= bucketEnd) ||
                        (p.UpdatedDate == null && p.CreatedDate >= bucketStart && p.CreatedDate <= bucketEnd)
                    )
                    .CountAsync();

                int bucketCancelled = await _context.Properties
                    .Where(p => p.ApprovalStatus == PropertyApprovalStatus.Rejected)
                    .Where(p => (p.UpdatedDate ?? p.CreatedDate) >= bucketStart && (p.UpdatedDate ?? p.CreatedDate) <= bucketEnd)
                    .CountAsync();

                int conversion = 0;
                if (bucketNew > 0)
                    conversion = (int)Math.Round((bucketSold * 100.0) / bucketNew);

                rows.Add(new AdminReportRow
                {
                    PeriodLabel = $"Week {bucketIndex}",
                    NewListings = bucketNew,
                    Sold = bucketSold,
                    Cancelled = bucketCancelled,
                    ConversionPercent = conversion
                });

                cursor = cursor.AddDays(7);
                bucketIndex++;
            }

            // 4) Quick insights
            var best = rows.OrderByDescending(r => r.ConversionPercent).FirstOrDefault();
            string bestLabel = best?.PeriodLabel ?? "-";
            decimal bestConv = best?.ConversionPercent ?? 0;

            // Avg time to sell (days) from CreatedDate -> UpdatedDate (sold mark should update UpdatedDate)
            var soldForAvg = await _context.Properties
                .Where(p => p.Status == PropertyStatus.Sold)
                .Where(p => p.UpdatedDate != null)
                .Select(p => new { p.CreatedDate, p.UpdatedDate })
                .ToListAsync();

            int avgDaysToSell = 0;
            if (soldForAvg.Any())
            {
                avgDaysToSell = (int)Math.Round(soldForAvg
                    .Average(x => (x.UpdatedDate.Value - x.CreatedDate).TotalDays));
                if (avgDaysToSell < 0) avgDaysToSell = 0;
            }

            // Top category by sold count in the period
            string topCategory = "-";
            var topCat = await _context.Properties
                .Where(p => p.Status == PropertyStatus.Sold)
                .Where(p =>
                    (p.UpdatedDate != null && p.UpdatedDate >= start && p.UpdatedDate <= end) ||
                    (p.UpdatedDate == null && p.CreatedDate >= start && p.CreatedDate <= end)
                )
                .GroupBy(p => p.PropertyType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            if (topCat != null) topCategory = topCat.Type;

            // Activity breakdown
            int activityNew = newListings;
            int activitySold = completedSales;
            int activityCancelled = cancelled;

            // 5) (Optional) Trends vs previous same-length period
            var daysSpan = (end.Date - start.Date).Days + 1;
            DateTime prevEnd = start.AddDays(-1).Date.AddDays(1).AddTicks(-1);
            DateTime prevStart = start.AddDays(-daysSpan).Date;

            decimal prevRevenue = await _context.CommissionInvoices
                .Where(i => i.Status == CommissionInvoiceStatus.Paid && i.VerifiedDate != null)
                .Where(i => i.VerifiedDate >= prevStart && i.VerifiedDate <= prevEnd)
                .SumAsync(i => (decimal?)i.CommissionAmount) ?? 0m;

            int prevSales = await _context.Properties
                .Where(p => p.Status == PropertyStatus.Sold)
                .Where(p =>
                    (p.UpdatedDate != null && p.UpdatedDate >= prevStart && p.UpdatedDate <= prevEnd) ||
                    (p.UpdatedDate == null && p.CreatedDate >= prevStart && p.CreatedDate <= prevEnd)
                )
                .CountAsync();

            int prevListings = await _context.Properties
                .Where(p => p.CreatedDate >= prevStart && p.CreatedDate <= prevEnd)
                .CountAsync();

            decimal revenueChangePercent = 0;
            if (prevRevenue > 0)
                revenueChangePercent = Math.Round(((totalRevenue - prevRevenue) / prevRevenue) * 100m, 2);

            int salesChangeValue = completedSales - prevSales;

            decimal listingsChangePercent = 0;
            if (prevListings > 0)
                listingsChangePercent = Math.Round(((newListings - prevListings) / (decimal)prevListings) * 100m, 2);

            // 6) Final ViewModel
            var model = new AdminReportsViewModel
            {
                Range = range,
                StartDate = start,
                EndDate = end,

                ActiveReport = report,

                TotalRevenue = totalRevenue,
                CompletedSales = completedSales,
                NewListings = newListings,

                RevenueChangePercent = revenueChangePercent,
                SalesChangeValue = salesChangeValue,
                ListingsChangePercent = listingsChangePercent,

                Rows = rows,

                BestPeriodLabel = bestLabel,
                BestPeriodConversion = bestConv,

                AvgTimeToSellDays = avgDaysToSell,
                TopCategory = topCategory,

                ActivityNew = activityNew,
                ActivitySold = activitySold,
                ActivityCancelled = activityCancelled
            };

            ViewData["PageTitle"] = "Reports";
            ViewData["PageSubtitle"] = "Generate insights and activity reports.";

            return View(model);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Added: months query param for dropdown range (1,2,4,6,12)
        public IActionResult Index(int months = 6)
        {

            ViewData["CommissionPendingCount"] =
                _context.CommissionInvoices.Count(i => i.Status == CommissionInvoiceStatus.PendingVerification);

            ViewData["CommissionPaidCount"] =
                _context.CommissionInvoices.Count(i => i.Status == CommissionInvoiceStatus.Paid);

            ViewData["CommissionTotalPaid"] =
                _context.CommissionInvoices
                    .Where(i => i.Status == CommissionInvoiceStatus.Paid)
                    .Sum(i => (decimal?)i.CommissionAmount) ?? 0m;

            // Only allow these values (prevents invalid query values)
            var allowed = new[] { 1, 2, 4, 6, 12 };
            if (!allowed.Contains(months)) months = 6;

            // =========================
            // Overview cards
            // =========================
            var totalUsers = _context.Users.Count();
            var totalBuyers = _context.Users.Count(u => u.Role == UserRole.Buyer);
            var totalSellers = _context.Users.Count(u => u.Role == UserRole.Seller);

            var activeListings = _context.Properties.Count(p =>
                p.Status == PropertyStatus.Available &&
                p.ApprovalStatus == PropertyApprovalStatus.Approved
            );

            // =========================
            // Pending approvals table
            // =========================
            var pending = _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .Where(p => p.ApprovalStatus == PropertyApprovalStatus.Pending)
                .OrderByDescending(p => p.CreatedDate)
                .Take(4)
                .Select(p => new PendingPropertyApprovalViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    SellerName = p.Seller != null && p.Seller.User != null
                        ? p.Seller.User.FirstName + " " + p.Seller.User.LastName
                        : "Unknown seller",
                    SubmittedAt = p.CreatedDate,
                    ApprovalStatus = p.ApprovalStatus
                })
                .ToList();

            // =========================
            // Chart #1: Listing & Sales Overview (Dynamic months range)
            // =========================
            // For "Last N months", we include current month + (N-1) previous months
            var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-(months - 1));
            var endExclusive = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);

            // Pull only what we need
            var chartProps = _context.Properties
                .AsNoTracking()
                .Where(p =>
                    // created in range (new listings)
                    (p.CreatedDate >= startMonth && p.CreatedDate < endExclusive)
                    ||
                    // sold in range (sold listings)
                    (p.Status == PropertyStatus.Sold &&
                     ((p.UpdatedDate ?? p.CreatedDate) >= startMonth) &&
                     ((p.UpdatedDate ?? p.CreatedDate) < endExclusive))
                )
                .Select(p => new
                {
                    p.CreatedDate,
                    p.Status,
                    SoldDate = (p.UpdatedDate ?? p.CreatedDate)
                })
                .ToList();

            // Build month buckets
            var monthsList = new List<DateTime>();
            for (int i = 0; i < months; i++)
                monthsList.Add(startMonth.AddMonths(i));

            // New listings by created month
            var newCounts = chartProps
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

            // Sold listings by sold month
            var soldCounts = chartProps
                .Where(x => x.Status == PropertyStatus.Sold)
                .GroupBy(x => new { x.SoldDate.Year, x.SoldDate.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

            // Create points
            var rawPoints = monthsList.Select(m =>
            {
                var key = (m.Year, m.Month);
                var monthLabel = m.ToString("MMM", CultureInfo.InvariantCulture);

                var newC = newCounts.ContainsKey(key) ? newCounts[key] : 0;
                var soldC = soldCounts.ContainsKey(key) ? soldCounts[key] : 0;

                return new MonthlyListingSalesPoint
                {
                    MonthLabel = monthLabel,
                    NewListings = newC,
                    SoldListings = soldC
                };
            }).ToList();

            // Scale bar heights
            const int maxBarPx = 120;
            const int minBarPxIfNonZero = 8;

            var maxCount = rawPoints.Max(x => Math.Max(x.NewListings, x.SoldListings));
            if (maxCount < 1) maxCount = 1;

            foreach (var p in rawPoints)
            {
                p.NewHeightPx = p.NewListings == 0
                    ? 2
                    : Math.Max(minBarPxIfNonZero, (int)Math.Round((p.NewListings * 1.0 / maxCount) * maxBarPx));

                p.SoldHeightPx = p.SoldListings == 0
                    ? 2
                    : Math.Max(minBarPxIfNonZero, (int)Math.Round((p.SoldListings * 1.0 / maxCount) * maxBarPx));
            }

            // =========================
            // Chart #2: Property Category Distribution (Current listings)
            // =========================
            var activeByType = _context.Properties
                .AsNoTracking()
                .Where(p => p.Status == PropertyStatus.Available &&
                            p.ApprovalStatus == PropertyApprovalStatus.Approved)
                .GroupBy(p => p.PropertyType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            int totalActive = activeByType.Sum(x => x.Count);

            var typeOrder = new[]
            {
                new { Type = PropertyType.Apartment,  Label = "Apartments", Css = "legend-apartments", Color = "#3b82f6" },
                new { Type = PropertyType.House,      Label = "Houses",     Css = "legend-houses",     Color = "#22c55e" },
                new { Type = PropertyType.Commercial, Label = "Commercial", Css = "legend-commercial", Color = "#f97316" },
                new { Type = PropertyType.Land,       Label = "Land",       Css = "legend-land",       Color = "#6366f1" }
            };

            var slices = new List<PropertyCategorySlice>();
            foreach (var t in typeOrder)
            {
                var count = activeByType.FirstOrDefault(x => x.Type == t.Type)?.Count ?? 0;
                var percent = totalActive == 0 ? 0 : (int)Math.Round((count * 100m) / totalActive);

                slices.Add(new PropertyCategorySlice
                {
                    Label = t.Label,
                    Count = count,
                    Percent = percent,
                    CssClass = t.Css,
                    ColorHex = t.Color
                });
            }

            // Fix rounding so total = 100
            if (totalActive > 0)
            {
                var sum = slices.Sum(s => s.Percent);
                var diff = 100 - sum;
                var biggest = slices.OrderByDescending(s => s.Count).FirstOrDefault();
                if (biggest != null) biggest.Percent += diff;
            }

            // Build conic gradient
            string gradient;
            if (totalActive == 0)
            {
                gradient = "conic-gradient(#e5e7eb 0 100%)";
            }
            else
            {
                int current = 0;
                var parts = new List<string>();
                foreach (var s in slices)
                {
                    var start = current;
                    var end = current + s.Percent;
                    parts.Add($"{s.ColorHex} {start}% {end}%");
                    current = end;
                }
                gradient = $"conic-gradient({string.Join(", ", parts)})";
            }

            // Range label for the dropdown badge
            var rangeLabel = months == 12 ? "Last 1 year" : $"Last {months} months";

            // =========================
            // Build model
            // =========================
            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalBuyers = totalBuyers,
                TotalSellers = totalSellers,
                ActiveListings = activeListings,
                PendingApprovals = pending,

                ListingSalesLast6Months = rawPoints,
                CategoryDistribution = slices,
                CategoryPieGradientCss = gradient,

                // ✅ New fields for dropdown
                SelectedMonths = months,
                RangeLabel = rangeLabel
            };

            ViewData["PageTitle"] = "Dashboard";
            ViewData["PageSubtitle"] = "Overview of users, listings and system activity.";

            return View(model);
        }
    }
}

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using System.Collections.Generic;

namespace RealEstateSystem.Controllers
{
    public class SellerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SellerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get Seller profile for this logged-in user
            var seller = _context.Sellers
                .Include(s => s.User)
                .FirstOrDefault(s => s.UserId == userId.Value);

            if (seller == null)
            {
                // Not a seller
                return RedirectToAction("Index", "Home");
            }

            var sellerId = seller.SellerId;

            var propertiesQuery = _context.Properties
                .Where(p => p.SellerId == sellerId);

            // TOP CARDS
            var activeListings = propertiesQuery.Count(p =>
                p.Status == PropertyStatus.Available &&
                p.ApprovalStatus == PropertyApprovalStatus.Approved);

            var pendingApproval = propertiesQuery.Count(p =>
                p.ApprovalStatus == PropertyApprovalStatus.Pending);

            var soldOrRented = propertiesQuery.Count(p =>
                p.Status == PropertyStatus.Sold ||
                p.Status == PropertyStatus.Rented);

            var since = DateTime.Now.AddDays(-30);
            var totalInquiriesLast30Days = _context.Inquiries
                .Include(i => i.Property)
                .Count(i =>
                    i.Property.SellerId == sellerId &&
                    i.InquiryDate >= since);

            // LATEST INQUIRIES (top 5)
            var inquiriesRaw = _context.Inquiries
                .Include(i => i.Property)
                .Include(i => i.Buyer)
                    .ThenInclude(b => b.User)
                .Where(i => i.Property.SellerId == sellerId)
                .OrderByDescending(i => i.InquiryDate)
                .Take(5)
                .ToList();

            var latestInquiries = inquiriesRaw
                .Select(i =>
                {
                    var (statusText, cssClass) = MapInquiryStatus(i.InquiryStatus);

                    return new SellerDashboardInquiryItem
                    {
                        PropertyTitle = i.Property?.Title ?? "Unknown property",
                        BuyerName = i.Buyer?.User != null
                            ? $"{i.Buyer.User.FirstName} {i.Buyer.User.LastName}"
                            : "Unknown buyer",
                        InquiryDate = i.InquiryDate,
                        StatusText = statusText,
                        StatusCssClass = cssClass
                    };
                })
                .ToList();

            // RECENT OFFERS (top 5)
            var offersRaw = _context.Offers
                .Include(o => o.Property)
                .Include(o => o.Buyer)
                    .ThenInclude(b => b.User)
                .Where(o => o.SellerId == sellerId)
                .OrderByDescending(o => o.OfferDate)
                .Take(5)
                .ToList();

            var recentOffers = offersRaw
                .Select(o =>
                {
                    var (statusText, badgeClass) = MapOfferStatus(o.OfferStatus);

                    return new SellerDashboardOfferItem
                    {
                        PropertyTitle = o.Property?.Title ?? "Unknown property",
                        BuyerName = o.Buyer?.User != null
                            ? $"{o.Buyer.User.FirstName} {o.Buyer.User.LastName}"
                            : "Unknown buyer",
                        OfferAmount = o.OfferAmount,
                        StatusText = statusText,
                        StatusBadgeClass = badgeClass
                    };
                })
                .ToList();

            // TOP PERFORMING PROPERTIES (by ViewCount)
            var topPropsRaw = _context.Properties
                .Include(p => p.Inquiries)
                .Include(p => p.Offers)
                .Where(p => p.SellerId == sellerId &&
                            p.ApprovalStatus == PropertyApprovalStatus.Approved)
                .OrderByDescending(p => p.ViewCount)
                .Take(5)
                .ToList();

            var topProps = topPropsRaw
                .Select(p => new SellerDashboardTopPropertyItem
                {
                    PropertyTitle = p.Title,
                    Views = p.ViewCount,
                    Inquiries = p.Inquiries?.Count ?? 0,
                    Offers = p.Offers?.Count ?? 0
                })
                .ToList();

            // MONTHLY STATS (last 6 months) for the mini chart
            var now = DateTime.Now;
            var startMonth = now.AddMonths(-5);

            var analyticsRaw = _context.PropertyAnalytics
                .Include(a => a.Property)
                .Where(a =>
                    a.Property.SellerId == sellerId &&
                    (a.Year > startMonth.Year ||
                     (a.Year == startMonth.Year && a.Month >= startMonth.Month)))
                .ToList();

            var monthlyStats = new List<SellerDashboardMonthlyStat>();

            for (int i = 0; i < 6; i++)
            {
                var monthDate = new DateTime(now.Year, now.Month, 1).AddMonths(-5 + i);

                var monthData = analyticsRaw
                    .Where(a => a.Year == monthDate.Year && a.Month == monthDate.Month);

                monthlyStats.Add(new SellerDashboardMonthlyStat
                {
                    MonthLabel = monthDate.ToString("MMM"),
                    Views = monthData.Sum(a => a.ViewCount),
                    Inquiries = monthData.Sum(a => a.InquiryCount),
                    Offers = monthData.Sum(a => a.OfferCount)
                });
            }

            var model = new SellerDashboardViewModel
            {
                SellerName = $"{seller.User.FirstName} {seller.User.LastName}",
                ActiveListings = activeListings,
                PendingApproval = pendingApproval,
                SoldOrRented = soldOrRented,
                TotalInquiriesLast30Days = totalInquiriesLast30Days,
                LatestInquiries = latestInquiries,
                RecentOffers = recentOffers,
                TopProperties = topProps,
                MonthlyStats = monthlyStats
            };

            ViewData["PageTitle"] = "Seller Dashboard";
            ViewData["PageSubtitle"] = "Manage your listings, inquiries and offers in one place.";
            ViewData["SellerDisplayName"] = model.SellerName;

            return View(model);
        }

        // Map InquiryStatus enum to text + CSS pill class (matches your HTML design)
        private (string text, string css) MapInquiryStatus(InquiryStatus status)
        {
            switch (status)
            {
                case InquiryStatus.New:
                    return ("New", "pill pill-new");
                case InquiryStatus.Open:
                case InquiryStatus.Replied:
                    return ("Replied", "pill pill-open");
                case InquiryStatus.FollowUp:
                    return ("In follow-up", "pill pill-followup");
                case InquiryStatus.Closed:
                    return ("Closed", "pill pill-closed");
                default:
                    return ("New", "pill pill-new");
            }
        }

        // Map OfferStatus enum to text + CSS badge class
        private (string text, string css) MapOfferStatus(OfferStatus status)
        {
            switch (status)
            {
                case OfferStatus.Pending:
                    return ("Pending", "badge badge-pending");
                case OfferStatus.CounterOffer:
                    return ("Countered", "badge badge-counter");
                case OfferStatus.Accepted:
                    return ("Accepted", "badge badge-accepted");
                case OfferStatus.Rejected:
                    return ("Rejected", "badge badge-rejected");
                default:
                    return ("Pending", "badge badge-pending");
            }
        }
    }
}

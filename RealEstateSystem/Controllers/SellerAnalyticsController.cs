using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;

namespace RealEstateSystem.Controllers
{
    public class SellerAnalyticsController : BaseSellerController
    {
        public SellerAnalyticsController(ApplicationDbContext context)
            : base(context)
        {
        }

        public IActionResult Index()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var sellerPropertyIds = _context.Properties
                .Where(p => p.SellerId == seller.SellerId)
                .Select(p => p.PropertyId)
                .ToList();

            var analytics = _context.PropertyAnalytics
                .Include(a => a.Property)
                .Where(a => sellerPropertyIds.Contains(a.PropertyId))
                .OrderByDescending(a => a.RecordedDate)
                .Take(100)
                .ToList();

            ViewData["PageTitle"] = "Analytics";
            ViewData["PageSubtitle"] = "Performance metrics for your listings.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(analytics);
        }
    }
}

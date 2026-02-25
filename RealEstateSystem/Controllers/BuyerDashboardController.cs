using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class BuyerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuyerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // get logged-in user id from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userIdValue = userId.Value;

            // load buyer by user id
            var buyer = _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.UserId == userIdValue);

            if (buyer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var buyerName = $"{buyer.User?.FirstName} {buyer.User?.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(buyerName))
                buyerName = "Buyer";

            // saved properties
            var savedPropertiesCount = _context.Wishlists
                .Count(w => w.BuyerId == buyer.BuyerId);

            // active offers
            var activeOffersCount = _context.Offers
                .Count(o => o.BuyerId == buyer.BuyerId &&
                            (o.OfferStatus == OfferStatus.Pending ||
                             o.OfferStatus == OfferStatus.CounterOffer));

            // upcoming visits
            var now = DateTime.Now;
            var upcomingVisitsCount = _context.Appointments
                .Count(a => a.BuyerId == buyer.BuyerId &&
                            a.AppointmentDate >= now &&
                            a.Status == AppointmentStatus.Confirmed);

            // next visit
            var nextVisitEntity = _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Seller)
                    .ThenInclude(s => s.User)
                .Where(a => a.BuyerId == buyer.BuyerId &&
                            a.AppointmentDate >= now &&
                            a.Status == AppointmentStatus.Confirmed)
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefault();

            var nextVisit = new NextVisitViewModel();
            if (nextVisitEntity != null)
            {
                nextVisit.HasVisit = true;
                nextVisit.AppointmentId = nextVisitEntity.AppointmentId;
                nextVisit.PropertyTitle = nextVisitEntity.Property?.Title ?? "Property";
                nextVisit.Location = nextVisitEntity.Property?.City ?? string.Empty;
                nextVisit.VisitDateTime = nextVisitEntity.AppointmentDate;

                var sellerName =
                    $"{nextVisitEntity.Seller?.User?.FirstName} {nextVisitEntity.Seller?.User?.LastName}".Trim();
                nextVisit.SellerName = string.IsNullOrWhiteSpace(sellerName) ? "Seller" : sellerName;
            }

            // latest offer
            var latestOfferEntity = _context.Offers
                .Include(o => o.Property)
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderByDescending(o => o.OfferDate)
                .FirstOrDefault();

            var latestOffer = new LatestOfferViewModel();
            if (latestOfferEntity != null)
            {
                latestOffer.HasOffer = true;
                latestOffer.OfferId = latestOfferEntity.OfferId;
                latestOffer.PropertyTitle = latestOfferEntity.Property?.Title ?? "Property";
                latestOffer.OfferAmount = latestOfferEntity.OfferAmount;
                latestOffer.Status = latestOfferEntity.OfferStatus.ToString();

                latestOffer.HasReceipt = _context.Payments
                    .Any(p => p.OfferId == latestOfferEntity.OfferId &&
                              p.PaymentStatus == PaymentStatus.Completed);
            }

            // property list (latest approved & available)
            var wishlistPropertyIds = _context.Wishlists
                .Where(w => w.BuyerId == buyer.BuyerId)
                .Select(w => w.PropertyId)
                .ToHashSet();

            var properties = _context.Properties
                .Where(p => p.Status == PropertyStatus.Available &&
                            p.ApprovalStatus == PropertyApprovalStatus.Approved)
                .OrderByDescending(p => p.CreatedDate)
                .Take(10)
                .ToList();

            int thumbIndex = 1;
            var propertyItems = properties.Select(p =>
            {
                int currentThumb = thumbIndex;
                thumbIndex++;
                if (thumbIndex > 3) thumbIndex = 1;

                return new BuyerDashboardPropertyItemViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    Location = p.City ?? string.Empty,
                    Bedrooms = p.Bedrooms ?? 0,
                    AreaSqft = p.AreaSqft,
                    Price = p.Price,
                    IsInWishlist = wishlistPropertyIds.Contains(p.PropertyId),
                    ThumbIndex = currentThumb
                };
            }).ToList();

            // simple recent activity
            var activities = new[]
            {
                new BuyerDashboardActivityItemViewModel
                {
                    Text = "Welcome back!",
                    Meta = "Just now",
                    Color = "blue"
                },
                new BuyerDashboardActivityItemViewModel
                {
                    Text = $"You have {savedPropertiesCount} properties in your wishlist.",
                    Meta = "Today",
                    Color = "green"
                },
                new BuyerDashboardActivityItemViewModel
                {
                    Text = $"You have {activeOffersCount} active offers.",
                    Meta = "Today",
                    Color = "yellow"
                }
            }.ToList();

            var model = new BuyerDashboardViewModel
            {
                BuyerName = buyerName,
                SavedPropertiesCount = savedPropertiesCount,
                ActiveOffersCount = activeOffersCount,
                UpcomingVisitsCount = upcomingVisitsCount,
                NextVisit = nextVisit,
                LatestOffer = latestOffer,
                Properties = propertyItems,
                RecentActivities = activities
            };

            return View(model);
        }
    }
}

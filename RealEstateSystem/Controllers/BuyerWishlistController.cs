using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class BuyerWishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuyerWishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------- helper: get current buyer + set ViewBag.BuyerName --------
        private Buyer GetCurrentBuyer()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;

            var buyer = _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.UserId == userId.Value);

            if (buyer == null) return null;

            var name = $"{buyer.User?.FirstName} {buyer.User?.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "Buyer";

            ViewBag.BuyerName = name;
            return buyer;
        }

        // -------- GET: /BuyerWishlist --------
        public IActionResult Index()
        {
            var buyer = GetCurrentBuyer();
            if (buyer == null)
                return RedirectToAction("Login", "Account");

            var wishlistItems = _context.Wishlists
                .Include(w => w.Property)
                    .ThenInclude(p => p.Images)
                .Include(w => w.Property.Seller)
                    .ThenInclude(s => s.User)
                .Where(w => w.BuyerId == buyer.BuyerId)
                .OrderByDescending(w => w.AddedDate)
                .ToList();

            var cards = wishlistItems.Select(w =>
            {
                var p = w.Property;

                var images = p.Images?
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .ToList() ?? new List<PropertyImage>();

                var primary = images.FirstOrDefault();
                var primaryUrl = primary?.ImageUrl ?? "/images/property-placeholder.jpg";

                var sellerName = $"{p.Seller?.User?.FirstName} {p.Seller?.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(sellerName))
                    sellerName = "Listing Agent";

                return new BuyerPropertyBrowseItemViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    SellerName = sellerName,
                    City = p.City,
                    AreaOrLocation = p.AreaOrLocation,
                    PropertyType = p.PropertyType.ToString(),
                    Price = p.Price,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    AreaSqft = p.AreaSqft,
                    PrimaryImageUrl = primaryUrl,
                    ImageUrls = images.Select(i => i.ImageUrl).ToList(),
                    IsInWishlist = true
                };
            }).ToList();

            return View(cards);
        }

        // -------- POST: /BuyerWishlist/ --------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int propertyId)
        {
            var buyer = GetCurrentBuyer();
            if (buyer == null)
                return RedirectToAction("Login", "Account");

            var existing = _context.Wishlists
                .FirstOrDefault(w => w.BuyerId == buyer.BuyerId && w.PropertyId == propertyId);

            if (existing == null)
            {
                // add new wishlist row
                var wish = new Wishlist
                {
                    BuyerId = buyer.BuyerId,
                    PropertyId = propertyId,
                    AddedDate = DateTime.Now
                };
                _context.Wishlists.Add(wish);
            }
            else
            {
                // remove from wishlist
                _context.Wishlists.Remove(existing);
            }

            _context.SaveChanges();

            
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "BuyerProperties");
        }
    }
}

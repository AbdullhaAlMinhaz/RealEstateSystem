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
    public class BuyerPropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuyerPropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Small helper to set buyer name from session (top-right avatar)
        private bool TrySetBuyerName()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var buyer = _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.UserId == userId.Value);

            if (buyer == null) return false;

            var name = $"{buyer.User?.FirstName} {buyer.User?.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "Buyer";

            ViewBag.BuyerName = name;
            return true;
        }

        // ========= BROWSE PROPERTIES =========
        public IActionResult Index(
            string location,
            string propertyType,
            decimal? minPrice,
            decimal? maxPrice,
            int? minBedrooms)
                {
            if (!TrySetBuyerName())
                return RedirectToAction("Login", "Account");

            // ---- find current buyer & their wishlist property IDs ----
            var wishlistPropertyIds = new HashSet<int>();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var buyer = _context.Buyers.FirstOrDefault(b => b.UserId == userId.Value);
                if (buyer != null)
                {
                    wishlistPropertyIds = _context.Wishlists
                        .Where(w => w.BuyerId == buyer.BuyerId)
                        .Select(w => w.PropertyId)
                        .ToHashSet();
                }
            }

            // Base query: only active, approved properties
            var query = _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .Include(p => p.Images)
                .Where(p =>
                    p.Status == PropertyStatus.Available &&
                    p.ApprovalStatus == PropertyApprovalStatus.Approved);

            // --- Filter: location / area / city ---
            if (!string.IsNullOrWhiteSpace(location))
            {
                var term = location.Trim().ToLower();
                query = query.Where(p =>
                    (p.City ?? "").ToLower().Contains(term) ||
                    (p.AreaOrLocation ?? "").ToLower().Contains(term) ||
                    (p.Address ?? "").ToLower().Contains(term));
            }

            // --- Filter: property type (enum) ---
            if (!string.IsNullOrWhiteSpace(propertyType) &&
                Enum.TryParse<PropertyType>(propertyType, out var typeEnum))
            {
                query = query.Where(p => p.PropertyType == typeEnum);
            }

            // --- Filter: price range ---
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // --- Filter: bedrooms ---
            if (minBedrooms.HasValue && minBedrooms.Value > 0)
            {
                query = query.Where(p => (p.Bedrooms ?? 0) >= minBedrooms.Value);
            }

            // Get latest 50 properties (featured first)
            var properties = query
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedDate)
                .Take(50)
                .ToList();

            // Map to card viewmodels (with gallery images)
            var items = properties.Select(p =>
            {
                var images = p.Images?
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .ToList() ?? new List<PropertyImage>();

                var primary = images.FirstOrDefault();
                string primaryUrl = primary?.ImageUrl ?? "/images/property-placeholder.jpg";

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
                    // ⭐ here we mark wishlist status
                    IsInWishlist = wishlistPropertyIds.Contains(p.PropertyId)
                };
            }).ToList();

            var model = new BuyerPropertyBrowseViewModel
            {
                Location = location,
                PropertyType = propertyType,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinBedrooms = minBedrooms,
                Properties = items
            };

            return View(model);
        }


        // ========= PROPERTY DETAILS =========
        public IActionResult Details(int id)
        {
            if (!TrySetBuyerName())
                return RedirectToAction("Login", "Account");

            var property = _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.PropertyId == id &&
                                     p.ApprovalStatus == PropertyApprovalStatus.Approved);

            if (property == null)
            {
                return NotFound();
            }

            var detailsModel = new PropertyDetailsViewModel
            {
                Property = property,
                Images = property.Images?
                    .OrderBy(i => i.DisplayOrder)
                    .ThenByDescending(i => i.IsPrimary),
                SellerName = $"{property.Seller?.User?.FirstName} {property.Seller?.User?.LastName}".Trim(),
                SellerEmail = property.Seller?.User?.Email,
                SellerPhone = property.Seller?.User?.PhoneNumber
            };

            return View(detailsModel);
        }
    }
}

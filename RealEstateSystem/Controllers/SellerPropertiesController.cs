using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class SellerPropertiesController : BaseSellerController
    {
        public SellerPropertiesController(ApplicationDbContext context)
            : base(context)
        {
        }

        // ========== MY PROPERTIES ==========
        public IActionResult Index()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var properties = _context.Properties
                .Where(p => p.SellerId == seller.SellerId)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var model = new SellerPropertyListViewModel
            {
                Title = "My Properties",
                Subtitle = "View and manage all your listed properties.",
                Properties = properties
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(model);
        }

        // ========== ADD NEW PROPERTY (GET) ==========
        [HttpGet]
        public IActionResult Create()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            ViewData["PageTitle"] = "Add New Property";
            ViewData["PageSubtitle"] = "Create a new listing and submit for admin approval.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(new SellerPropertyCreateViewModel());
        }

        // ========== ADD NEW PROPERTY (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SellerPropertyCreateViewModel model)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            ViewData["PageTitle"] = "Add New Property";
            ViewData["PageSubtitle"] = "Create a new listing and submit for admin approval.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            if (!ModelState.IsValid)
            {
                // Validation errors – just show the form again
                return View(model);
            }

            try
            {
                var property = new Property
                {
                    SellerId = seller.SellerId,
                    Title = model.Title,
                    Description = model.Description,
                    PropertyType = model.PropertyType,
                    Price = model.Price,
                    Address = model.Address,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Bedrooms = model.Bedrooms,
                    Bathrooms = model.Bathrooms,
                    AreaSqft = model.AreaSqft,
                    AreaOrLocation = model.AreaOrLocation,
                    State = model.State,

                    Status = PropertyStatus.Available,
                    ApprovalStatus = PropertyApprovalStatus.Pending,
                    CreatedDate = DateTime.Now,
                    SubmittedDate = DateTime.Now,
                    ViewCount = 0,
                    IsFeatured = false
                };

                _context.Properties.Add(property);
                _context.SaveChanges();

                // ✅ success: go back to My Properties
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty, "Error saving property: " + msg);
                return View(model);
            }

        }
    }
}

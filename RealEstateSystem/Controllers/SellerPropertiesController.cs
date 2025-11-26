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

        public IActionResult Pending()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var properties = _context.Properties
                .Where(p => p.SellerId == seller.SellerId &&
                            p.ApprovalStatus == PropertyApprovalStatus.Pending)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var model = new SellerPropertyListViewModel
            {
                Title = "Pending Approval",
                Subtitle = "Properties waiting for admin review.",
                Properties = properties
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View("Index", model);   // reuse the same table view
        }

        public IActionResult SoldOrRented()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var properties = _context.Properties
                .Where(p => p.SellerId == seller.SellerId &&
                           (p.Status == PropertyStatus.Sold ||
                            p.Status == PropertyStatus.Rented))
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var model = new SellerPropertyListViewModel
            {
                Title = "Sold / Rented",
                Subtitle = "Properties that have been sold or rented.",
                Properties = properties
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View("Index", model);   // reuse same table view
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

        // ========== EDIT PROPERTY (GET) ==========
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id && p.SellerId == seller.SellerId);

            if (property == null)
                return NotFound();

            var model = new SellerPropertyEditViewModel
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                PropertyType = property.PropertyType,
                Price = property.Price,
                AreaOrLocation = property.AreaOrLocation,
                State = property.State,
                Description = property.Description,
                Address = property.Address,
                City = property.City,
                ZipCode = property.ZipCode,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                AreaSqft = property.AreaSqft
            };

            ViewData["PageTitle"] = "Edit Property";
            ViewData["PageSubtitle"] = "Update details of your listing.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(model);
        }

        // ========== EDIT PROPERTY (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SellerPropertyEditViewModel model)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            ViewData["PageTitle"] = "Edit Property";
            ViewData["PageSubtitle"] = "Update details of your listing.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var property = _context.Properties
                    .FirstOrDefault(p => p.PropertyId == model.PropertyId &&
                                         p.SellerId == seller.SellerId);

                if (property == null)
                    return NotFound();

                property.Title = model.Title;
                property.PropertyType = model.PropertyType;
                property.Price = model.Price;
                property.AreaOrLocation = model.AreaOrLocation;
                property.State = model.State;
                property.Description = model.Description;
                property.Address = model.Address;
                property.City = model.City;
                property.ZipCode = model.ZipCode;
                property.Bedrooms = model.Bedrooms;
                property.Bathrooms = model.Bathrooms;
                property.AreaSqft = model.AreaSqft;

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty, "Error updating property: " + msg);
                return View(model);
            }
        }


        // ========== DELETE PROPERTY (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id && p.SellerId == seller.SellerId);

            if (property == null)
                return NotFound();

            _context.Properties.Remove(property);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}

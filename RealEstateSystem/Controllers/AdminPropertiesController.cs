using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminPropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- APPROVE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null)
            {
                return Json(new { success = false, message = "Property not found." });
            }

            property.ApprovalStatus = PropertyApprovalStatus.Approved;
            property.Status = PropertyStatus.Available;
            property.ApprovalDate = DateTime.Now;
            property.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            // ✅ AJAX request -> JSON (toast + no page reload)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Property approved successfully." });

            // Normal request -> redirect back safely
            return RedirectToAction("Pending", "AdminApprovals");
        }

        // ---------- REJECT ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null)
            {
                return Json(new { success = false, message = "Property not found." });
            }

            property.ApprovalStatus = PropertyApprovalStatus.Rejected;
            property.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Property rejected." });

            return RedirectToAction("Pending", "AdminApprovals");
        }


        // ---------- EDIT (GET) ----------
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null)
                return NotFound();

            var model = new AdminPropertyEditViewModel
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Description = property.Description,
                PropertyType = property.PropertyType,
                Price = property.Price,
                Address = property.Address,
                City = property.City,
                AreaOrLocation = property.AreaOrLocation,
                State = property.State,
                ZipCode = property.ZipCode,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                AreaSqft = property.AreaSqft,
                YearBuilt = property.YearBuilt,
                Status = property.Status,
                ApprovalStatus = property.ApprovalStatus,
                IsFeatured = property.IsFeatured
            };

            return View(model);
        }

        // ---------- EDIT (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AdminPropertyEditViewModel model)
        {
            if (id != model.PropertyId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null)
                return NotFound();

            property.Title = model.Title;
            property.Description = model.Description;
            property.PropertyType = model.PropertyType;
            property.Price = model.Price;
            property.Address = model.Address;
            property.City = model.City;
            property.AreaOrLocation = model.AreaOrLocation;
            property.State = model.State;
            property.ZipCode = model.ZipCode;
            property.Bedrooms = model.Bedrooms;
            property.Bathrooms = model.Bathrooms;
            property.AreaSqft = model.AreaSqft;
            property.YearBuilt = model.YearBuilt;
            property.Status = model.Status;
            property.ApprovalStatus = model.ApprovalStatus;
            property.IsFeatured = model.IsFeatured;
            property.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Property updated successfully.";
            return RedirectToAction("ActiveListings");
        }

        // ---------- DETAILS (ADMIN VIEW) ----------
        [HttpGet]
        public IActionResult Details(int id)
        {
            var property = _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefault(p => p.PropertyId == id);

            if (property == null)
                return NotFound();

            var user = property.Seller?.User;

            var model = new PropertyDetailsViewModel
            {
                Property = property,
                Images = property.Images ?? Enumerable.Empty<PropertyImage>(),
                SellerName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                SellerEmail = user?.Email,
                SellerPhone = user?.PhoneNumber
            };

            return View(model);
        }

        // ---------- ACTIVE LISTINGS ----------
        public IActionResult ActiveListings()
        {
            var properties = _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .Where(p => p.Status == PropertyStatus.Available &&
                            p.ApprovalStatus == PropertyApprovalStatus.Approved)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var model = new AdminPropertyListViewModel
            {
                Title = "Active Listings",
                Subtitle = "Properties currently visible to buyers.",
                Properties = properties
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("ActiveListings", model);
        }
    }
}

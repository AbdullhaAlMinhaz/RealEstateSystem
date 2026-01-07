using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using RealEstateSystem.Services.Email;

namespace RealEstateSystem.Controllers
{
    public class AdminPropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminPropertiesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int commissionRatePercent)
        {
            var property = await _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found." });
            }

            if (commissionRatePercent < 2 || commissionRatePercent > 5)
            {
                return Json(new { success = false, message = "Commission rate must be between 2% and 5%." });
            }

            property.CommissionRatePercent = commissionRatePercent;

            property.ApprovalStatus = PropertyApprovalStatus.Approved;
            property.Status = PropertyStatus.Available;
            property.ApprovalDate = DateTime.Now;
            property.UpdatedDate = DateTime.Now;

            var adminUserId = HttpContext.Session.GetInt32("UserId");
            if (adminUserId != null)
                property.ApprovedBy = adminUserId.Value;

            await _context.SaveChangesAsync();

            // ✅ EMAIL TO SELLER
            try
            {
                var sellerEmail = property.Seller?.User?.Email;
                var sellerName = $"{property.Seller?.User?.FirstName} {property.Seller?.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(sellerName)) sellerName = "Seller";

                var subject = $"Your Property is Approved: {property.Title}";
                var body =
$@"Hello {sellerName},

Good news! Your property has been approved in the system and is now available for buyers.

Property: {property.Title}
Location: {property.City}, {property.AreaOrLocation}
Commission Rate: {property.CommissionRatePercent}%

Thanks,
Real Estate Property Management System
Developed By Abdullah Al Minhaz";

                if (!string.IsNullOrWhiteSpace(sellerEmail))
                    await _emailService.SendEmailAsync(sellerEmail, subject, body);
            }
            catch
            {
                // Don't break approval if email fails
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Property approved successfully." });

            return RedirectToAction("Pending", "AdminApprovals");
        }

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

//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using RealEstateSystem.Data;
//using RealEstateSystem.Models;
//using RealEstateSystem.ViewModels;

//namespace RealEstateSystem.Controllers
//{


//    public class AdminPropertiesController : Controller
//    {
//        //........................................................................................................
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Approve(int id)
//        {
//            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
//            if (property == null) return NotFound();

//            property.ApprovalStatus = PropertyApprovalStatus.Approved;
//            property.Status = PropertyStatus.Available; // if you have this property
//            _context.SaveChanges();

//            TempData["SuccessMessage"] = "Property approved successfully.";
//            return RedirectToAction("Index", "AdminDashboard");
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Reject(int id)
//        {
//            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
//            if (property == null) return NotFound();

//            property.ApprovalStatus = PropertyApprovalStatus.Rejected;
//            _context.SaveChanges();

//            TempData["SuccessMessage"] = "Property rejected.";
//            return RedirectToAction("Index", "AdminDashboard");
//        }
//        //,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,

//        private readonly ApplicationDbContext _context;

//        public AdminPropertiesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public IActionResult ActiveListings()
//        {
//            var properties = _context.Properties
//                .Include(p => p.Seller)
//                    .ThenInclude(s => s.User)
//                .Where(p => p.Status == PropertyStatus.Available &&
//                            p.ApprovalStatus == PropertyApprovalStatus.Approved)
//                .OrderByDescending(p => p.CreatedDate)
//                .ToList();

//            var model = new AdminPropertyListViewModel
//            {
//                Title = "Active Listings",
//                Subtitle = "Properties currently visible to buyers.",
//                Properties = properties
//            };

//            ViewData["PageTitle"] = model.Title;
//            ViewData["PageSubtitle"] = model.Subtitle;

//            return View("ActiveListings", model);
//        }
//    }
//}


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

        // --------------------------------------------------------------------
        // Approve / Reject actions used from the admin dashboard
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null) return NotFound();

            property.ApprovalStatus = PropertyApprovalStatus.Approved;
            property.Status = PropertyStatus.Available;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Property approved successfully.";
            return RedirectToAction("Index", "AdminDashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null) return NotFound();

            property.ApprovalStatus = PropertyApprovalStatus.Rejected;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Property rejected.";
            return RedirectToAction("Index", "AdminDashboard");
        }

        // --------------------------------------------------------------------
        // GET: /AdminProperties/Edit/19
        // --------------------------------------------------------------------
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id);

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

            return View(model); // Views/AdminProperties/Edit.cshtml
        }

        // --------------------------------------------------------------------
        // POST: /AdminProperties/Edit/19
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AdminPropertyEditViewModel model)
        {
            if (id != model.PropertyId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id);

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

        // --------------------------------------------------------------------
        // Active listings page (list used in your screenshot)
        // --------------------------------------------------------------------
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

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
        //........................................................................................................
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);
            if (property == null) return NotFound();

            property.ApprovalStatus = PropertyApprovalStatus.Approved;
            property.Status = PropertyStatus.Available; // if you have this property
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
        //,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,

        private readonly ApplicationDbContext _context;

        public AdminPropertiesController(ApplicationDbContext context)
        {
            _context = context;
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

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

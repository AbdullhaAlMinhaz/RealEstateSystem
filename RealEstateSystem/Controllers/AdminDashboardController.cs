using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ✅ inject the database context
        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ still the same action name & route: /AdminDashboard/Index
        public IActionResult Index()
        {
            // build the dashboard stats from the database
            var model = new AdminDashboardViewModel
            {
                // all users (Admin + Seller + Buyer)
                TotalUsers = _context.Users.Count(),

                // only buyers
                TotalBuyers = _context.Users.Count(u => u.Role == UserRole.Buyer),

                // only sellers
                TotalSellers = _context.Users.Count(u => u.Role == UserRole.Seller),

                // active, approved properties
                ActiveListings = _context.Properties.Count(p =>
                    p.Status == PropertyStatus.Available &&
                    p.ApprovalStatus == PropertyApprovalStatus.Approved)
            };

            // optional: used by _AdminLayout for the heading text
            ViewData["PageTitle"] = "Dashboard";
            ViewData["PageSubtitle"] = "Overview of users, listings and system activity.";

            // return the same view, but now with data
            return View(model);
        }
    }
}

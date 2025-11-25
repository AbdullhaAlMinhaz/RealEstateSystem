//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using RealEstateSystem.Data;
//using RealEstateSystem.Models;
//using RealEstateSystem.ViewModels;

//namespace RealEstateSystem.Controllers
//{
//    public class AdminDashboardController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        // ✅ inject the database context
//        public AdminDashboardController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // ✅ still the same action name & route: /AdminDashboard/Index
//        public IActionResult Index()
//        {
//            // build the dashboard stats from the database
//            var model = new AdminDashboardViewModel
//            {
//                // all users (Admin + Seller + Buyer)
//                TotalUsers = _context.Users.Count(),

//                // only buyers
//                TotalBuyers = _context.Users.Count(u => u.Role == UserRole.Buyer),

//                // only sellers
//                TotalSellers = _context.Users.Count(u => u.Role == UserRole.Seller),

//                // active, approved properties
//                ActiveListings = _context.Properties.Count(p =>
//                    p.Status == PropertyStatus.Available &&
//                    p.ApprovalStatus == PropertyApprovalStatus.Approved)
//            };

//            // optional: used by _AdminLayout for the heading text
//            ViewData["PageTitle"] = "Dashboard";
//            ViewData["PageSubtitle"] = "Overview of users, listings and system activity.";

//            // return the same view, but now with data
//            return View(model);
//        }
//    }
//}


using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalUsers = _context.Users.Count();
            var totalBuyers = _context.Users.Count(u => u.Role == UserRole.Buyer);
            var totalSellers = _context.Users.Count(u => u.Role == UserRole.Seller);
            var activeListings = _context.Properties
                                         .Count(p => p.Status == PropertyStatus.Available &&
                                                     p.ApprovalStatus == PropertyApprovalStatus.Approved);

            // take latest 4 pending properties
            var pending = _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .Where(p => p.ApprovalStatus == PropertyApprovalStatus.Pending)
                .OrderByDescending(p => p.CreatedDate)
                .Take(4)
                .Select(p => new PendingPropertyApprovalViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    SellerName = p.Seller != null && p.Seller.User != null
                        ? p.Seller.User.FirstName + " " + p.Seller.User.LastName
                        : "Unknown seller",
                    SubmittedAt = p.CreatedDate,
                    ApprovalStatus = p.ApprovalStatus
                })
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalBuyers = totalBuyers,
                TotalSellers = totalSellers,
                ActiveListings = activeListings,
                PendingApprovals = pending
            };

            ViewData["PageTitle"] = "Dashboard";
            ViewData["PageSubtitle"] = "Overview of users, listings and system activity.";

            return View(model);
        }
    }
}

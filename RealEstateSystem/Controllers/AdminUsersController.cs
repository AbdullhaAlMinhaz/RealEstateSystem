using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // All users
        public IActionResult Index()
        {
            var model = new AdminUserListViewModel
            {
                Title = "All Users",
                Subtitle = "All registered users in the system.",
                Users = _context.Users.OrderBy(u => u.UserId).ToList()
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("UserList", model);
        }

        // Buyers only
        public IActionResult Buyers()
        {
            var model = new AdminUserListViewModel
            {
                Title = "Buyers",
                Subtitle = "Registered buyers and their accounts.",
                Users = _context.Users
                                    .Where(u => u.Role == UserRole.Buyer)
                                    .OrderBy(u => u.UserId)
                                    .ToList()
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("UserList", model);
        }

        // Sellers only
        public IActionResult Sellers()
        {
            var model = new AdminUserListViewModel
            {
                Title = "Sellers",
                Subtitle = "Registered sellers and their accounts.",
                Users = _context.Users
                                    .Where(u => u.Role == UserRole.Seller)
                                    .OrderBy(u => u.UserId)
                                    .ToList()
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("UserList", model);
        }
    }
}

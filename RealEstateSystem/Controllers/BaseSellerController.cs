using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;

namespace RealEstateSystem.Controllers
{
    // Not a route by itself – inherited by all seller controllers
    public abstract class BaseSellerController : Controller
    {
        protected readonly ApplicationDbContext _context;

        protected BaseSellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected Seller GetCurrentSeller()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return null;

            return _context.Sellers
                .Include(s => s.User)
                .FirstOrDefault(s => s.UserId == userId.Value);
        }

        protected IActionResult RedirectToLoginIfNotSeller(out Seller seller)
        {
            seller = GetCurrentSeller();
            if (seller == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return null; // OK
        }
    }
}

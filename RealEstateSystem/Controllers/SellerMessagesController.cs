using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;

namespace RealEstateSystem.Controllers
{
    public class SellerMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SellerMessagesController(ApplicationDbContext context) { _context = context; }

        private bool TrySetSellerName()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var seller = _context.Sellers
                .Include(s => s.User)
                .FirstOrDefault(s => s.UserId == userId.Value);

            if (seller == null) return false;

            var name = $"{seller.User?.FirstName} {seller.User?.LastName}".Trim();
            ViewBag.SellerName = string.IsNullOrWhiteSpace(name) ? "Seller" : name;
            return true;
        }

        public IActionResult Index()
        {
            if (!TrySetSellerName()) return RedirectToAction("Login", "Account");
            return View();
        }
    }
}

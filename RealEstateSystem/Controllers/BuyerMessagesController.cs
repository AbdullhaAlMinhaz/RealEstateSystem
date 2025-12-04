using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;

namespace RealEstateSystem.Controllers
{
    public class BuyerMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuyerMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool TrySetBuyerName()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var buyer = _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.UserId == userId.Value);

            if (buyer == null) return false;

            var name = $"{buyer.User?.FirstName} {buyer.User?.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "Buyer";

            ViewBag.BuyerName = name;
            return true;
        }

        public IActionResult Index()
        {
            if (!TrySetBuyerName())
                return RedirectToAction("Login", "Account");

            // later: load inquiries / messages
            return View();
        }
    }
}

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;

namespace RealEstateSystem.Controllers
{
    public class SellerOffersController : BaseSellerController
    {
        public SellerOffersController(ApplicationDbContext context)
            : base(context)
        {
        }

        public IActionResult Index()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var offers = _context.Offers
                .Include(o => o.Property)
                .Include(o => o.Buyer).ThenInclude(b => b.User)
                .Where(o => o.SellerId == seller.SellerId)
                .OrderByDescending(o => o.OfferDate)
                .ToList();

            ViewData["PageTitle"] = "Offers";
            ViewData["PageSubtitle"] = "Review and respond to buyer offers.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(offers);
        }
    }
}

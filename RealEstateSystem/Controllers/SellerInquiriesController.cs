using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;

namespace RealEstateSystem.Controllers
{
    public class SellerInquiriesController : BaseSellerController
    {
        public SellerInquiriesController(ApplicationDbContext context)
            : base(context)
        {
        }

        public IActionResult Index()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var inquiries = _context.Inquiries
                .Include(i => i.Property)
                .Include(i => i.Buyer).ThenInclude(b => b.User)
                .Where(i => i.Property.SellerId == seller.SellerId)
                .OrderByDescending(i => i.InquiryDate)
                .ToList();

            ViewData["PageTitle"] = "Inquiries";
            ViewData["PageSubtitle"] = "Handle buyer questions and messages.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(inquiries);
        }
    }
}

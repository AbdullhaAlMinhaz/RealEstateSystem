using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;

namespace RealEstateSystem.Controllers
{
    public class SellerAppointmentsController : BaseSellerController
    {
        public SellerAppointmentsController(ApplicationDbContext context)
            : base(context)
        {
        }

        public IActionResult Index()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var appointments = _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Buyer).ThenInclude(b => b.User)
                .Where(a => a.SellerId == seller.SellerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            ViewData["PageTitle"] = "Appointments";
            ViewData["PageSubtitle"] = "View scheduled property visits.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(appointments);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using System.Linq;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, AppointmentStatus status)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var appointment = _context.Appointments
                .FirstOrDefault(a => a.AppointmentId == id && a.SellerId == seller.SellerId);

            if (appointment == null)
                return NotFound();

            appointment.Status = status;
            appointment.UpdatedDate = DateTime.Now;


            //........ Message for the seller in pop up to show the notification..................


            string message;
            switch (status)
            {
                case AppointmentStatus.Confirmed:
                    message = "You have confirmed this appointment.";
                    break;
                case AppointmentStatus.Cancelled:
                    message = "You have cancelled this appointment.";
                    break;
                case AppointmentStatus.Completed:
                    message = "You marked this appointment as completed.";
                    break;
                default:
                    message = "Appointment updated.";
                    break;
            }

            appointment.UpdatedDate = DateTime.Now;
            _context.SaveChanges();

            TempData["AppointmentSuccess"] = message;

            _context.SaveChanges();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index");
        }


    }
}

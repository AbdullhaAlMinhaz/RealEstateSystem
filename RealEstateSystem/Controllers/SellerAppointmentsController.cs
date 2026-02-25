using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.Services.Email;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateSystem.Controllers
{
    public class SellerAppointmentsController : BaseSellerController
    {
        private readonly IEmailService _emailService;

        public SellerAppointmentsController(ApplicationDbContext context, IEmailService emailService)
            : base(context)
        {
            _emailService = emailService;
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
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var appointment = await _context.Appointments
                .Include(a => a.Buyer).ThenInclude(b => b.User)
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.SellerId == seller.SellerId);

            if (appointment == null)
                return NotFound();

            appointment.Status = status;
            appointment.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            //  POPUP message for seller
            string sellerPopup;
            switch (status)
            {
                case AppointmentStatus.Confirmed:
                    sellerPopup = "You have confirmed this appointment.";
                    break;

                case AppointmentStatus.Rescheduled:
                    sellerPopup = "You marked this appointment as rescheduled.";
                    break;

                case AppointmentStatus.Cancelled:

                    sellerPopup = "You have cancelled this appointment.";
                    break;

                case AppointmentStatus.Completed:
                    sellerPopup = "You marked this appointment as completed.";
                    break;

                case AppointmentStatus.Requested:
                    sellerPopup = "Appointment status set to requested.";
                    break;

                default:
                    sellerPopup = "Appointment updated.";
                    break;
            }

            TempData["AppointmentSuccess"] = sellerPopup;

            //  EMAIL TO BUYER
            try
            {
                var buyerEmail = appointment.Buyer?.User?.Email;
                var buyerName = $"{appointment.Buyer?.User?.FirstName} {appointment.Buyer?.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(buyerName)) buyerName = "Buyer";

                var subject = $"Appointment Status Updated: {appointment.Property?.Title}";

                var body =
$@"Hello {buyerName},

Your appointment request has been updated by the seller.

Property: {appointment.Property?.Title}
Date: {appointment.AppointmentDate:yyyy-MM-dd}
Time: {appointment.AppointmentTime}
New Status: {appointment.Status}

Thanks,
Real Estate Property Management System
Developed By Abdullha Al Minhaz";

                if (!string.IsNullOrWhiteSpace(buyerEmail))
                    await _emailService.SendEmailAsync(buyerEmail, subject, body);
            }
            catch
            {
               
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index");
        }
    }
}

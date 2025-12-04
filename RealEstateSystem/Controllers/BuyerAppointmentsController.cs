using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class BuyerAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuyerAppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------
        private bool TrySetBuyerName(out Buyer buyer)
        {
            buyer = null;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            buyer = _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.UserId == userId.Value);

            if (buyer == null) return false;

            var name = $"{buyer.User?.FirstName} {buyer.User?.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "Buyer";

            ViewBag.BuyerName = name;
            return true;
        }

        private bool TrySetBuyerName()
        {
            return TrySetBuyerName(out _);
        }

        // --------------------------------------------------------------------
        // GET: /BuyerAppointments
        // Shows upcoming & past visits
        // --------------------------------------------------------------------
        public IActionResult Index()
        {
            if (!TrySetBuyerName(out var buyer))
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var nowTime = DateTime.Now.TimeOfDay;

            var appointments = _context.Appointments
                .Include(a => a.Property)
                    .ThenInclude(p => p.Images)
                .Include(a => a.Seller)
                    .ThenInclude(s => s.User)
                .Where(a => a.BuyerId == buyer.BuyerId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            var items = appointments.Select(a =>
            {
                var property = a.Property;
                var images = property.Images?
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .ToList() ?? new System.Collections.Generic.List<PropertyImage>();

                var primary = images.FirstOrDefault();
                var primaryUrl = primary?.ImageUrl ?? "/images/property-placeholder.jpg";

                var sellerName = $"{a.Seller?.User?.FirstName} {a.Seller?.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(sellerName))
                    sellerName = "Seller";

                return new BuyerAppointmentItemViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PropertyId = property.PropertyId,
                    PropertyTitle = property.Title,
                    PrimaryImageUrl = primaryUrl,
                    City = property.City,
                    AreaOrLocation = property.AreaOrLocation,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    SellerName = sellerName
                };
            }).ToList();

            var upcoming = items
                .Where(i =>
                    i.AppointmentDate.Date > today ||
                    (i.AppointmentDate.Date == today && i.AppointmentTime >= nowTime))
                .ToList();

            var past = items
                .Except(upcoming)
                .OrderByDescending(i => i.AppointmentDate)
                .ThenByDescending(i => i.AppointmentTime)
                .ToList();

            var model = new BuyerAppointmentListViewModel
            {
                Upcoming = upcoming,
                Past = past
            };

            return View(model);
        }

        // --------------------------------------------------------------------
        // POST: /BuyerAppointments/Book
        // Called from property details page
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(int propertyId, DateTime date, string time, string message)
        {
            if (!TrySetBuyerName(out var buyer))
                return RedirectToAction("Login", "Account");

            var property = _context.Properties
                .Include(p => p.Seller)
                .FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
                return NotFound();

            if (!TimeSpan.TryParse(time, out var timeSpan))
            {
                TempData["AppointmentError"] = "Please select a valid time.";
                return RedirectToAction("Details", "BuyerProperties", new { id = propertyId });
            }

            var dateOnly = date.Date;
            var today = DateTime.Today;

            if (dateOnly < today)
            {
                TempData["AppointmentError"] = "You cannot book a visit in the past.";
                return RedirectToAction("Details", "BuyerProperties", new { id = propertyId });
            }

            // Optional: avoid duplicate active bookings for same property & time
            var hasExisting = _context.Appointments.Any(a =>
                a.PropertyId == propertyId &&
                a.BuyerId == buyer.BuyerId &&
                (a.Status == AppointmentStatus.Requested ||
                 a.Status == AppointmentStatus.Confirmed ||
                 a.Status == AppointmentStatus.Rescheduled) &&
                a.AppointmentDate == dateOnly &&
                a.AppointmentTime == timeSpan);

            if (hasExisting)
            {
                TempData["AppointmentError"] =
                    "You already have a visit booked for this property at that time.";
                return RedirectToAction("Details", "BuyerProperties", new { id = propertyId });
            }

            var appointment = new Appointment
            {
                PropertyId = propertyId,
                BuyerId = buyer.BuyerId,
                SellerId = property.SellerId,
                AppointmentDate = dateOnly,
                AppointmentTime = timeSpan,
                Status = AppointmentStatus.Requested,
                Notes = string.IsNullOrWhiteSpace(message) ? null : message,
                CreatedDate = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            TempData["AppointmentSuccess"] =
                "Your visit request has been sent to the seller.";

            return RedirectToAction("Details", "BuyerProperties", new { id = propertyId });
        }

        // --------------------------------------------------------------------
        // POST: /BuyerAppointments/Cancel/5
        // Allows buyer to cancel their own appointment
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            if (!TrySetBuyerName(out var buyer))
                return RedirectToAction("Login", "Account");

            var appointment = _context.Appointments
                .FirstOrDefault(a => a.AppointmentId == id && a.BuyerId == buyer.BuyerId);

            if (appointment == null)
                return NotFound();

            if (appointment.Status == AppointmentStatus.Completed)
            {
                TempData["AppointmentError"] = "You cannot cancel a completed visit.";
            }
            else
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedDate = DateTime.Now;
                _context.SaveChanges();

                TempData["AppointmentSuccess"] = "Your visit has been cancelled.";
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index");
        }

        // You can keep Details simple for now or expand later
        public IActionResult Details(int id)
        {
            if (!TrySetBuyerName(out var buyer))
                return RedirectToAction("Login", "Account");

            var appointment = _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Seller).ThenInclude(s => s.User)
                .FirstOrDefault(a => a.AppointmentId == id && a.BuyerId == buyer.BuyerId);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }
    }
}

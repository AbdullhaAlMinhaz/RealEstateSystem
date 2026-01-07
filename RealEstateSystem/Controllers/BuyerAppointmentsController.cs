using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using RealEstateSystem.Services.Email;

namespace RealEstateSystem.Controllers
{
    public class BuyerAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BuyerAppointmentsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
        // Buyer books -> Email Seller
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int propertyId, DateTime date, string time, string message)
        {
            if (!TrySetBuyerName(out var buyer))
                return RedirectToAction("Login", "Account");

            var property = await _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

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

            var hasExisting = await _context.Appointments.AnyAsync(a =>
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
            await _context.SaveChangesAsync();

            // ✅ EMAIL TO SELLER
            try
            {
                var sellerEmail = property.Seller?.User?.Email;
                var sellerName = $"{property.Seller?.User?.FirstName} {property.Seller?.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(sellerName)) sellerName = "Seller";

                var buyerName = $"{buyer.User?.FirstName} {buyer.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(buyerName)) buyerName = "Buyer";

                var subject = $"New Appointment Request: {property.Title}";
                var body =
$@"Hello {sellerName},

A buyer has requested an appointment to visit your property.

Property: {property.Title}
Location: {property.City}, {property.AreaOrLocation}
Date: {dateOnly:yyyy-MM-dd}
Time: {timeSpan}

Buyer: {buyerName}
Buyer Email: {buyer.User?.Email}
Buyer Phone: {buyer.User?.PhoneNumber}

Message:
{(string.IsNullOrWhiteSpace(message) ? "N/A" : message)}

Thanks,
Real Estate Property Management System
Developed By Abdullah Al Minhaz";

                if (!string.IsNullOrWhiteSpace(sellerEmail))
                    await _emailService.SendEmailAsync(sellerEmail, subject, body);
            }
            catch
            {
                // Don't break booking if email fails
            }

            TempData["AppointmentSuccess"] =
                "Your visit request has been sent to the seller.";

            return RedirectToAction("Details", "BuyerProperties", new { id = propertyId });
        }

        // --------------------------------------------------------------------
        // POST: /BuyerAppointments/Cancel/5
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

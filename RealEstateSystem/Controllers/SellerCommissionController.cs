using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using RealEstateSystem.Services.Email;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateSystem.Controllers
{
    public class SellerCommissionController : BaseSellerController
    {
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly Services.Email.EmailSettings _emailSettings;

        public SellerCommissionController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IEmailService emailService,
            IOptions<RealEstateSystem.Services.Email.EmailSettings> emailOptions)
            : base(context)
        {
            _env = env;
            _emailService = emailService;
            _emailSettings = emailOptions.Value;
        }

        [HttpGet]
        public IActionResult Pay(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var invoice = _context.CommissionInvoices
                .Where(i => i.CommissionInvoiceId == id && i.SellerId == seller.SellerId)
                .Select(i => new SellerCommissionPaymentViewModel
                {
                    CommissionInvoiceId = i.CommissionInvoiceId,
                    PropertyTitle = i.Property.Title,
                    ListingPrice = i.ListingPrice,
                    CommissionRatePercent = i.CommissionRatePercent,
                    CommissionAmount = i.CommissionAmount,
                    Status = i.Status
                })
                .FirstOrDefault();

            if (invoice == null)
                return NotFound();

            ViewData["PageTitle"] = "Pay Commission";
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(SellerCommissionPaymentViewModel model)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            if (!ModelState.IsValid)
                return View(model);

            var invoice = await _context.CommissionInvoices
                .Include(i => i.Property)
                .Include(i => i.Seller).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(i => i.CommissionInvoiceId == model.CommissionInvoiceId
                                       && i.SellerId == seller.SellerId);

            if (invoice == null)
                return NotFound();

            invoice.CommissionRatePercent = model.CommissionRatePercent;
            invoice.CommissionAmount =
                Math.Round(invoice.ListingPrice * (model.CommissionRatePercent / 100m), 2);

            var folder = Path.Combine(_env.WebRootPath, "uploads", "commission");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(model.ProofImage.FileName);
            var fileName = $"invoice_{invoice.CommissionInvoiceId}_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.ProofImage.CopyTo(stream);
            }

            invoice.PaymentMethod = model.PaymentMethod;
            invoice.TransactionId = model.TransactionId;
            invoice.ProofImageUrl = $"/uploads/commission/{fileName}";
            invoice.SubmittedDate = DateTime.Now;
            invoice.Status = CommissionInvoiceStatus.PendingVerification;

            await _context.SaveChangesAsync();

            // ✅ EMAIL TO ADMIN
            try
            {
                var adminEmail = _emailSettings.AdminNotificationEmail;

                var sellerName = $"{invoice.Seller?.User?.FirstName} {invoice.Seller?.User?.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(sellerName)) sellerName = "Seller";

                var subject = $"Commission Payment Submitted (Invoice #{invoice.CommissionInvoiceId})";
                var body =
$@"Hello Admin,

A seller has submitted a commission payment proof.

Invoice ID: {invoice.CommissionInvoiceId}
Property: {invoice.Property?.Title}
Seller: {sellerName}
Seller Email: {invoice.Seller?.User?.Email}

Payment Method: {invoice.PaymentMethod}
Transaction ID: {invoice.TransactionId}
Commission Amount: {invoice.CommissionAmount}

Status: {invoice.Status}

Thanks,
Real Estate Property Management System
Developed By Abdullah Al Minhaz";

                if (!string.IsNullOrWhiteSpace(adminEmail))
                    await _emailService.SendEmailAsync(adminEmail, subject, body);
            }
            catch
            {
                // Don't break payment submit if email fails
            }

            TempData["Success"] = "Payment submitted. Awaiting admin verification.";
            return RedirectToAction("Index", "SellerProperties");
        }

        [HttpGet]
        public IActionResult Receipt(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var invoice = _context.CommissionInvoices
                .Include(i => i.Property)
                .Include(i => i.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefault(i => i.CommissionInvoiceId == id && i.SellerId == seller.SellerId);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != CommissionInvoiceStatus.Paid)
            {
                TempData["Error"] = "Receipt is available only after payment is approved.";
                return RedirectToAction("Index", "SellerProperties");
            }

            return View(invoice);
        }
    }
}

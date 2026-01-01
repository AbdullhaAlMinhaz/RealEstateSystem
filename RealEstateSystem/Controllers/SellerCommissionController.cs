using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using System;
using System.IO;
using System.Linq;

namespace RealEstateSystem.Controllers
{
    public class SellerCommissionController : BaseSellerController
    {
        private readonly IWebHostEnvironment _env;

        public SellerCommissionController(ApplicationDbContext context, IWebHostEnvironment env)
            : base(context)
        {
            _env = env;
        }

        // ================= VIEW INVOICE =================
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

        // ================= SUBMIT PAYMENT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pay(SellerCommissionPaymentViewModel model)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            if (!ModelState.IsValid)
                return View(model);

            var invoice = _context.CommissionInvoices
                .FirstOrDefault(i => i.CommissionInvoiceId == model.CommissionInvoiceId
                                  && i.SellerId == seller.SellerId);

            if (invoice == null)
                return NotFound();

            // Recalculate commission (security)
            invoice.CommissionRatePercent = model.CommissionRatePercent;
            invoice.CommissionAmount =
                Math.Round(invoice.ListingPrice * (model.CommissionRatePercent / 100m), 2);

            // Save proof image
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

            _context.SaveChanges();

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

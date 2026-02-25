using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;

namespace RealEstateSystem.Controllers
{
    public class AdminCommissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCommissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // invoice List

        public IActionResult Index()
        {
            var invoices = _context.CommissionInvoices
                .Include(i => i.Property)
                .Include(i => i.Seller)
                    .ThenInclude(s => s.User)
                .OrderByDescending(i => i.CreatedDate)
                .ToList();

            ViewData["PageTitle"] = "Commission Management";
            return View(invoices);
        }

        // Details View

        public IActionResult Details(int id)
        {
            var invoice = _context.CommissionInvoices
                .Include(i => i.Property)
                .Include(i => i.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefault(i => i.CommissionInvoiceId == id);

            if (invoice == null)
                return NotFound();

            ViewData["PageTitle"] = "Commission Details";
            return View(invoice);
        }

        // APPROVE 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, string adminNote)
        {
            var invoice = _context.CommissionInvoices
                .FirstOrDefault(i => i.CommissionInvoiceId == id);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != CommissionInvoiceStatus.PendingVerification)
            {
                TempData["Error"] = "Only Pending Verification invoices can be approved.";
                return RedirectToAction(nameof(Index));
            }

            invoice.Status = CommissionInvoiceStatus.Paid;
            invoice.AdminNote = adminNote;
            invoice.VerifiedDate = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Commission payment approved.";
            return RedirectToAction(nameof(Index));
        }

        //  REJECT 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string adminNote)
        {
            var invoice = _context.CommissionInvoices
                .FirstOrDefault(i => i.CommissionInvoiceId == id);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != CommissionInvoiceStatus.PendingVerification)
            {
                TempData["Error"] = "Only Pending Verification invoices can be rejected.";
                return RedirectToAction(nameof(Index));
            }

            invoice.Status = CommissionInvoiceStatus.Rejected;
            invoice.AdminNote = adminNote;
            invoice.VerifiedDate = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Commission payment rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}

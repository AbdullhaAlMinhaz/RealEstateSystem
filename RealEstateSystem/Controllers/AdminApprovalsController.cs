using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminApprovalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminApprovalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Shows all pending properties for approval
        [HttpGet]
        public IActionResult Pending()
        {
            ViewData["PageTitle"] = "Approvals";
            ViewData["PageSubtitle"] = "Pending property submissions from sellers.";

            var pending = _context.Properties
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .Where(p => p.ApprovalStatus == PropertyApprovalStatus.Pending)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new PendingPropertyApprovalViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    SellerName = p.Seller != null && p.Seller.User != null
                        ? p.Seller.User.FirstName + " " + p.Seller.User.LastName
                        : "Unknown seller",
                    SubmittedAt = p.CreatedDate,
                    ApprovalStatus = p.ApprovalStatus
                })
                .ToList();

            return View(pending);
        }
    }
}

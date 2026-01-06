using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RealEstateSystem.Controllers
{
    public class SellerPropertiesController : BaseSellerController
    {
        private readonly IWebHostEnvironment _environment;

        public SellerPropertiesController(ApplicationDbContext context, IWebHostEnvironment environment)
            : base(context)
        {
            _environment = environment;
        }

        // ========== MY PROPERTIES ==========
        public IActionResult Index()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            //var properties = _context.Properties
            //    .Where(p => p.SellerId == seller.SellerId)
            //    .OrderByDescending(p => p.CreatedDate)
            //    .ToList();

            var properties = _context.Properties
                .Include(p => p.CommissionInvoice)
                .Where(p => p.SellerId == seller.SellerId)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();


            var model = new SellerPropertyListViewModel
            {
                Title = "My Properties",
                Subtitle = "Manage your listed properties.",
                Properties = properties
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(model);
        }

        // ========== MARK PROPERTY AS SOLD + AUTO CREATE COMMISSION INVOICE ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsSold(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id && p.SellerId == seller.SellerId);

            if (property == null)
                return NotFound();

            // Only allow Available -> Sold
            if (property.Status != PropertyStatus.Available)
            {
                TempData["Error"] = "Only Available properties can be marked as Sold.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Use transaction so Sold + Invoice happen together safely
            using var tx = _context.Database.BeginTransaction();

            try
            {
                // 1) Mark Sold
                property.Status = PropertyStatus.Sold;
                property.UpdatedDate = DateTime.Now;

                // 2) Create invoice only if not exists (prevent duplicates)
                bool invoiceExists = _context.CommissionInvoices.Any(ci => ci.PropertyId == property.PropertyId);
                if (!invoiceExists)
                {
                    //int ratePercent = 2; // Default 2% (dropdown later in Step 3)

                    int ratePercent = property.CommissionRatePercent;
                    if (ratePercent < 2 || ratePercent > 5)
                    {
                        ratePercent = 2; // fallback safety
                    }


                    var invoice = new CommissionInvoice
                    {
                        PropertyId = property.PropertyId,
                        SellerId = seller.SellerId,
                        ListingPrice = property.Price,
                        CommissionRatePercent = ratePercent,
                        CommissionAmount = Math.Round(property.Price * (ratePercent / 100m), 2),
                        Status = CommissionInvoiceStatus.Unpaid,
                        CreatedDate = DateTime.Now,

                        AdminNote = "",
                        TransactionId = "",
                        ProofImageUrl = ""
                    };


                    _context.CommissionInvoices.Add(invoice);
                }

                _context.SaveChanges();
                tx.Commit();

                TempData["Success"] = $"'{property.Title}' marked as Sold and commission invoice created.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                tx.Rollback();
                TempData["Error"] = "Failed to mark sold: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }


        // ========== PENDING PROPERTIES (OPTIONAL TAB) ==========
        public IActionResult Pending()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;



            var properties = _context.Properties
                .Include(p => p.CommissionInvoice)
                .Where(p => p.SellerId == seller.SellerId &&
                            p.ApprovalStatus == PropertyApprovalStatus.Pending)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();


            var model = new SellerPropertyListViewModel
            {
                Title = "Pending Approval",
                Subtitle = "Listings waiting for admin review.",
                Properties = properties
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View("Index", model);
        }

        // ========== ADD NEW PROPERTY (GET) ==========
        [HttpGet]
        public IActionResult Create()
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var model = new SellerPropertyCreateViewModel();

            ViewData["PageTitle"] = "Add New Property";
            ViewData["PageSubtitle"] = "Create a new listing and submit for admin approval.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(model);
        }

        // ========== ADD NEW PROPERTY (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SellerPropertyCreateViewModel model)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            ViewData["PageTitle"] = "Add New Property";
            ViewData["PageSubtitle"] = "Create a new listing and submit for admin approval.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var property = new Property
                {
                    SellerId = seller.SellerId,
                    Title = model.Title,
                    Description = model.Description,
                    PropertyType = model.PropertyType,
                    Price = model.Price,
                    Address = model.Address,
                    City = model.City,
                    AreaOrLocation = model.AreaOrLocation,
                    State = model.State,
                    ZipCode = model.ZipCode,
                    Bedrooms = model.Bedrooms,
                    Bathrooms = model.Bathrooms,
                    AreaSqft = model.AreaSqft,
                    Status = PropertyStatus.Available,
                    ApprovalStatus = PropertyApprovalStatus.Pending,
                    CreatedDate = DateTime.Now,
                    SubmittedDate = DateTime.Now,
                    ViewCount = 0,
                    IsFeatured = false
                };

                _context.Properties.Add(property);
                _context.SaveChanges();

                SaveUploadedImages(property, model.Photos);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty, "Error saving property: " + msg);
                return View(model);
            }
        }

        // ========== EDIT PROPERTY (GET) ==========
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id && p.SellerId == seller.SellerId);

            if (property == null)
                return NotFound();

            // ✅ LOCK AFTER SOLD
            if (property.Status == PropertyStatus.Sold)
            {
                TempData["Error"] = "This property is Sold, so it can no longer be edited.";
                return RedirectToAction(nameof(Index));
            }

            var model = new SellerPropertyEditViewModel
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                PropertyType = property.PropertyType,
                Price = property.Price,
                AreaOrLocation = property.AreaOrLocation,
                State = property.State,
                Description = property.Description,
                Address = property.Address,
                City = property.City,
                ZipCode = property.ZipCode,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                AreaSqft = property.AreaSqft
            };

            ViewData["PageTitle"] = "Edit Property";
            ViewData["PageSubtitle"] = "Update details of your listing.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(model);
        }

        // ========== EDIT PROPERTY (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SellerPropertyEditViewModel model)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            ViewData["PageTitle"] = "Edit Property";
            ViewData["PageSubtitle"] = "Update details of your listing.";
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var property = _context.Properties
                    .FirstOrDefault(p => p.PropertyId == model.PropertyId &&
                                         p.SellerId == seller.SellerId);

                if (property == null)
                    return NotFound();

                // ✅ LOCK AFTER SOLD
                if (property.Status == PropertyStatus.Sold)
                {
                    TempData["Error"] = "This property is Sold, so it can no longer be edited.";
                    return RedirectToAction(nameof(Index));
                }

                property.Title = model.Title;
                property.PropertyType = model.PropertyType;
                property.Price = model.Price;
                property.AreaOrLocation = model.AreaOrLocation;
                property.State = model.State;
                property.Description = model.Description;
                property.Address = model.Address;
                property.City = model.City;
                property.ZipCode = model.ZipCode;
                property.Bedrooms = model.Bedrooms;
                property.Bathrooms = model.Bathrooms;
                property.AreaSqft = model.AreaSqft;
                property.UpdatedDate = DateTime.Now;

                _context.SaveChanges();

                SaveUploadedImages(property, model.Photos);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty, "Error updating property: " + msg);
                return View(model);
            }
        }

        // ========== DETAILS (seller view) ==========
        [HttpGet]
        public IActionResult Details(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            var property = _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefault(p => p.PropertyId == id &&
                                     p.SellerId == seller.SellerId);

            if (property == null)
                return NotFound();

            var user = property.Seller?.User;

            var model = new PropertyDetailsViewModel
            {
                Property = property,
                Images = property.Images ?? new List<PropertyImage>(),
                SellerName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                SellerEmail = user?.Email,
                SellerPhone = user?.PhoneNumber
            };

            ViewData["PageTitle"] = "Property Details";
            ViewData["PageSubtitle"] = property.Title;
            ViewData["SellerDisplayName"] = $"{seller.User.FirstName} {seller.User.LastName}";

            return View(model);
        }

        // ========== DELETE PROPERTY ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (RedirectToLoginIfNotSeller(out var seller) is IActionResult redirect)
                return redirect;

            // ✅ Only delete seller's own property
            var property = _context.Properties
                .Include(p => p.Images)
                .FirstOrDefault(p => p.PropertyId == id && p.SellerId == seller.SellerId);

            if (property == null)
                return NotFound();

            try
            {
                // ✅ 1) Remove Wishlist references FIRST (Fix FK_Wishlists_Properties_PropertyId)
                // NOTE: Your DbSet name must be Wishlists in ApplicationDbContext
                var wishItems = _context.Wishlists.Where(w => w.PropertyId == id).ToList();
                if (wishItems.Any())
                    _context.Wishlists.RemoveRange(wishItems);

                // ✅ 2) Delete images in DB
                if (property.Images != null && property.Images.Any())
                {
                    _context.PropertyImages.RemoveRange(property.Images);
                }

                // ✅ 3) Delete images folder from wwwroot/uploads/properties/{id}
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "properties", property.PropertyId.ToString());
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }

                // ✅ 4) Delete property
                _context.Properties.Remove(property);

                // ✅ 5) Save once (all deletes in one transaction)
                _context.SaveChanges();

                TempData["Success"] = "Property deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Delete failed: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== HELPER: SAVE UPLOADED IMAGES ==========
        private void SaveUploadedImages(Property property, List<IFormFile> photos)
        {
            if (photos == null || photos.Count == 0)
                return;

            var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "properties", property.PropertyId.ToString());
            Directory.CreateDirectory(uploadRoot);

            int nextOrder = _context.PropertyImages
                .Where(i => i.PropertyId == property.PropertyId)
                .Select(i => (int?)i.DisplayOrder)
                .Max() ?? 0;

            string[] allowedExt = { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var file in photos.Where(f => f != null && f.Length > 0))
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExt.Contains(ext))
                    continue;

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var image = new PropertyImage
                {
                    PropertyId = property.PropertyId,
                    ImageUrl = $"/uploads/properties/{property.PropertyId}/{fileName}",
                    IsPrimary = nextOrder == 0 && !_context.PropertyImages.Any(i => i.PropertyId == property.PropertyId),
                    DisplayOrder = nextOrder++
                };

                _context.PropertyImages.Add(image);
            }

            _context.SaveChanges();
        }
    }
}

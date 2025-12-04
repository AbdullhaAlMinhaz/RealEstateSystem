using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class BuyerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BuyerProfileController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /BuyerProfile
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            var vm = new BuyerProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive,
                RegisteredOn = user.CreatedDate,
                ProfilePhoto = string.IsNullOrWhiteSpace(user.ProfilePhoto)
                    ? "/images/default-avatar.png"
                    : user.ProfilePhoto
            };

            return View(vm);
        }

        // POST: /BuyerProfile/UploadPhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadPhoto(IFormFile profilePhoto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                TempData["ProfileError"] = "Please select a picture to upload.";
                return RedirectToAction("Index");
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(profilePhoto.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["ProfileError"] = "Only image files (jpg, png, gif, webp) are allowed.";
                return RedirectToAction("Index");
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profile");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"user_{userId.Value}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                profilePhoto.CopyTo(stream);
            }

            var relativePath = $"/uploads/profile/{fileName}";

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                TempData["ProfileError"] = "User not found.";
                return RedirectToAction("Index");
            }

            user.ProfilePhoto = relativePath;
            user.UpdatedDate = DateTime.Now;
            _context.SaveChanges();

            TempData["ProfileSuccess"] = "Profile photo updated successfully.";
            return RedirectToAction("Index");
        }

        // POST: /BuyerProfile/UpdateProfile  (Profile Information form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(BuyerProfileViewModel model)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.UserId != sessionUserId.Value)
            {
                return Forbid();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == sessionUserId.Value);
            if (user == null)
            {
                return NotFound();
            }

            // update editable fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            TempData["ProfileSuccess"] = "Profile information updated successfully.";
            return RedirectToAction("Index");
        }
    }
}

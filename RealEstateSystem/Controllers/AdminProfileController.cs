using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IWebHostEnvironment _env;

        public AdminProfileController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IWebHostEnvironment env)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _env = env;
        }

        // AdminProfile 
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.Role == UserRole.Admin);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var model = new AdminProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                //PresentAddress = user.PresentAddress,
                IsActive = user.IsActive,
                RegisteredOn = user.CreatedDate == default ? DateTime.Now : user.CreatedDate,
                ProfilePhoto = user.ProfilePhoto
            };

            return View(model);
        }

        //  UPDATE BASIC PROFILE 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(AdminProfileViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId && u.Role == UserRole.Admin);
            if (user == null)
                return NotFound();

            // bind editable fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.UpdatedDate = DateTime.Now;

            // handle image upload
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(model.ProfileImageFile.FileName);
                var fileName = $"admin_{user.UserId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfileImageFile.CopyTo(stream);
                }

                // store relative path in DB
                user.ProfilePhoto = $"/uploads/profiles/{fileName}";
            }

            _context.SaveChanges();   

            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

            TempData["ProfileUpdated"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }

        //  UPDATE PASSWORD 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePassword(AdminProfileViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId && u.Role == UserRole.Admin);
            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
            {
                ModelState.AddModelError(string.Empty, "Please fill in all password fields.");
                return View("Index", ReloadModel(user));
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirmation do not match.");
                return View("Index", ReloadModel(user));
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                return View("Index", ReloadModel(user));
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            user.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            TempData["PasswordUpdated"] = "Password updated successfully.";
            return RedirectToAction("Index");
        }

        private AdminProfileViewModel ReloadModel(User user)
        {
            return new AdminProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                //PresentAddress = user.PresentAddress,
                IsActive = user.IsActive,
                RegisteredOn = user.CreatedDate == default ? DateTime.Now : user.CreatedDate,
                ProfilePhoto = user.ProfilePhoto
            };
        }
    }
}

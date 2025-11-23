using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // ========== REGISTER (GET) ==========
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ========== REGISTER (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check duplicate email
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            // Create User
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Role = model.Role,
                IsVerified = false,
                IsActive = true,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            // Create Buyer or Seller profile based on Role
            if (model.Role == UserRole.Buyer)
            {
                var buyer = new Buyer
                {
                    UserId = user.UserId,
                    SavedSearchPreferences = null
                };
                _context.Buyers.Add(buyer);
            }
            else if (model.Role == UserRole.Seller)
            {
                var seller = new Seller
                {
                    UserId = user.UserId,
                    SellerType = SellerType.Owner,          // adjust default if you want
                    RegistrationStatus = RegistrationStatus.Pending,
                    JoinedDate = DateTime.Now
                };
                _context.Sellers.Add(seller);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Registration successful. Please login.";

            // tell Home/Index.cshtml to open the login modal
            TempData["OpenLoginModal"] = true;

            // go to your designed Home page (which contains the login modal UI)
            return RedirectToAction("Index", "Home");

        }

        // ========== LOGIN (GET) ==========
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ========== LOGIN (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Save session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", ((int)user.Role).ToString());
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

            // Redirect by role
            switch (user.Role)
            {
                case UserRole.Admin:
                    return RedirectToAction("Index", "AdminDashboard");
                case UserRole.Seller:
                    return RedirectToAction("Index", "SellerDashboard");
                case UserRole.Buyer:
                    return RedirectToAction("Index", "BuyerDashboard");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        // ========== LOGOUT ==========
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}

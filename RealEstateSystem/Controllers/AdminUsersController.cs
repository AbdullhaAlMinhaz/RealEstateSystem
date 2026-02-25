using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        // All User
        public IActionResult Index()
        {
            var model = new AdminUserListViewModel
            {
                Title = "All Users",
                Subtitle = "All registered users in the system.",
                Users = _context.Users
                                .OrderBy(u => u.UserId)
                                .ToList()
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("UserList", model);
        }

        // Buyers
        public IActionResult Buyers()
        {
            var model = new AdminUserListViewModel
            {
                Title = "Buyers",
                Subtitle = "Registered buyers and their accounts.",
                Users = _context.Users
                                .Where(u => u.Role == UserRole.Buyer)
                                .OrderBy(u => u.UserId)
                                .ToList()
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("UserList", model);
        }

        // Sellers
        public IActionResult Sellers()
        {
            var model = new AdminUserListViewModel
            {
                Title = "Sellers",
                Subtitle = "Registered sellers and their accounts.",
                Users = _context.Users
                                .Where(u => u.Role == UserRole.Seller)
                                .OrderBy(u => u.UserId)
                                .ToList()
            };

            ViewData["PageTitle"] = model.Title;
            ViewData["PageSubtitle"] = model.Subtitle;

            return View("UserList", model);
        }

        //Edit Users

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            var vm = new AdminUserEditViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsVerified = user.IsVerified,
                IsActive = user.IsActive
            };

            ViewData["PageTitle"] = "Edit User";
            ViewData["PageSubtitle"] = $"Update details for {user.FirstName} {user.LastName}.";

            return View(vm);
        }

        // User Edit post

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AdminUserEditViewModel model)
        {
            if (id != model.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Role = model.Role;
            user.IsVerified = model.IsVerified;
            user.IsActive = model.IsActive;
            user.UpdatedDate = DateTime.Now;

            _context.SaveChanges();
            TempData["SuccessMessage"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // prevent admin from deleting themselves
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId.HasValue && currentUserId.Value == user.UserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own admin account.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Cannot delete user because there is related data in the system.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

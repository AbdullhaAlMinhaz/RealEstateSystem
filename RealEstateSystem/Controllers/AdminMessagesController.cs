using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RealEstateSystem.Models;

namespace RealEstateSystem.Controllers
{
    public class AdminMessagesController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roleStr = HttpContext.Session.GetString("UserRole");

            if (userId == null) return RedirectToAction("Login", "Account");

            // UserRole is stored as int string: "1" for Admin
            if (string.IsNullOrWhiteSpace(roleStr) ||
                roleStr != ((int)UserRole.Admin).ToString())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}

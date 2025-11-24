using Microsoft.AspNetCore.Mvc;

namespace RealEstateSystem.Controllers
{
    public class AdminSettingsController : Controller
    {
        // application-level settings page
        public IActionResult Index()
        {
            return View();
        }
    }
}

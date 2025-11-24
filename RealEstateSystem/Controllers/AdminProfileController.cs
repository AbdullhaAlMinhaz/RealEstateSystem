using Microsoft.AspNetCore.Mvc;

namespace RealEstateSystem.Controllers
{
    public class AdminProfileController : Controller
    {
        // later we can load the real admin data from DB / session
        public IActionResult Index()
        {
            return View();
        }
    }
}

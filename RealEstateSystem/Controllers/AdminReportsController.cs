using Microsoft.AspNetCore.Mvc;

namespace RealEstateSystem.Controllers
{
    public class AdminReportsController : Controller
    {
        // GET: /AdminReports
        public IActionResult Index()
        {
            return View();
        }
    }
}

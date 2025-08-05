using Microsoft.AspNetCore.Mvc;

namespace TTtasarim.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Herkesin erişebileceği ana sayfa
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace TTtasarim.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Token = token;
            return View();
        }
    }
}

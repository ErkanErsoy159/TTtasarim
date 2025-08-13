using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TTtasarim.Web.Models;

namespace TTtasarim.Web.Controllers
{
    public class HataController : Controller
    {
        public IActionResult Index()
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Yetkisiz()
        {
            return View();
        }
    }
}

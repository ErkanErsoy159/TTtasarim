using Microsoft.AspNetCore.Mvc;

namespace TTtasarim.Web.Controllers
{
    public class HataController : Controller
    {
        public IActionResult Yetkisiz()
        {
            return View();
        }
    }
}

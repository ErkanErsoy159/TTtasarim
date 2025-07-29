using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TTtasarim.Web.Models;

namespace TTtasarim.Web.Controllers
{
    public class KullanicilarController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public KullanicilarController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return RedirectToAction("Yetkisiz", "Hata");

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _configuration["Api:BaseUrl"] + "/api/users";
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Hata = "Kullanıcı verileri alınamadı.";
                return View(new List<KullaniciViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var kullanicilar = JsonConvert.DeserializeObject<List<KullaniciViewModel>>(json);

            return View(kullanicilar);
        }
    }
}

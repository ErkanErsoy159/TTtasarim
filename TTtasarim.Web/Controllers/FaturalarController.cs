using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TTtasarim.Web.Models;

namespace TTtasarim.Web.Controllers
{
    public class FaturalarController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public FaturalarController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _configuration["Api:BaseUrl"] + "/api/invoices";
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Hata = "Fatura verileri alınamadı.";
                return View(new List<FaturaViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var faturalar = JsonConvert.DeserializeObject<List<FaturaViewModel>>(json);

            return View(faturalar);
        }

        [HttpPost]
        public async Task<IActionResult> OdemeYap(string id)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _configuration["Api:BaseUrl"] + $"/api/invoices/odeme/{id}";
            var response = await client.PostAsync(apiUrl, null);

            if (!response.IsSuccessStatusCode)
            {
                TempData["OdemeHata"] = "Ödeme işlemi başarısız oldu.";
            }

            return RedirectToAction("Index");
        }
    }
}

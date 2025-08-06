using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TTtasarim.Web.Models;

namespace TTtasarim.Web.Controllers
{
    public class RaporlarController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public RaporlarController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        // Raporlar ana sayfası
        public IActionResult Index()
        {
            // Giriş yapmış kullanıcı kontrolü
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            // Kullanıcı rolünü ViewBag'e aktar
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = role ?? "normal";

            return View();
        }

        // Fatura ödeme raporları (ödeme geçmişinin aynısı)
        public async Task<IActionResult> FaturaOdemeRaporlari()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Kullanıcı rolüne göre API endpoint'i seç
                var role = HttpContext.Session.GetString("UserRole");
                var apiUrl = role == "admin" 
                    ? _configuration["Api:BaseUrl"] + "/api/invoices/admin-history"
                    : _configuration["Api:BaseUrl"] + "/api/invoices/history";

                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<List<dynamic>>>(json);
                    
                    ViewBag.PaymentHistory = result?.Data ?? new List<dynamic>();
                    ViewBag.Message = result?.Message;
                    ViewBag.UserRole = role ?? "normal";
                }
                else
                {
                    ViewBag.PaymentHistory = new List<dynamic>();
                    ViewBag.Hata = "Fatura ödeme raporları alınamadı";
                    ViewBag.UserRole = role ?? "normal";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.PaymentHistory = new List<dynamic>();
                ViewBag.Hata = "Bir hata oluştu: " + ex.Message;
                ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "normal";
                return View();
            }
        }

        // Kredi takip raporları
        public async Task<IActionResult> KrediTakipRaporlari()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return RedirectToAction("Login", "Auth");

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/credits";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<CreditDto>>(json);
                    
                    ViewBag.Credits = result ?? new List<CreditDto>();
                }
                else
                {
                    ViewBag.Credits = new List<CreditDto>();
                    ViewBag.Hata = "Kredi takip raporları alınamadı";
                }

                // Credit logs için de API çağrısı yapacağız
                var logsApiUrl = _configuration["Api:BaseUrl"] + "/api/creditlogs";
                var logsResponse = await client.GetAsync(logsApiUrl);

                if (logsResponse.IsSuccessStatusCode)
                {
                    var logsJson = await logsResponse.Content.ReadAsStringAsync();
                    var logsResult = JsonConvert.DeserializeObject<List<dynamic>>(logsJson);
                    
                    ViewBag.CreditLogs = logsResult ?? new List<dynamic>();
                }
                else
                {
                    ViewBag.CreditLogs = new List<dynamic>();
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Credits = new List<CreditDto>();
                ViewBag.CreditLogs = new List<CreditLogDto>();
                ViewBag.Hata = "Bir hata oluştu: " + ex.Message;
                return View();
            }
        }
    }

    // DTO Models
    public class CreditDto
    {
        public string Id { get; set; } = string.Empty;
        public string DealerId { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string CreditId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
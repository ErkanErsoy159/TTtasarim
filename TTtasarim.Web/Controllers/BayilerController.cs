using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TTtasarim.Web.Models;

namespace TTtasarim.Web.Controllers
{
    public class BayilerController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public BayilerController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        // Bayi listesi
        public async Task<IActionResult> Index()
        {
            //Yetki kontrolü: sadece admin görebilir
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return RedirectToAction("Yetkisiz", "Hata");

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/dealers";
                var response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Hata = "Bayi verileri alınamadı.";
                    return View(new List<BayiViewModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var bayiler = JsonConvert.DeserializeObject<List<BayiViewModel>>(json);

                return View(bayiler ?? new List<BayiViewModel>());
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Bir hata oluştu: " + ex.Message;
                return View(new List<BayiViewModel>());
            }
        }

        // Yeni bayi ekleme (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddDealer([FromBody] BayiViewModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return Json(new { success = false, message = "Yetkisiz erişim" });

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Oturum süresi dolmuş" });

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var dealerData = new
                {
                    Code = model.Code,
                    Name = model.Name,
                    Status = model.Status ?? "aktif"
                };

                var json = JsonConvert.SerializeObject(dealerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/dealers";
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Bayi başarıyla eklendi" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Bayi eklenirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Bayi güncelleme (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateDealer([FromBody] BayiViewModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return Json(new { success = false, message = "Yetkisiz erişim" });

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Oturum süresi dolmuş" });

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var dealerData = new
                {
                    Code = model.Code,
                    Name = model.Name,
                    Status = model.Status
                };

                var json = JsonConvert.SerializeObject(dealerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/dealers/{model.Id}";
                var response = await client.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Bayi başarıyla güncellendi" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Bayi güncellenirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Kredi güncelleme (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateCredit([FromBody] UpdateCreditModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return Json(new { success = false, message = "Yetkisiz erişim" });

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Oturum süresi dolmuş" });

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var creditData = new { Amount = model.Amount };
                var json = JsonConvert.SerializeObject(creditData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/dealers/{model.DealerId}/credit";
                var response = await client.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Kredi başarıyla güncellendi", data = responseContent });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Kredi güncellenirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Bayi silme (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteDealer([FromBody] DeleteModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin")
                return Json(new { success = false, message = "Yetkisiz erişim" });

            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Oturum süresi dolmuş" });

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/dealers/{model.Id}";
                var response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Bayi başarıyla silindi" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Bayi silinirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
    }

    // Helper models
    public class UpdateCreditModel
    {
        public string DealerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DeleteModel
    {
        public string Id { get; set; } = string.Empty;
    }
}

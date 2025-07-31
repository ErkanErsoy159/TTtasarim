using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
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

        // Kullanıcı listesi
        public async Task<IActionResult> Index()
        {
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

                // Kullanıcıları al
                var usersApiUrl = _configuration["Api:BaseUrl"] + "/api/users";
                var usersResponse = await client.GetAsync(usersApiUrl);

                if (!usersResponse.IsSuccessStatusCode)
                {
                    ViewBag.Hata = "Kullanıcı verileri alınamadı.";
                    return View(new List<KullaniciViewModel>());
                }

                var usersJson = await usersResponse.Content.ReadAsStringAsync();
                var kullanicilar = JsonConvert.DeserializeObject<List<KullaniciViewModel>>(usersJson);

                // Bayileri al (dropdown için)
                var dealersApiUrl = _configuration["Api:BaseUrl"] + "/api/dealers";
                var dealersResponse = await client.GetAsync(dealersApiUrl);
                
                if (dealersResponse.IsSuccessStatusCode)
                {
                    var dealersJson = await dealersResponse.Content.ReadAsStringAsync();
                    var bayiler = JsonConvert.DeserializeObject<List<BayiViewModel>>(dealersJson);
                    ViewBag.Bayiler = bayiler ?? new List<BayiViewModel>();
                }
                else
                {
                    ViewBag.Bayiler = new List<BayiViewModel>();
                }

                return View(kullanicilar ?? new List<KullaniciViewModel>());
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Bir hata oluştu: " + ex.Message;
                return View(new List<KullaniciViewModel>());
            }
        }

        // Yeni kullanıcı ekleme (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] KullaniciViewModel model)
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

                var userData = new
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    GSM = model.GSM,
                    UserType = model.UserType ?? "normal"
                };

                var json = JsonConvert.SerializeObject(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/users";
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var createdUser = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    // Bayi ataması varsa yap
                    if (!string.IsNullOrEmpty(model.AssignedDealerId))
                    {
                        await AssignUserToDealerInternal(createdUser.id.ToString(), model.AssignedDealerId, client);
                    }

                    return Json(new { success = true, message = "Kullanıcı başarıyla eklendi" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Kullanıcı eklenirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Kullanıcı güncelleme (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] KullaniciViewModel model)
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

                var userData = new
                {
                    Username = model.Username,
                    Email = model.Email,
                    GSM = model.GSM,
                    UserType = model.UserType
                };

                var json = JsonConvert.SerializeObject(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/users/{model.Id}";
                var response = await client.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Kullanıcı başarıyla güncellendi" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Kullanıcı güncellenirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Bayi atama (AJAX)
        [HttpPost]
        public async Task<IActionResult> AssignDealer([FromBody] AssignDealerModel model)
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

                var result = await AssignUserToDealerInternal(model.UserId, model.DealerId, client);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Bayi ataması kaldırma (AJAX)
        [HttpPost]
        public async Task<IActionResult> RemoveDealer([FromBody] RemoveDealerModel model)
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

                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/users/{model.UserId}/remove-dealer";
                var response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Bayi ataması başarıyla kaldırıldı" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Bayi ataması kaldırılırken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Kullanıcı silme (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteModel model)
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

                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/users/{model.Id}";
                var response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Kullanıcı başarıyla silindi" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Kullanıcı silinirken hata: " + error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Helper method for dealer assignment
        private async Task<object> AssignUserToDealerInternal(string userId, string dealerId, HttpClient client)
        {
            var assignData = new { DealerId = dealerId };
            var json = JsonConvert.SerializeObject(assignData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiUrl = _configuration["Api:BaseUrl"] + $"/api/users/{userId}/assign-dealer";
            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return new { success = true, message = "Kullanıcı başarıyla bayiye atandı" };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new { success = false, message = "Bayi ataması yapılırken hata: " + error };
            }
        }
    }

    // Helper models
    public class AssignDealerModel
    {
        public string UserId { get; set; } = string.Empty;
        public string DealerId { get; set; } = string.Empty;
    }

    public class RemoveDealerModel
    {
        public string UserId { get; set; } = string.Empty;
    }
}

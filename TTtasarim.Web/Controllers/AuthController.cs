using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TTtasarim.Web.Models;

namespace TTtasarim.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _clientFactory.CreateClient();

            var json = JsonConvert.SerializeObject(new
            {
                Email = model.Email,
                Password = model.Password
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiUrl = _configuration["Api:BaseUrl"] + "/api/auth/login";
            var response = await client.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                model.ErrorMessage = "Geçersiz e-posta veya şifre";
                return View(model);
            }

            // Gelen veriyi çözümle
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseContent);

            string token = result.token;
            string role = result.userType;

            // Session'a kaydet
            HttpContext.Session.SetString("JWT", token);
            HttpContext.Session.SetString("UserRole", role); // ROL buraya yazıldı

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
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

        // Fatura sorgulama ana sayfası
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            // Şirketleri API'den al
            var companies = await GetCompaniesAsync();
            ViewBag.Companies = companies;
            ViewBag.ApiBaseUrl = _configuration["Api:BaseUrl"];

            return View();
        }

        // AJAX ile fatura sorgulama
        [HttpPost]
        public async Task<IActionResult> QueryInvoices(string companyId, string accessNo)
        {
            try
            {
                if (string.IsNullOrEmpty(companyId) || string.IsNullOrEmpty(accessNo))
                {
                    return Json(new { success = false, message = "Şirket ve abone numarası gerekli" });
                }

                var client = _clientFactory.CreateClient();
                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/invoices/query?companyId={companyId}&accessNo={accessNo}";
                var response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Fatura sorgulaması başarısız: {errorContent}" });
                }

                var json = await response.Content.ReadAsStringAsync();
                
                // Debug logging
                Console.WriteLine($"API Response JSON: {json}");
                
                // JSON deserialize işleminde decimal formatting'i koruyalım
                var settings = new JsonSerializerSettings
                {
                    FloatParseHandling = FloatParseHandling.Decimal,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                };
                
                var invoices = JsonConvert.DeserializeObject<List<dynamic>>(json, settings);
                
                // Debug: Her fatura için amount değerini kontrol et
                if (invoices != null)
                {
                    foreach (var invoice in invoices)
                    {
                        Console.WriteLine($"Web Controller - Fatura: {invoice.description}, Amount: {invoice.amount} (Type: {invoice.amount?.GetType()})");
                    }
                }

                return Json(new { success = true, data = invoices });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QueryInvoices Error: {ex.Message}");
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // Fatura ödeme
        [HttpPost]
        public async Task<IActionResult> PayInvoice(string invoiceId)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "Oturum süresi dolmuş" });

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var paymentRequest = new { InvoiceId = invoiceId };
                var json = JsonConvert.SerializeObject(paymentRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/invoices/pay";
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    
                    return Json(new { 
                        success = true, 
                        message = "Fatura başarıyla ödendi!", 
                        remainingCredit = result.remainingCredit 
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = errorContent });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ödeme hatası: " + ex.Message });
            }
        }

        // Şirketleri API'den çek
        private async Task<List<CompanyViewModel>> GetCompaniesAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var apiUrl = _configuration["Api:BaseUrl"] + "/api/companies";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CompanyViewModel>>(json) ?? new List<CompanyViewModel>();
                }
            }
            catch
            {
                // Hata durumunda boş liste döndür
            }

            return new List<CompanyViewModel>();
        }
    }
}

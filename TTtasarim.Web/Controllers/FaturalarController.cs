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

            try
            {
                // Şirketleri ve kredi durumunu al
                var companies = await GetCompaniesAsync();
                var creditStatus = await GetCreditStatusAsync();

                ViewBag.Companies = companies;
                ViewBag.CreditStatus = creditStatus;
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Sayfa yüklenirken hata oluştu: " + ex.Message;
                ViewBag.Companies = new List<CompanyViewModel>();
                ViewBag.CreditStatus = null;
                return View();
            }
        }

        // AJAX ile fatura sorgulama
        [HttpPost]
        public async Task<IActionResult> QueryInvoices([FromBody] QueryInvoicesRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CompanyId) || string.IsNullOrEmpty(request.AccessNo))
                {
                    return Json(new { success = false, message = "Şirket ve abone numarası gerekli" });
                }

                var client = _clientFactory.CreateClient();
                var apiUrl = _configuration["Api:BaseUrl"] + $"/api/invoices/query?companyId={request.CompanyId}&accessNo={request.AccessNo}";
                var response = await client.GetAsync(apiUrl);

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<List<InvoiceDto>>>(json);

                if (response.IsSuccessStatusCode && result?.Success == true)
                {
                    return Json(new { 
                        success = true, 
                        message = result.Message,
                        data = result.Data 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = result?.Message ?? "Fatura sorgulaması başarısız" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // Fatura ödeme
        [HttpPost]
        public async Task<IActionResult> PayInvoice([FromBody] PayInvoiceRequest request)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "Oturum süresi dolmuş, lütfen tekrar giriş yapın" });

                if (string.IsNullOrEmpty(request.InvoiceId))
                    return Json(new { success = false, message = "Geçersiz fatura ID" });

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var paymentRequest = new { InvoiceId = request.InvoiceId };
                var json = JsonConvert.SerializeObject(paymentRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/invoices/pay";
                var response = await client.PostAsync(apiUrl, content);

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseJson);

                if (response.IsSuccessStatusCode && result?.success == true)
                {
                    return Json(new { 
                        success = true, 
                        message = result.message?.ToString(),
                        data = new {
                            paidAmount = result.payment?.paidAmount,
                            remainingCredit = result.payment?.remainingCredit,
                            dealerName = result.dealer?.name,
                            transactionId = result.payment?.transactionId
                        }
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = result?.message?.ToString() ?? "Ödeme işlemi başarısız" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ödeme hatası: " + ex.Message });
            }
        }

        // Ödeme geçmişi
        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/invoices/history";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<List<PaymentHistoryDto>>>(json);
                    
                    ViewBag.PaymentHistory = result?.Data ?? new List<PaymentHistoryDto>();
                    ViewBag.Message = result?.Message;
                }
                else
                {
                    ViewBag.PaymentHistory = new List<PaymentHistoryDto>();
                    ViewBag.Hata = "Ödeme geçmişi alınamadı";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.PaymentHistory = new List<PaymentHistoryDto>();
                ViewBag.Hata = "Bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Kredi durumu (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCreditStatus()
        {
            try
            {
                var creditStatus = await GetCreditStatusAsync();
                if (creditStatus != null)
                {
                    return Json(new { success = true, data = creditStatus });
                }
                else
                {
                    return Json(new { success = false, message = "Kredi durumu alınamadı" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // Helper: Şirketleri API'den çek
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
                    var companies = JsonConvert.DeserializeObject<List<CompanyViewModel>>(json);
                    return companies ?? new List<CompanyViewModel>();
                }
            }
            catch
            {
                // Hata durumunda boş liste döndür
            }

            return new List<CompanyViewModel>();
        }

        // Helper: Kredi durumunu API'den çek
        private async Task<CreditStatusDto?> GetCreditStatusAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return null;

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = _configuration["Api:BaseUrl"] + "/api/invoices/credit-status";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<CreditStatusData>>(json);
                    
                    if (result?.Success == true && result.Data != null)
                    {
                        return new CreditStatusDto
                        {
                            DealerName = result.Data.Dealer?.Name ?? "",
                            DealerCode = result.Data.Dealer?.Code ?? "",
                            CurrentCredit = result.Data.Credit?.CurrentValue ?? 0,
                            FormattedCredit = result.Data.Credit?.FormattedValue ?? "₺0,00"
                        };
                    }
                }
            }
            catch
            {
                // Hata durumunda null döndür
            }

            return null;
        }
    }

    // Request/Response models
    public class QueryInvoicesRequest
    {
        public string CompanyId { get; set; } = string.Empty;
        public string AccessNo { get; set; } = string.Empty;
    }

    public class PayInvoiceRequest
    {
        public string InvoiceId { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class InvoiceDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccessNo { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
    }

    public class PaymentHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccessNo { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string DealerName { get; set; } = string.Empty;
    }

    public class CreditStatusDto
    {
        public string DealerName { get; set; } = string.Empty;
        public string DealerCode { get; set; } = string.Empty;
        public decimal CurrentCredit { get; set; }
        public string FormattedCredit { get; set; } = string.Empty;
    }

    public class CreditStatusData
    {
        public DealerInfo? Dealer { get; set; }
        public CreditInfo? Credit { get; set; }
    }

    public class DealerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class CreditInfo
    {
        public decimal CurrentValue { get; set; }
        public string FormattedValue { get; set; } = string.Empty;
    }
}
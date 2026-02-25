using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RealEstateSystem.Controllers
{
    public class SSLCommerzController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        public SSLCommerzController(ApplicationDbContext context, IConfiguration config, IHttpClientFactory httpFactory)
        {
            _context = context;
            _config = config;
            _httpFactory = httpFactory;
        }

        

        // 1) INIT: Create Session + Redirect to Gateway
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Init(int invoiceId)
        {
            // Load invoice + property + seller user info 
            var invoice = await _context.CommissionInvoices
                .Include(i => i.Property)
                .Include(i => i.Seller)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(i => i.CommissionInvoiceId == invoiceId);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != CommissionInvoiceStatus.Unpaid &&
                invoice.Status != CommissionInvoiceStatus.Rejected)
            {
                TempData["Error"] = "This invoice is not eligible for payment.";
                return RedirectToAction("Index", "SellerProperties");
            }

            // Build unique transaction id
            string tranId = $"COMM_INV_{invoice.CommissionInvoiceId}_{Guid.NewGuid().ToString("N").Substring(0, 10)}_{DateTime.Now:yyyyMMddHHmmss}";

            invoice.GatewayTranId = tranId;
            invoice.GatewayValId = "";
            invoice.GatewayStatus = "INITIATED";
            await _context.SaveChangesAsync();

            // SSLCOMMERZ settings
            var storeId = _config["SSLCommerz:StoreId"];
            var storePass = _config["SSLCommerz:StorePassword"];
            var baseUrl = _config["SSLCommerz:BaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(storePass) || string.IsNullOrWhiteSpace(baseUrl))
            {
                TempData["Error"] = "SSLCOMMERZ config missing. Check appsettings.json (StoreId/StorePassword/BaseUrl).";
                return RedirectToAction("Index", "SellerProperties");
            }

            // Callback base:
            // Prefer config if provided (useful for ngrok / deployed HTTPS),
            // otherwise fall back to current request host.
            var callbackBaseFromConfig = _config["SSLCommerz:CallbackBaseUrl"]?.TrimEnd('/');
            string callbackBase = !string.IsNullOrWhiteSpace(callbackBaseFromConfig)
                ? callbackBaseFromConfig
                : $"{Request.Scheme}://{Request.Host}";

            // Initiate endpoint
            string initiateUrl = $"{baseUrl}/gwprocess/v4/api.php";

            // Real customer info
            var user = invoice.Seller?.User;
            string cusName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Seller";
            string cusEmail = user?.Email ?? "seller@example.com";
            string cusPhone = user?.PhoneNumber ?? "01700000000";

            // Amount format must be dot-decimal
            string totalAmount = invoice.CommissionAmount.ToString("0.00", CultureInfo.InvariantCulture);

            // Safety: amount must be positive
            if (!decimal.TryParse(totalAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) || amt <= 0)
            {
                TempData["Error"] = "Invalid commission amount. Please contact admin.";
                return RedirectToAction("Index", "SellerProperties");
            }

            // SSLCOMMERZ initiate fields
            var postData = new Dictionary<string, string>
            {
                ["store_id"] = storeId,
                ["store_passwd"] = storePass,
                ["total_amount"] = totalAmount,
                ["currency"] = "BDT",
                ["tran_id"] = tranId,

                ["success_url"] = $"{callbackBase}/SSLCommerz/Success",
                ["fail_url"] = $"{callbackBase}/SSLCommerz/Fail",
                ["cancel_url"] = $"{callbackBase}/SSLCommerz/Cancel",
                ["ipn_url"] = $"{callbackBase}/SSLCommerz/IPN",

                // Highly recommended / often required
                ["product_name"] = "Property Commission",
                ["product_category"] = "Service",
                ["product_profile"] = "general",
                ["shipping_method"] = "NO",
                ["cart_id"] = tranId,

                // Customer info
                ["cus_name"] = cusName,
                ["cus_email"] = cusEmail,
                ["cus_add1"] = "Dhaka",
                ["cus_city"] = "Dhaka",
                ["cus_state"] = "Dhaka",
                ["cus_postcode"] = "1200",
                ["cus_country"] = "Bangladesh",
                ["cus_phone"] = cusPhone,

                // Optional mapping
                ["value_a"] = invoice.CommissionInvoiceId.ToString()
            };

            string json;

            try
            {
                var client = _httpFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(25);

                using var response = await client.PostAsync(initiateUrl, new FormUrlEncodedContent(postData));
                json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    //  user-friendly message (toast)
                    TempData["Error"] = "Payment gateway initialization failed. Please try again later.";

                    //  keep details out of UI (but you can log it)
                    Console.WriteLine("SSLCOMMERZ init HTTP failed: " + json);

                    return RedirectToAction("Index", "SellerProperties");
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = "SSLCommerz sandbox is currently unavailable. Please try again later.";
                Console.WriteLine("SSLCOMMERZ HttpRequestException: " + ex.Message);
                return RedirectToAction("Index", "SellerProperties");
            }
            catch (TaskCanceledException)
            {
                TempData["Warning"] = "SSLCommerz gateway timeout. Please try again.";
                Console.WriteLine("SSLCOMMERZ timeout while initiating payment.");
                return RedirectToAction("Index", "SellerProperties");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Payment gateway error. Please try again later.";
                Console.WriteLine("SSLCOMMERZ unknown error: " + ex);
                return RedirectToAction("Index", "SellerProperties");
            }

            // Parse response JSON safely
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(json);
            }
            catch
            {
                TempData["Error"] = "Payment gateway returned an unexpected response. Please try again later.";
                Console.WriteLine("SSLCOMMERZ returned non-JSON: " + json);
                return RedirectToAction("Index", "SellerProperties");
            }

            // GatewayPageURL can be missing or empty when failed
            if (!doc.RootElement.TryGetProperty("GatewayPageURL", out var urlProp))
            {
                TempData["Error"] = "Payment gateway initialization failed. Please try again later.";
                Console.WriteLine("SSLCOMMERZ missing GatewayPageURL. Full: " + json);
                return RedirectToAction("Index", "SellerProperties");
            }

            var gatewayUrl = urlProp.GetString();

            // Try to extract extra info
            string status = doc.RootElement.TryGetProperty("status", out var st) ? st.GetString() : null;
            string failedReason = doc.RootElement.TryGetProperty("failedreason", out var fr) ? fr.GetString() : null;

            if (string.IsNullOrWhiteSpace(gatewayUrl))
            {
                TempData["Error"] = "Payment gateway failed to start. Please try again later.";
                Console.WriteLine($"SSLCOMMERZ GatewayPageURL empty. status={status}, failedreason={failedReason}. Full: {json}");
                return RedirectToAction("Index", "SellerProperties");
            }

            return Redirect(gatewayUrl);
        }



        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Success()
        {
            var form = Request.Form;

            string tranId = form["tran_id"];
            string valId = form["val_id"];

            if (string.IsNullOrWhiteSpace(tranId) || string.IsNullOrWhiteSpace(valId))
            {
                TempData["Error"] = "SSLCOMMERZ success callback missing tran_id or val_id.";
                return RedirectToAction("Index", "SellerProperties");
            }

            var invoice = await _context.CommissionInvoices
                .Include(i => i.Property)
                .FirstOrDefaultAsync(i => i.GatewayTranId == tranId);

            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found for this SSLCOMMERZ transaction.";
                return RedirectToAction("Index", "SellerProperties");
            }

            bool ok = await ValidateAndMarkPaid(invoice, valId);

            if (!ok)
            {
                invoice.GatewayStatus = "VALIDATION_FAILED";
                await _context.SaveChangesAsync();

                TempData["Error"] = "Payment received but validation failed.";
                return RedirectToAction("Index", "SellerProperties");
            }

            // Optional: show seller name in layout if available
            var sellerUser = await _context.Sellers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SellerId == invoice.SellerId);

            if (sellerUser?.User != null)
                ViewData["SellerDisplayName"] = $"{sellerUser.User.FirstName} {sellerUser.User.LastName}".Trim();

            ViewData["PageTitle"] = "Payment Successful";
            ViewData["PageSubtitle"] = "Your commission payment has been completed.";

            var vm = new RealEstateSystem.ViewModels.PaymentSuccessViewModel
            {
                InvoiceId = invoice.CommissionInvoiceId,
                PropertyTitle = invoice.Property?.Title ?? "Property",
                TransactionId = invoice.GatewayTranId ?? tranId,
                AmountPaid = invoice.CommissionAmount,
                StatusText = "Success",
                PaymentDate = invoice.VerifiedDate ?? DateTime.Now,
                Message = "Payment completed successfully"
            };

            return View(vm); // => Views/SSLCommerz/Success.cshtml
        }



        // 3) FAIL
        
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Fail()
        {
            var tranId = Request.Form["tran_id"].ToString();

            if (!string.IsNullOrWhiteSpace(tranId))
            {
                var invoice = await _context.CommissionInvoices.FirstOrDefaultAsync(i => i.GatewayTranId == tranId);
                if (invoice != null)
                {
                    invoice.GatewayStatus = "FAILED";
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Error"] = "Payment failed. Please try again.";
            return RedirectToAction("Index", "SellerProperties");
        }

        // 4) CANCEL
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Cancel()
        {
            var tranId = Request.Form["tran_id"].ToString();

            if (!string.IsNullOrWhiteSpace(tranId))
            {
                var invoice = await _context.CommissionInvoices.FirstOrDefaultAsync(i => i.GatewayTranId == tranId);
                if (invoice != null)
                {
                    invoice.GatewayStatus = "CANCELLED";
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Error"] = "Payment cancelled.";
            return RedirectToAction("Index", "SellerProperties");
        }

        // 5) IPN: Server to server notification
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> IPN()
        {
            var form = Request.Form;

            string tranId = form["tran_id"];
            string valId = form["val_id"];

            if (string.IsNullOrWhiteSpace(tranId) || string.IsNullOrWhiteSpace(valId))
                return Ok();

            var invoice = await _context.CommissionInvoices
                .FirstOrDefaultAsync(i => i.GatewayTranId == tranId);

            if (invoice == null)
                return Ok();

            if (invoice.Status == CommissionInvoiceStatus.Paid)
                return Ok();

            await ValidateAndMarkPaid(invoice, valId);
            return Ok();
        }

        // Helper: Validation API Call + Mark Paid
        private async Task<bool> ValidateAndMarkPaid(CommissionInvoice invoice, string valId)
        {
            var storeId = _config["SSLCommerz:StoreId"];
            var storePass = _config["SSLCommerz:StorePassword"];
            var baseUrl = _config["SSLCommerz:BaseUrl"]?.TrimEnd('/');

            string validateUrl =
                $"{baseUrl}/validator/api/validationserverAPI.php?val_id={Uri.EscapeDataString(valId)}" +
                $"&store_id={Uri.EscapeDataString(storeId)}&store_passwd={Uri.EscapeDataString(storePass)}&format=json";

            string json;
            try
            {
                var client = _httpFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                json = await client.GetStringAsync(validateUrl);
            }
            catch
            {
                return false;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(json);
            }
            catch
            {
                return false;
            }

            string apiStatus = doc.RootElement.TryGetProperty("status", out var st) ? (st.GetString() ?? "") : "";

            string apiCurrency = doc.RootElement.TryGetProperty("currency", out var cur)
                ? (cur.GetString() ?? "")
                : "";

            string apiAmountStr = doc.RootElement.TryGetProperty("amount", out var amt)
                ? (amt.GetString() ?? "")
                : "";

            if (!decimal.TryParse(apiAmountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var apiAmount))
                apiAmount = -1;

            bool statusOk = apiStatus.Equals("VALID", StringComparison.OrdinalIgnoreCase)
                         || apiStatus.Equals("VALIDATED", StringComparison.OrdinalIgnoreCase);

            bool currencyOk = apiCurrency.Equals("BDT", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(apiCurrency);

            bool amountOk = apiAmount >= 0 && Math.Round(apiAmount, 2) == Math.Round(invoice.CommissionAmount, 2);

            if (!statusOk || !amountOk || !currencyOk)
                return false;

            //  Extract SSLCommerz bank transaction id (real transaction ID)
            string bankTranId = doc.RootElement.TryGetProperty("bank_tran_id", out var bt)
                ? (bt.GetString() ?? "")
                : "";

            //  Mark invoice paid
            invoice.GatewayValId = valId;
            invoice.GatewayStatus = apiStatus;
            invoice.Status = CommissionInvoiceStatus.Paid;
            invoice.VerifiedDate = DateTime.Now;

            //  Save Transaction ID for receipt (bank_tran_id if available, otherwise fallback)
            invoice.TransactionId = !string.IsNullOrWhiteSpace(bankTranId)
                ? bankTranId
                : invoice.GatewayTranId;

            await _context.SaveChangesAsync();
            return true;
        }


    }
}

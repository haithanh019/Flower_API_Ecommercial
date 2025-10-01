using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataAccess.DTOs.CartDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory http, ILogger<IndexModel> logger)
        {
            _http = http;
            _logger = logger;
        }

        // OData: resource theo controller hiện tại
        private const string CART_RESOURCE = "Carts";

        public CartDto? Cart { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        private bool NotLoggedIn =>
            string.IsNullOrWhiteSpace(HttpContext.Session.GetString("JWToken"));

        // GET /Cart
        public async Task<IActionResult> OnGetAsync()
        {
            // Chưa đăng nhập -> chuyển thẳng sang trang login
            if (NotLoggedIn)
            {
                _logger.LogInformation("[Cart] Not logged in -> redirect to login");
                return Redirect("/Account/Login?returnUrl=/Cart");
            }

            if (TempData.TryGetValue("Success", out var s))
                SuccessMessage = s?.ToString();
            if (TempData.TryGetValue("Error", out var e))
                ErrorMessage = e?.ToString();

            Cart = await LoadCartAsync();
            if (Cart is null)
                ErrorMessage ??= "Không tải được giỏ hàng.";

            return Page();
        }

        // POST /Cart?handler=UpdateQty
        public async Task<IActionResult> OnPostUpdateQtyAsync(int cartItemId, int qty)
        {
            if (NotLoggedIn)
                return Redirect("/Account/Login?returnUrl=/Cart");

            var client = _http.CreateClient("odata");
            try
            {
                var url = $"{CART_RESOURCE}(1)"; // key chỉ để khớp route OData
                var body = new CartUpdateQtyRequest
                {
                    CartItemId = cartItemId,
                    Quantity = Math.Max(0, qty), // 0 => xoá item theo service
                };

                _logger.LogInformation("[Cart] PUT {Url} body={@Body}", url, body);
                var res = await client.PutAsJsonAsync(url, body);
                _logger.LogInformation("[Cart] PUT status {Status}", (int)res.StatusCode);

                if (res.StatusCode == HttpStatusCode.Unauthorized)
                    return Redirect("/Account/Login?returnUrl=/Cart");

                if (!res.IsSuccessStatusCode)
                    TempData["Error"] = $"Cập nhật số lượng thất bại ({(int)res.StatusCode}).";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cart] UpdateQty failed");
                TempData["Error"] = "Không thể cập nhật số lượng.";
            }
            return RedirectToPage();
        }

        // POST /Cart?handler=Remove  (xoá một dòng -> set qty = 0)
        public Task<IActionResult> OnPostRemoveAsync(int cartItemId) =>
            OnPostUpdateQtyAsync(cartItemId, 0);

        // POST /Cart?handler=Clear  (xoá toàn bộ)
        public async Task<IActionResult> OnPostClearAsync()
        {
            if (NotLoggedIn)
                return Redirect("/Account/Login?returnUrl=/Cart");

            var client = _http.CreateClient("odata");
            try
            {
                var url = $"{CART_RESOURCE}(1)";
                _logger.LogInformation("[Cart] DELETE {Url}", url);
                var res = await client.DeleteAsync(url);
                _logger.LogInformation("[Cart] DELETE status {Status}", (int)res.StatusCode);

                if (res.StatusCode == HttpStatusCode.Unauthorized)
                    return Redirect("/Account/Login?returnUrl=/Cart");

                if (!res.IsSuccessStatusCode)
                    TempData["Error"] = $"Không thể xoá giỏ ({(int)res.StatusCode}).";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cart] Clear failed");
                TempData["Error"] = "Không thể xoá giỏ hàng.";
            }
            return RedirectToPage();
        }

        // GET /Cart?handler=Count  (badge giỏ)
        public async Task<IActionResult> OnGetCountAsync()
        {
            if (NotLoggedIn)
                return new JsonResult(new { ok = false, count = 0 });

            var dto = await LoadCartAsync();
            return new JsonResult(new { ok = dto != null, count = dto?.ItemCount ?? 0 });
        }

        // ===== Helpers =====
        private async Task<CartDto?> LoadCartAsync()
        {
            var client = _http.CreateClient("odata");

            // BẮT BUỘC expand Items để OData serialize danh sách dòng giỏ
            var url =
                "Carts?"
                + "$select=CartId,UserId,ItemCount,Subtotal,CreatedAt,UpdatedAt"
                + "&$expand=Items("
                + "$select=CartItemId,CartId,ProductId,ProductName,ImageUrl,Quantity,UnitPrice"
                + ")";

            _logger.LogInformation("[Cart] GET {Url}", client.BaseAddress + url);

            var res = await client.GetAsync(url);
            _logger.LogInformation("[Cart] GET status {Status}", (int)res.StatusCode);

            if (!res.IsSuccessStatusCode)
            {
                ErrorMessage = res.StatusCode
                    is HttpStatusCode.Unauthorized
                        or HttpStatusCode.Forbidden
                    ? "Bạn cần đăng nhập để xem giỏ hàng."
                    : $"Không tải được giỏ hàng ({(int)res.StatusCode}).";
                return null;
            }

            var raw = await res.Content.ReadAsStringAsync();
            _logger.LogDebug("[Cart] Payload: {Raw}", raw);

            var data = JsonSerializer.Deserialize<OWrapper<CartDto>>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var dto = data?.Value?.FirstOrDefault();
            _logger.LogInformation("[Cart] Loaded {Count} items", dto?.Items?.Count ?? -1);

            return dto;
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Business.Entities;
using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.OrderDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Orders
{
    public class CheckoutModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<CheckoutModel> _logger;

        public CheckoutModel(IHttpClientFactory http, ILogger<CheckoutModel> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ---- Query inputs (GET) ----
        [BindProperty(SupportsGet = true)]
        public int Pid { get; set; } // ProductId

        [BindProperty(SupportsGet = true)]
        public int Qty { get; set; } = 1;

        // ---- Form inputs (POST) ----
        [BindProperty, Required, MaxLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [BindProperty]
        [Range(0, 2)] // 0: Cash/COD, 1: Bank, 2: EWallet
        public int PaymentMethod { get; set; } = 0;

        // ---- View models ----
        public ProductDto? Product { get; set; }
        public decimal Subtotal => (Product?.Price ?? 0m) * Math.Max(1, Qty);

        public string? ErrorMessage { get; set; }

        public record CategoryOption(int Id, string Name);

        public List<CategoryOption> Categories { get; set; } = new();

        public string CategoryName(int id) =>
            Categories.FirstOrDefault(x => x.Id == id)?.Name ?? $"#{id}";

        public async Task<IActionResult> OnGetAsync()
        {
            if (Pid <= 0 || Qty <= 0)
            {
                ErrorMessage = "Thiếu thông tin sản phẩm.";
                return Page();
            }

            // Kiểm tra đăng nhập (JWT trong Session)
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt hàng.";
                return Redirect("/Login");
            }

            await LoadCategoriesAsync();

            var client = _http.CreateClient("odata");
            try
            {
                Product = await client.GetFromJsonAsync<ProductDto>($"Products({Pid})");
                if (Product is null || !Product.IsActive)
                {
                    ErrorMessage = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout] load product failed");
                ErrorMessage = "Không tải được thông tin sản phẩm.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // re-load product to compute subtotal & show page on error
            await LoadCategoriesAsync();
            var client = _http.CreateClient("odata");
            Product = await client.GetFromJsonAsync<ProductDto>($"Products({Pid})");

            if (!ModelState.IsValid || Product is null)
                return Page();

            // Build OrderPlaceDto
            var dto = new OrderPlaceDto
            {
                // CustomerId sẽ được API gán từ JWT khi role=Customer
                ShippingAddress = ShippingAddress.Trim(),
                PaymentMethod = (PaymentMethod)PaymentMethod, // tương thích enum cũ
                Items = new List<OrderItemPlaceDto>
                {
                    new OrderItemPlaceDto { ProductId = Pid, Quantity = Math.Max(1, Qty) },
                },
            };

            try
            {
                var res = await client.PostAsJsonAsync("Orders", dto);
                var body = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    {
                        TempData["Error"] = "Bạn cần đăng nhập bằng tài khoản Khách hàng.";
                        return Redirect("/Login");
                    }
                    // Trả lỗi hiển thị lại trang
                    ErrorMessage = $"Đặt hàng thất bại ({(int)res.StatusCode}). {body}";
                    return Page();
                }

                // Lấy đơn vừa tạo để redirect chi tiết
                var created = await res.Content.ReadFromJsonAsync<OrderDto>();
                if (created?.OrderId > 0)
                    return Redirect($"/Orders/Details?id={created.OrderId}");

                TempData["Success"] = "Đặt hàng thành công.";
                return Redirect("/Orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout] place order failed");
                ErrorMessage = "Không thể đặt hàng lúc này. Vui lòng thử lại.";
                return Page();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var client = _http.CreateClient("odata");
            try
            {
                var data = await client.GetFromJsonAsync<OWrapper<CategoryDto>>(
                    "Categories?$select=CategoryId,CategoryName&$filter=IsActive eq true&$orderby=CategoryName"
                );
                Categories = (data?.Value ?? new List<CategoryDto>())
                    .Select(x => new CategoryOption(x.CategoryId, x.CategoryName))
                    .ToList();
            }
            catch
            {
                Categories = new();
            }
        }
    }
}

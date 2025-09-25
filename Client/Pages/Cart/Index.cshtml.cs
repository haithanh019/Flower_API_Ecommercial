using Client.Models;
using Client.Utils;
using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.ProductDTOs;
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

        private const string CART_KEY = "CART";

        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public record CategoryOption(int Id, string Name);

        public List<CategoryOption> Categories { get; set; } = new();

        public string CategoryName(int id) =>
            Categories.FirstOrDefault(x => x.Id == id)?.Name ?? $"#{id}";

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
            Items = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new();
        }

        // Dùng ở trang Shop để thêm nhanh
        public async Task<IActionResult> OnPostAddAsync(int productId, int qty = 1)
        {
            qty = Math.Max(1, qty);
            try
            {
                var client = _http.CreateClient("odata");
                var p = await client.GetFromJsonAsync<ProductDto>($"Products({productId})");
                if (p == null || !p.IsActive)
                    return BadRequest("Sản phẩm không khả dụng.");

                var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new();
                var ex = cart.FirstOrDefault(x => x.ProductId == productId);
                if (ex == null)
                {
                    cart.Add(
                        new CartItem
                        {
                            ProductId = p.ProductId,
                            ProductName = p.ProductName,
                            UnitPrice = p.Price,
                            Quantity = qty,
                            CategoryId = p.CategoryId,
                            ImageUrl = !string.IsNullOrWhiteSpace(p.ImageUrl)
                                ? p.ImageUrl
                                : p.ImageUrls?.FirstOrDefault(),
                        }
                    );
                }
                else
                {
                    ex.Quantity = Math.Min(999, ex.Quantity + qty);
                }
                HttpContext.Session.SetObject(CART_KEY, cart);
                TempData["Success"] = "Đã thêm vào giỏ hàng.";
                return RedirectToPage("/Cart/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cart] Add failed");
                TempData["Error"] = "Không thể thêm vào giỏ.";
                return RedirectToPage("/Cart/Index");
            }
        }

        public async Task<IActionResult> OnPostUpdateQtyAsync(int productId, int qty)
        {
            await LoadCategoriesAsync();
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new();
            var it = cart.FirstOrDefault(x => x.ProductId == productId);
            if (it != null)
            {
                if (qty <= 0)
                    cart.Remove(it);
                else
                    it.Quantity = Math.Min(999, qty);
                HttpContext.Session.SetObject(CART_KEY, cart);
                SuccessMessage = "Đã cập nhật số lượng.";
            }
            Items = cart;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int productId)
        {
            await LoadCategoriesAsync();
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new();
            cart.RemoveAll(x => x.ProductId == productId);
            HttpContext.Session.SetObject(CART_KEY, cart);
            Items = cart;
            SuccessMessage = "Đã xoá sản phẩm.";
            return Page();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            await LoadCategoriesAsync();
            HttpContext.Session.Remove(CART_KEY);
            Items = new();
            SuccessMessage = "Đã xoá giỏ hàng.";
            return Page();
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

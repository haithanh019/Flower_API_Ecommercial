using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Shop
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IHttpClientFactory http, ILogger<DetailsModel> logger)
        {
            _http = http;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int id { get; set; }

        public ProductDto? Product { get; set; }
        public List<string> Images { get; set; } = new();
        public List<ProductDto> Related { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public record CategoryOption(int Id, string Name);

        public List<CategoryOption> Categories { get; set; } = new();

        public string CategoryName(int id) =>
            Categories.FirstOrDefault(x => x.Id == id)?.Name ?? $"#{id}";

        public async Task<IActionResult> OnGetAsync()
        {
            if (id <= 0)
            {
                return Redirect("/Shop");
            }

            await LoadCategoriesAsync();

            var client = _http.CreateClient("odata");
            try
            {
                // Lấy sản phẩm
                Product = await client.GetFromJsonAsync<ProductDto>($"Products({id})");
                if (Product is null)
                {
                    ErrorMessage = "Không tìm thấy sản phẩm.";
                    return Page();
                }

                // Lấy tối đa 4 ảnh: ưu tiên ImageUrls, fallback ImageUrl
                var imgs = (Product.ImageUrls ?? new List<string>());
                if (imgs.Count == 0 && !string.IsNullOrWhiteSpace(Product.ImageUrl))
                    imgs.Add(Product.ImageUrl);
                Images = imgs.Take(4).ToList();

                // Gợi ý cùng danh mục
                var url =
                    $"Products?$top=8&$orderby=ProductId desc&$filter=IsActive eq true and CategoryId eq {Product.CategoryId} and ProductId ne {Product.ProductId}";
                var wrap = await client.GetFromJsonAsync<OWrapper<ProductDto>>(url);
                Related = wrap?.Value ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Shop/Details] Load failed");
                ErrorMessage = "Không tải được dữ liệu sản phẩm.";
            }

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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Details] LoadCategories failed");
                Categories = new();
            }
        }
    }
}

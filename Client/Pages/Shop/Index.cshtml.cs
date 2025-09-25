using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Shop
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

        public List<ProductDto> Items { get; set; } = new();
        public int Total { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public string? ErrorMessage { get; set; }

        public record CategoryOption(int Id, string Name);

        public List<CategoryOption> Categories { get; set; } = new();

        public string CategoryName(int id) =>
            Categories.FirstOrDefault(x => x.Id == id)?.Name ?? $"#{id}";

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
            await LoadAsync();
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
                _logger.LogWarning(ex, "[LoadCategories] failed");
                Categories = new();
            }
        }

        private async Task LoadAsync()
        {
            var client = _http.CreateClient("odata");

            PageIndex = PageIndex > 0 ? PageIndex : 1;
            PageSize = PageSize > 0 ? PageSize : 12;

            var skip = (PageIndex - 1) * PageSize;

            var conditions = new List<string> { "IsActive eq true" };
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var escaped = Q.Replace("'", "''");
                conditions.Add($"contains(ProductName,'{escaped}')");
            }
            if (CategoryId.HasValue)
                conditions.Add($"CategoryId eq {CategoryId.Value}");

            var filter = $"&$filter={string.Join(" and ", conditions)}";
            var url =
                $"Products?$count=true&$orderby=ProductId desc&$top={PageSize}&$skip={skip}{filter}";

            try
            {
                var data = await client.GetFromJsonAsync<OWrapper<ProductDto>>(url);
                Items = data?.Value ?? new List<ProductDto>();
                Total = data?.Count ?? Items.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Shop] Load products failed");
                ErrorMessage = "Không tải được danh sách sản phẩm.";
            }
        }
    }
}

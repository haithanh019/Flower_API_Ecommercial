using System.Net.Http.Json;
using DataAccess.DTOs.OrderDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public IndexModel(IHttpClientFactory http) => _http = http;

        public List<OrderDto> Items { get; set; } = new();
        public int Total { get; set; }

        [BindProperty(SupportsGet = true, Name = "pg")]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; } // OrderStatus int value

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (TempData.TryGetValue("Success", out var s))
                SuccessMessage = s?.ToString();
            if (TempData.TryGetValue("Error", out var e))
                ErrorMessage = e?.ToString();
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var client = _http.CreateClient("odata");

            PageIndex = PageIndex <= 0 ? 1 : PageIndex;
            PageSize = PageSize <= 0 ? 10 : PageSize;

            var skip = (PageIndex - 1) * PageSize;

            var filter = "";
            if (Status.HasValue)
            {
                filter = $"&$filter=Status eq {Status.Value}";
            }

            var url =
                $"Orders?$count=true&$orderby=OrderId desc&$top={PageSize}&$skip={skip}{filter}";

            try
            {
                var data = await client.GetFromJsonAsync<OWrapper<OrderDto>>(url);
                Items = data?.Value ?? new();
                Total = data?.Count ?? Items.Count;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không tải được đơn hàng: {ex.Message}";
                Items.Clear();
                Total = 0;
            }
        }
    }
}

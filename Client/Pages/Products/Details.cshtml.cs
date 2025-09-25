using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public DetailsModel(IHttpClientFactory http) => _http = http;

        public ProductDto? Product { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public bool CanManage =>
            (
                HttpContext
                    .Session.GetString("RoleName")
                    ?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false
            );

        public class EditVM
        {
            public string ProductName { get; set; } = "";
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public int CategoryId { get; set; }
            public bool IsActive { get; set; }
            public List<string> ImageUrls { get; set; } = new(); // NEW
        }

        [BindProperty]
        public EditVM Edit { get; set; } = new();

        [BindProperty]
        public string? Edit_GalleryJson { get; set; } // NEW

        // categories for dropdown
        public record CategoryOption(int Id, string Name);

        public List<CategoryOption> Categories { get; set; } = new();

        public string CategoryName(int id) =>
            Categories.FirstOrDefault(x => x.Id == id)?.Name ?? $"#{id}";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadCategoriesAsync();
            await LoadAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (!CanManage)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { returnUrl = Url.Page("/Products/Details", new { id }) }
                );
            }

            // Parse gallery JSON -> List<string>
            var urls = new List<string>();
            if (!string.IsNullOrWhiteSpace(Edit_GalleryJson))
            {
                try
                {
                    urls = JsonSerializer.Deserialize<List<string>>(Edit_GalleryJson!) ?? new();
                }
                catch
                {
                    urls = new();
                }
            }

            var client = _http.CreateClient("odata");
            var dto = new ProductUpdateDto
            {
                ProductName = Edit.ProductName,
                Description = Edit.Description,
                Price = Edit.Price,
                StockQuantity = Edit.StockQuantity,
                CategoryId = Edit.CategoryId,
                IsActive = Edit.IsActive,
                ImageUrls = urls,
            };

            try
            {
                var res = await client.PutAsJsonAsync($"Products({id})", dto);
                if (!res.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        (
                            res.StatusCode == HttpStatusCode.Unauthorized
                            || res.StatusCode == HttpStatusCode.Forbidden
                        )
                            ? "Bạn không có quyền cập nhật sản phẩm."
                            : $"Cập nhật thất bại ({(int)res.StatusCode}).";
                }
                else
                {
                    SuccessMessage = "Cập nhật thành công.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối: {ex.Message}";
            }

            await LoadCategoriesAsync();
            await LoadAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!CanManage)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { returnUrl = Url.Page("/Products/Details", new { id }) }
                );
            }

            var client = _http.CreateClient("odata");
            try
            {
                var res = await client.DeleteAsync($"Products({id})");
                if (!res.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        (
                            res.StatusCode == HttpStatusCode.Unauthorized
                            || res.StatusCode == HttpStatusCode.Forbidden
                        )
                            ? "Bạn không có quyền xoá sản phẩm."
                            : $"Xoá thất bại ({(int)res.StatusCode}).";
                    return RedirectToPage("/Products/Details", new { id });
                }
                TempData["Success"] = "Đã xoá sản phẩm.";
                return RedirectToPage("/Products/Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi kết nối: {ex.Message}";
                return RedirectToPage("/Products/Details", new { id });
            }
        }

        // Upload nhiều ảnh (reuse từ Index)
        public async Task<IActionResult> OnPostUploadManyAsync(List<IFormFile> files)
        {
            try
            {
                if (!CanManage)
                    return new JsonResult(new { ok = false, message = "Không có quyền upload." });

                if (files is null || files.Count == 0)
                    return new JsonResult(new { ok = false, message = "Không có file." });

                const long maxBytes = 5 * 1024 * 1024; // 5MB/ảnh
                var uploadsDir = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "products"
                );
                Directory.CreateDirectory(uploadsDir);

                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var urls = new List<string>();

                foreach (var file in files)
                {
                    if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        return new JsonResult(new { ok = false, message = "Chỉ chấp nhận ảnh." });
                    if (file.Length > maxBytes)
                        return new JsonResult(new { ok = false, message = "Ảnh vượt quá 5MB." });

                    var ext = Path.GetExtension(file.FileName);
                    var safeExt = string.IsNullOrWhiteSpace(ext) ? ".jpg" : ext;
                    var fileName = $"{Guid.NewGuid():N}{safeExt}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using var fs = System.IO.File.Create(filePath);
                    await file.CopyToAsync(fs);

                    urls.Add($"{baseUrl}/uploads/products/{fileName}");
                }

                return new JsonResult(new { ok = true, urls });
            }
            catch
            {
                return new JsonResult(new { ok = false, message = "Upload lỗi, thử lại sau." });
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

        private async Task LoadAsync(int id)
        {
            var client = _http.CreateClient("odata");
            try
            {
                Product = await client.GetFromJsonAsync<ProductDto>($"Products({id})");
                if (Product != null)
                {
                    Edit = new EditVM
                    {
                        ProductName = Product.ProductName,
                        Description = Product.Description,
                        Price = Product.Price,
                        StockQuantity = Product.StockQuantity,
                        CategoryId = Product.CategoryId,
                        IsActive = Product.IsActive,
                        ImageUrls = Product.ImageUrls ?? new List<string>(),
                    };
                }
                if (TempData.TryGetValue("Success", out var s))
                    SuccessMessage = s?.ToString();
                if (TempData.TryGetValue("Error", out var e))
                    ErrorMessage = e?.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không tải được dữ liệu: {ex.Message}";
            }
        }
    }
}

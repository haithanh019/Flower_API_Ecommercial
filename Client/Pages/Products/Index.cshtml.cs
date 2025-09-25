using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Products
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

        [BindProperty(SupportsGet = true), ValidateNever]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true), ValidateNever]
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));

        [BindProperty(SupportsGet = true), ValidateNever]
        public string? Q { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public ProductCreateDto NewProduct { get; set; } = new();

        [BindProperty]
        public ProductUpdateDto EditProduct { get; set; } = new();

        [BindProperty(SupportsGet = true), ValidateNever]
        public int? CategoryId { get; set; }

        // nhận JSON gallery từ form (để map vào ImageUrl)
        [BindProperty]
        public string? NewProduct_GalleryJson { get; set; } // NEW

        [BindProperty]
        public string? EditProduct_GalleryJson { get; set; } // NEW

        public bool ShowCreateModal { get; set; } = false;

        public bool CanManage
        {
            get
            {
                var roleName = HttpContext.Session.GetString("RoleName");
                return (roleName?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false);
            }
        }

        // ====== Categories support ======
        public record CategoryOption(int Id, string Name);

        public List<CategoryOption> Categories { get; set; } = new();

        public string CategoryName(int id) =>
            Categories.FirstOrDefault(x => x.Id == id)?.Name ?? $"#{id}";

        private bool IsAjax =>
            HttpContext.Request.Headers.TryGetValue("X-Requested-With", out var v)
            && string.Equals(v, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        private void LogSession()
        {
            _logger.LogInformation(
                "[Session] UserId={UserId}, RoleName={RoleName}, HasJWToken={HasToken}",
                HttpContext.Session.GetInt32("UserId"),
                HttpContext.Session.GetString("RoleName"),
                !string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))
            );
        }

        private void LogModelState()
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("[ModelState] VALID");
                return;
            }
            foreach (var kv in ModelState)
            foreach (var e in kv.Value.Errors)
                _logger.LogWarning("[ModelState] {Key}: {Err}", kv.Key, e.ErrorMessage);
        }

        public async Task OnGetAsync()
        {
            LogSession();
            await LoadCategoriesAsync(); // NEW
            await LoadAsync();
            if (TempData.TryGetValue("Success", out var s))
                SuccessMessage = s?.ToString();
            if (TempData.TryGetValue("Error", out var e))
                ErrorMessage = e?.ToString();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            [FromForm] ProductCreateDto newProduct,
            string? q,
            int? categoryId,
            int? pg
        )
        {
            LogSession();
            if (!CanManage)
            {
                var msg = "Bạn không có quyền tạo sản phẩm.";
                if (IsAjax)
                    return new JsonResult(new { ok = false, message = msg });
                ErrorMessage = msg;
                ShowCreateModal = true;
                await LoadCategoriesAsync();
                await LoadAsync(q, categoryId, pg);
                return Page();
            }

            // Loại filter/paging khỏi ModelState
            ModelState.Remove(nameof(Q));
            ModelState.Remove(nameof(CategoryId));
            ModelState.Remove(nameof(PageIndex));
            ModelState.Remove(nameof(PageSize));

            // Gán & Validate
            NewProduct = newProduct;

            // Map ảnh đầu tiên (nếu có) vào ImageUrl
            if (!string.IsNullOrWhiteSpace(NewProduct_GalleryJson))
            {
                try
                {
                    NewProduct.ImageUrls =
                        JsonSerializer.Deserialize<List<string>>(NewProduct_GalleryJson!) ?? new();
                }
                catch
                {
                    NewProduct.ImageUrls = new();
                }
            }
            if (NewProduct.ImageUrls?.Count > 4)
            {
                ModelState.AddModelError(string.Empty, "Mỗi sản phẩm tối đa 4 ảnh.");
            }
            ModelState.Clear();
            TryValidateModel(NewProduct, nameof(NewProduct));
            LogModelState();

            if (!ModelState.IsValid)
            {
                var msg = string.Join(
                    "; ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                );
                if (IsAjax)
                    return new JsonResult(new { ok = false, message = msg });
                ErrorMessage = msg;
                ShowCreateModal = true;
                await LoadCategoriesAsync();
                await LoadAsync(q, categoryId, pg);
                return Page();
            }

            var client = _http.CreateClient("odata");
            try
            {
                var res = await client.PostAsJsonAsync("Products", NewProduct);
                var body = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode)
                {
                    var msg =
                        (
                            res.StatusCode == HttpStatusCode.Unauthorized
                            || res.StatusCode == HttpStatusCode.Forbidden
                        )
                            ? "Bạn không có quyền tạo sản phẩm."
                            : $"Tạo sản phẩm thất bại ({(int)res.StatusCode}). {body}";
                    if (IsAjax)
                        return new JsonResult(new { ok = false, message = msg });
                    ErrorMessage = msg;
                    ShowCreateModal = true;
                    await LoadCategoriesAsync();
                    await LoadAsync(q, categoryId, pg);
                    return Page();
                }

                var redirect = Url.Page(
                    "/Products/Index",
                    new
                    {
                        q,
                        categoryId,
                        pg,
                        pageSize = PageSize,
                    }
                );
                if (IsAjax)
                    return new JsonResult(new { ok = true, redirect });
                TempData["Success"] = "Tạo sản phẩm thành công.";
                return Redirect(redirect!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Create] Exception");
                var msg = $"Lỗi kết nối: {ex.Message}";
                if (IsAjax)
                    return new JsonResult(new { ok = false, message = msg });
                ErrorMessage = msg;
                ShowCreateModal = true;
                await LoadCategoriesAsync();
                await LoadAsync(q, categoryId, pg);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int productId,
            [FromForm] ProductUpdateDto editProduct,
            string? q,
            int? categoryId,
            int? pg
        )
        {
            LogSession();
            if (!CanManage)
            {
                var msg = "Bạn không có quyền cập nhật sản phẩm.";
                return IsAjax
                    ? new JsonResult(new { ok = false, message = msg })
                    : RedirectToPage(
                        new
                        {
                            q,
                            categoryId,
                            pg,
                            pageSize = PageSize,
                        }
                    );
            }

            ModelState.Remove(nameof(Q));
            ModelState.Remove(nameof(CategoryId));
            ModelState.Remove(nameof(PageIndex));
            ModelState.Remove(nameof(PageSize));

            EditProduct = editProduct;

            // Map ảnh đầu tiên nếu có
            if (!string.IsNullOrWhiteSpace(EditProduct_GalleryJson))
            {
                try
                {
                    EditProduct.ImageUrls =
                        JsonSerializer.Deserialize<List<string>>(EditProduct_GalleryJson!) ?? new();
                }
                catch
                {
                    EditProduct.ImageUrls = new();
                }
            }
            if (EditProduct.ImageUrls?.Count > 4)
            {
                ModelState.AddModelError(string.Empty, "Mỗi sản phẩm tối đa 4 ảnh.");
            }
            ModelState.Clear();
            TryValidateModel(EditProduct, nameof(EditProduct));
            LogModelState();

            if (!ModelState.IsValid)
            {
                var msg = string.Join(
                    "; ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                );
                return IsAjax
                    ? new JsonResult(new { ok = false, message = msg })
                    : RedirectToPage(
                        new
                        {
                            q,
                            categoryId,
                            pg,
                            pageSize = PageSize,
                        }
                    );
            }

            var client = _http.CreateClient("odata");
            try
            {
                var res = await client.PutAsJsonAsync($"Products({productId})", EditProduct);
                var body = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode)
                {
                    var msg =
                        (
                            res.StatusCode == HttpStatusCode.Unauthorized
                            || res.StatusCode == HttpStatusCode.Forbidden
                        )
                            ? "Bạn không có quyền cập nhật sản phẩm."
                            : $"Cập nhật thất bại ({(int)res.StatusCode}). {body}";
                    return IsAjax
                        ? new JsonResult(new { ok = false, message = msg })
                        : RedirectToPage(
                            new
                            {
                                q,
                                categoryId,
                                pg,
                                pageSize = PageSize,
                            }
                        );
                }

                var redirect = Url.Page(
                    "/Products/Index",
                    new
                    {
                        q,
                        categoryId,
                        pg,
                        pageSize = PageSize,
                    }
                );
                return IsAjax ? new JsonResult(new { ok = true, redirect }) : Redirect(redirect!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Update] Exception");
                var msg = $"Lỗi kết nối: {ex.Message}";
                return IsAjax
                    ? new JsonResult(new { ok = false, message = msg })
                    : RedirectToPage(
                        new
                        {
                            q,
                            categoryId,
                            pg,
                            pageSize = PageSize,
                        }
                    );
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(
            int productId,
            string? q,
            int? categoryId,
            int? pg
        )
        {
            LogSession();
            if (!CanManage)
            {
                var msg = "Bạn không có quyền xoá sản phẩm.";
                return IsAjax
                    ? new JsonResult(new { ok = false, message = msg })
                    : RedirectToPage(
                        new
                        {
                            q,
                            categoryId,
                            pg,
                            pageSize = PageSize,
                        }
                    );
            }

            var client = _http.CreateClient("odata");
            try
            {
                var res = await client.DeleteAsync($"Products({productId})");
                var body = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode)
                {
                    var msg =
                        (
                            res.StatusCode == HttpStatusCode.Unauthorized
                            || res.StatusCode == HttpStatusCode.Forbidden
                        )
                            ? "Bạn không có quyền xoá sản phẩm."
                            : $"Xoá thất bại ({(int)res.StatusCode}). {body}";
                    return IsAjax
                        ? new JsonResult(new { ok = false, message = msg })
                        : RedirectToPage(
                            new
                            {
                                q,
                                categoryId,
                                pg,
                                pageSize = PageSize,
                            }
                        );
                }

                var redirect = Url.Page(
                    "/Products/Index",
                    new
                    {
                        q,
                        categoryId,
                        pg,
                        pageSize = PageSize,
                    }
                );
                return IsAjax ? new JsonResult(new { ok = true, redirect }) : Redirect(redirect!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Delete] Exception");
                var msg = $"Lỗi kết nối: {ex.Message}";
                return IsAjax
                    ? new JsonResult(new { ok = false, message = msg })
                    : RedirectToPage(
                        new
                        {
                            q,
                            categoryId,
                            pg,
                            pageSize = PageSize,
                        }
                    );
            }
        }

        // ===== Helpers =====
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

        private async Task LoadAsync(string? q = null, int? categoryId = null, int? pg = null)
        {
            var client = _http.CreateClient("odata");

            PageIndex = pg.HasValue && pg.Value > 0 ? pg.Value : (PageIndex > 0 ? PageIndex : 1);
            PageSize = PageSize > 0 ? PageSize : 10;

            Q = q ?? Q;
            CategoryId = categoryId ?? CategoryId;

            var skip = (PageIndex - 1) * PageSize;

            var conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var escaped = Q.Replace("'", "''");
                conditions.Add($"contains(ProductName,'{escaped}')");
            }
            if (CategoryId.HasValue)
                conditions.Add($"CategoryId eq {CategoryId.Value}"); // NEW

            var filter = conditions.Count > 0 ? $"&$filter={string.Join(" and ", conditions)}" : "";
            var url =
                $"Products?$count=true&$orderby=ProductId desc&$top={PageSize}&$skip={skip}{filter}";
            _logger.LogInformation("[Load] OData URL: {Url}", url);

            try
            {
                var data = await client.GetFromJsonAsync<OWrapper<ProductDto>>(url);
                Items = data?.Value ?? new List<ProductDto>();
                Total = data?.Count ?? Items.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Load] Exception");
                ErrorMessage = $"Không tải được dữ liệu: {ex.Message}";
                Items = new();
                Total = 0;
            }
        }

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
            catch (Exception)
            {
                return new JsonResult(new { ok = false, message = "Upload lỗi, thử lại sau." });
            }
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataAccess.Auth;
using DataAccess.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory) =>
            _httpClientFactory = httpClientFactory;

        [BindProperty]
        public LoginDTO Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            // Nếu đã đăng nhập -> chuyển ra Home/Products
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                return LocalRedirect(Url.Content("~/"));
            }

            ReturnUrl = returnUrl ?? Request.Query["ReturnUrl"];
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("odata");

            HttpResponseMessage response;
            try
            {
                Input.Email = (Input.Email ?? string.Empty).Trim();
                response = await client.PostAsJsonAsync("Users", Input);
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Không kết nối được máy chủ. Vui lòng thử lại.";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage =
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Email hoặc mật khẩu không đúng."
                        : $"Đăng nhập thất bại ({(int)response.StatusCode}).";
                return Page();
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.Token))
            {
                ErrorMessage = "Phản hồi đăng nhập không hợp lệ.";
                return Page();
            }

            HttpContext.Session.SetString("JWToken", loginResponse.Token);
            HttpContext.Session.SetInt32("UserId", loginResponse.User.UserId);
            HttpContext.Session.SetString("UserEmail", loginResponse.User.Email ?? string.Empty);

            string roleName = string.Empty;
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse.Token);
                roleName =
                    jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                    ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(roleName))
                    HttpContext.Session.SetString("RoleName", roleName);
            }
            catch
            { /* ignore malformed token */
            }

            if (roleName == "Admin")
            {
                return LocalRedirect(Url.Content("~/Products/Index"));
            }

            var target =
                !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
                    ? ReturnUrl!
                    : Url.Content("~/");
            return LocalRedirect(target);
        }
    }
}

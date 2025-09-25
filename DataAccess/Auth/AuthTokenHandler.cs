using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DataAccess.Auth
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthTokenHandler> _logger;

        public AuthTokenHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthTokenHandler> logger
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation(
                    "[AuthTokenHandler] Attached Bearer token (len={Len}) to {Method} {Url}",
                    token.Length,
                    request.Method,
                    request.RequestUri
                );
            }
            else
            {
                _logger.LogWarning(
                    "[AuthTokenHandler] NO JWToken in session for {Method} {Url}",
                    request.Method,
                    request.RequestUri
                );
            }

            var res = await base.SendAsync(request, cancellationToken);
            _logger.LogInformation(
                "[AuthTokenHandler] => {StatusCode} for {Method} {Url}",
                (int)res.StatusCode,
                request.Method,
                request.RequestUri
            );
            return res;
        }
    }
}

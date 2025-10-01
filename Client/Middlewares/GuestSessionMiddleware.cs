using System.Security.Cryptography;

namespace Client.Middlewares
{
    public class GuestSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private const string Key = "GuestSessionId";

        public GuestSessionMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var sess = context.Session;
            if (string.IsNullOrWhiteSpace(sess.GetString(Key)))
            {
                var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                sess.SetString(Key, $"guest_{id}");
            }
            await _next(context);
        }
    }

    public static class GuestSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGuestSessionId(this IApplicationBuilder app) =>
            app.UseMiddleware<GuestSessionMiddleware>();
    }
}

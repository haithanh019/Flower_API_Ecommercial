using Business.DTOs.Auth;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(
                new WebApplicationOptions
                {
                    Args = args,
                    ContentRootPath = Directory.GetCurrentDirectory(),
                    ApplicationName = typeof(Program).Assembly.FullName,
                }
            );

            // Load custom config file: appsettings-web.json
            builder
                .Configuration.AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true
                )
                .AddJsonFile(
                    $"appsettings.{builder.Environment.EnvironmentName}.json",
                    optional: true
                )
                .AddEnvironmentVariables();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthTokenHandler>();

            // HttpClient for API
            builder
                .Services.AddHttpClient(
                    "odata",
                    c =>
                    {
                        var baseUrl = builder.Configuration["ApiSettings:BaseApiUrl"]?.TrimEnd('/');
                        if (string.IsNullOrEmpty(baseUrl))
                            throw new InvalidOperationException("BaseApiUrl is not configured.");

                        c.BaseAddress = new Uri($"{baseUrl}/odata/");
                    }
                )
                .AddHttpMessageHandler<AuthTokenHandler>();

            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(60);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();
            app.MapRazorPages();
            app.Run();
        }
    }
}

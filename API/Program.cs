using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccess;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.OrderDTOs;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.DTOs.UserDTOs;
using DataAccess.Helper;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Repositories.UnitOfWork;
using Services.FacadeService;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<FlowerShopDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            builder
                .Services.AddControllers()
                .AddOData(opt =>
                {
                    var modelBuilder = new ODataConventionModelBuilder();
                    modelBuilder.EntitySet<UserDto>("Users");
                    modelBuilder.EntitySet<ProductDto>("Products");
                    modelBuilder.EntitySet<OrderDto>("Orders");
                    modelBuilder.EntitySet<CategoryDto>("Categories");
                    modelBuilder.EntitySet<PaymentDto>("Payments");
                    modelBuilder.EntitySet<CartDto>("Carts");
                    modelBuilder.EntitySet<CartItemDto>("CartItems");
                    opt.AddRouteComponents("odata", modelBuilder.GetEdmModel())
                        .Select()
                        .Filter()
                        .OrderBy()
                        .Expand()
                        .SetMaxTop(100)
                        .Count()
                        .SkipToken();
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            // Add Authentication
            builder
                .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        ),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier,
                    };
                });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IFacadeService, FacadeService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FlowerShopDbContext>();
                db.Database.Migrate();
                SeedData.Initialize(db);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

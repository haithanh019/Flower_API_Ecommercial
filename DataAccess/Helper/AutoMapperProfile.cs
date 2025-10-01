using AutoMapper;
using Business.Entities;
using DataAccess.DTOs.CartDTOs;
// DTOs
using DataAccess.DTOs.CategoryDTOs;
using DataAccess.DTOs.OrderDTOs;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.DTOs.ProductImageDTOs;
using DataAccess.DTOs.UserDTOs;

namespace DataAccess.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ===== Category =====
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();

            // ===== Product =====
            CreateMap<Product, ProductDto>()
                .ForMember(
                    d => d.ImageUrl,
                    o =>
                        o.MapFrom(s =>
                            s.ProductImages.OrderBy(pi => pi.ProductImageId)
                                .Select(pi => pi.ImageUrl)
                                .FirstOrDefault()
                        )
                )
                .ForMember(
                    d => d.ImageUrls,
                    o =>
                        o.MapFrom(s =>
                            s.ProductImages.OrderBy(pi => pi.ProductImageId)
                                .Select(pi => pi.ImageUrl)
                        )
                );
            CreateMap<ProductCreateDto, Product>().ForMember(d => d.ProductImages, o => o.Ignore());

            CreateMap<ProductUpdateDto, Product>().ForMember(d => d.ProductImages, o => o.Ignore());

            // ===== ProductImage =====
            CreateMap<ProductImage, ProductImageDto>();

            // ===== User =====
            CreateMap<User, UserDto>();
            // Hash mật khẩu ở Service, không map trực tiếp vào PasswordHash
            CreateMap<UserCreateDto, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore());
            CreateMap<UserUpdateDto, User>().ForMember(d => d.PasswordHash, o => o.Ignore());

            // ===== Order =====
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(
                    d => d.ImageUrl,
                    o =>
                        o.MapFrom(s =>
                            s.Product.ProductImages.Select(pi => pi.ImageUrl).FirstOrDefault()
                        )
                );
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.FullName))
                .ForMember(d => d.Status, o => o.MapFrom(s => (int)s.Status))
                .ForMember(
                    d => d.PaymentStatus,
                    o => o.MapFrom(s => s.Payment != null ? (int?)s.Payment.Status : null)
                )
                .ForMember(
                    d => d.PaymentMethod,
                    o => o.MapFrom(s => s.Payment != null ? (int?)s.Payment.Method : null)
                )
                .ForMember(d => d.PaymentDate, o => o.MapFrom(s => s.Payment!.PaymentDate))
                .ForMember(d => d.TransactionId, o => o.MapFrom(s => s.Payment!.TransactionId))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
            // Nếu muốn dùng map khi nhận đơn từ client:
            CreateMap<OrderPlaceDto, Order>()
                .ForMember(d => d.Items, o => o.Ignore()) // build trong Service
                .ForMember(d => d.Payment, o => o.Ignore()) // tạo/ghép Payment ở Service
                .ForMember(d => d.TotalAmount, o => o.Ignore()) // tính trong Service
                .ForMember(d => d.Status, o => o.Ignore()); // set Pending ở Service nếu cần

            // ===== Payment =====
            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentCreateDto, Payment>()
                .ForMember(d => d.Status, o => o.MapFrom(_ => PaymentStatus.Pending))
                .ForMember(d => d.PaymentDate, o => o.Ignore());
            CreateMap<PaymentUpdateDto, Payment>();
            // ===== Cart =====
            CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(
                    d => d.ImageUrl,
                    o =>
                        o.MapFrom(s =>
                            s.Product.ProductImages.OrderBy(pi => pi.ProductImageId)
                                .Select(pi => pi.ImageUrl)
                                .FirstOrDefault()
                        )
                )
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity))
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId))
                .ForMember(d => d.CartId, o => o.MapFrom(s => s.CartId))
                .ForMember(d => d.CartItemId, o => o.MapFrom(s => s.CartItemId));

            CreateMap<Cart, CartDto>()
                // đưa danh sách item về đúng DTO
                .ForMember(
                    d => d.Items,
                    o => o.MapFrom(s => s.CartItems.OrderByDescending(x => x.CartItemId))
                )
                // “Số lượng” nên là tổng qty
                .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.CartItems.Sum(x => x.Quantity)))
                .ForMember(
                    d => d.Subtotal,
                    o => o.MapFrom(s => s.CartItems.Sum(x => x.UnitPrice * x.Quantity))
                );
        }
    }
}

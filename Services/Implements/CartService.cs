using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Entities;
using DataAccess.DTOs.CartDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.UnitOfWork;
using Services.Interfaces;

namespace Services.Implements
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork uow, IMapper mapper, ILogger<CartService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CartDto> GetOrCreateAsync(int userId)
        {
            var cart = await _uow.CartRepository.GetOrCreateByUserIdAsync(userId);
            return await ProjectCart(cart.CartId);
        }

        public async Task<CartDto> AddItemAsync(int userId, int productId, int quantity)
        {
            var cart = await _uow.CartRepository.GetOrCreateByUserIdAsync(userId);

            var product =
                await _uow.ProductRepository.GetByIdAsync(productId)
                ?? throw new InvalidOperationException($"Product {productId} not found");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is inactive");
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be > 0");

            var existed = await _uow.CartItemRepository.FindByCartAndProductAsync(
                cart.CartId,
                productId
            );
            if (existed is null)
            {
                await _uow.CartItemRepository.AddAsync(
                    new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        UnitPrice = product.Price, // lock giá tại thời điểm thêm
                    }
                );
            }
            else
            {
                existed.Quantity += quantity;
                await _uow.CartItemRepository.UpdateAsync(existed);
            }

            return await ProjectCart(cart.CartId);
        }

        public async Task<CartDto> UpdateItemQtyAsync(int userId, int cartItemId, int quantity)
        {
            var item =
                await _uow.CartItemRepository.GetByIdWithCartAsync(cartItemId)
                ?? throw new InvalidOperationException("Cart item not found");

            if (item.Cart.UserId != userId)
                throw new UnauthorizedAccessException();

            if (quantity <= 0)
            {
                await _uow.CartItemRepository.DeleteAsync(item);
            }
            else
            {
                item.Quantity = quantity;
                await _uow.CartItemRepository.UpdateAsync(item);
            }

            return await ProjectCart(item.CartId);
        }

        public async Task<bool> RemoveItemAsync(int userId, int cartItemId)
        {
            var item = await _uow.CartItemRepository.GetByIdWithCartAsync(cartItemId);
            if (item == null)
                return false;
            if (item.Cart.UserId != userId)
                throw new UnauthorizedAccessException();

            await _uow.CartItemRepository.DeleteAsync(item);
            return true;
        }

        public async Task<CartDto> ClearAsync(int userId)
        {
            var cart = await _uow.CartRepository.GetOrCreateByUserIdAsync(userId);
            await _uow.CartItemRepository.ClearByCartIdAsync(cart.CartId);
            return await ProjectCart(cart.CartId);
        }

        private async Task<CartDto> ProjectCart(int cartId)
        {
            _logger.LogInformation("[CartService] ProjectCart({CartId})", cartId);

            var q = _uow
                .CartRepository.Query()
                .Where(c => c.CartId == cartId)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.ProductImages);

            var dto = await q.ProjectTo<CartDto>(_mapper.ConfigurationProvider).SingleAsync();

            _logger.LogInformation(
                "[CartService] ProjectCart({CartId}) -> Items={Items}, ItemCount={ItemCount}, Subtotal={Subtotal}",
                cartId,
                dto.Items?.Count ?? -1,
                dto.ItemCount,
                dto.Subtotal
            );

            return dto;
        }
    }
}

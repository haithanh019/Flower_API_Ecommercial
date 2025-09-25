using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Entities;
using DataAccess.DTOs.OrderDTOs;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.Interfaces;

namespace Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<int> PlaceOrderAsync(OrderPlaceDto dto)
        {
            // Validate customer
            _ =
                await _uow.UserRepository.GetByIdAsync(dto.CustomerId)
                ?? throw new InvalidOperationException("Customer not found");

            decimal total = 0m;
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                ShippingAddress = dto.ShippingAddress,
                CustomerNote = dto.CustomerNote,
                Status = OrderStatus.Pending,
            };

            foreach (var i in dto.Items)
            {
                var product =
                    await _uow.ProductRepository.GetByIdAsync(i.ProductId)
                    ?? throw new InvalidOperationException($"Product {i.ProductId} not found");

                if (product.StockQuantity < i.Quantity)
                    throw new InvalidOperationException(
                        $"Not enough stock for {product.ProductName}"
                    );

                product.StockQuantity -= i.Quantity;
                await _uow.ProductRepository.UpdateAsync(product);

                order.Items.Add(
                    new OrderItem
                    {
                        ProductId = product.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = product.Price,
                    }
                );

                total += i.Quantity * product.Price;
            }

            order.TotalAmount = total;

            // Lưu Order trước để có OrderId
            await _uow.OrderRepository.AddAsync(order);

            // Tạo Payment pending 1–1
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = total,
                Method = dto.PaymentMethod,
                Status = PaymentStatus.Pending,
            };
            await _uow.PaymentRepository.AddAsync(payment);

            return order.OrderId;
        }

        public async Task<OrderDto?> GetByIdAsync(int orderId) =>
            await _uow
                .OrderRepository.Query()
                .Where(o => o.OrderId == orderId)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
    }
}

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

        public async Task<IEnumerable<OrderDto>> GetAllAsync() =>
            await _uow
                .OrderRepository.Query()
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId) =>
            await _uow
                .OrderRepository.Query()
                .Where(o => o.CustomerId == customerId)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        // THÊM METHOD MỚI: UpdateAsync
        public async Task<bool> UpdateAsync(int orderId, OrderUpdateDto dto)
        {
            var order = await _uow.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Chỉ cho phép update một số field nhất định
            order.Status = dto.Status;

            if (!string.IsNullOrEmpty(dto.ShippingAddress))
                order.ShippingAddress = dto.ShippingAddress;

            if (!string.IsNullOrEmpty(dto.CustomerNote))
                order.CustomerNote = dto.CustomerNote;

            order.UpdatedAt = DateTime.UtcNow;

            await _uow.OrderRepository.UpdateAsync(order);
            return true;
        }

        // THÊM METHOD MỚI: DeleteAsync (Soft delete hoặc cancel order)
        public async Task<bool> DeleteAsync(int orderId)
        {
            var order = await _uow
                .OrderRepository.Query()
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return false;

            // Chỉ cho phép delete/cancel nếu order chưa được giao
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Cannot cancel delivered or already cancelled order"
                );
            }

            // Hoàn lại stock nếu cancel order
            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing)
            {
                foreach (var item in order.Items)
                {
                    item.Product.StockQuantity += item.Quantity;
                    await _uow.ProductRepository.UpdateAsync(item.Product);
                }
            }

            // Soft delete - đánh dấu là cancelled thay vì xóa hẳn
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _uow.OrderRepository.UpdateAsync(order);
            return true;
        }
    }
}

using DataAccess.DTOs.OrderDTOs;

namespace Services.Interfaces;

public interface IOrderService
{
    Task<int> PlaceOrderAsync(OrderPlaceDto dto);
    Task<OrderDto?> GetByIdAsync(int orderId);
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
    Task<bool> UpdateAsync(int orderId, OrderUpdateDto dto);
    Task<bool> DeleteAsync(int orderId);
}

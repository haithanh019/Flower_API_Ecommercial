using DataAccess.DTOs.OrderDTOs;

namespace Services.Interfaces;

public interface IOrderService
{
    Task<int> PlaceOrderAsync(OrderPlaceDto dto);
    Task<OrderDto?> GetByIdAsync(int orderId);
}

using System.Security.Claims;
using DataAccess.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.FacadeService;

namespace API.Controllers
{
    [Authorize]
    public class OrdersController : ODataController
    {
        private readonly IFacadeService _facade;

        public OrdersController(IFacadeService facade) => _facade = facade;

        // GET /odata/Orders
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Forbid();

                var customerOrders = await _facade.OrderService.GetOrdersByCustomerIdAsync(userId);
                return Ok(customerOrders);
            }

            var data = await _facade.OrderService.GetAllAsync();
            return Ok(data);
        }

        // GET /odata/Orders(1)
        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            var dto = await _facade.OrderService.GetByIdAsync(key);
            if (dto is null)
                return NotFound();

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Forbid();

                if (dto.CustomerId != userId)
                    return Forbid();
            }

            return Ok(dto);
        }

        // POST /odata/Orders
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Post([FromBody] OrderPlaceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            // Customer không được đặt hộ người khác
            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Forbid();
                dto.CustomerId = userId;
            }

            var id = await _facade.OrderService.PlaceOrderAsync(dto);
            var created = await _facade.OrderService.GetByIdAsync(id);
            return Created(created);
        }

        // PUT /odata/Orders(1) - Chỉ cho phép Admin cập nhật trạng thái
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] OrderUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _facade.OrderService.UpdateAsync(key, dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /odata/Orders(1) - Chỉ cho phép Admin xóa (hoặc cancel)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var ok = await _facade.OrderService.DeleteAsync(key);
            return ok ? NoContent() : NotFound();
        }
    }
}

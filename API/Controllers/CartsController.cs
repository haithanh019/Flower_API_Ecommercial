using System.Security.Claims;
using DataAccess.DTOs.CartDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.FacadeService;

namespace API.Controllers
{
    [Authorize]
    public class CartsController : ODataController
    {
        private readonly IFacadeService _facade;

        public CartsController(IFacadeService facade) => _facade = facade;

        // GET /odata/Carts - Lấy cart của user hiện tại
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            var cart = await _facade.CartService.GetOrCreateAsync(userId);
            return Ok(new[] { cart }); // Trả về array với 1 element để tương thích OData
        }

        // GET /odata/Carts(1) - Không dùng vì mỗi user chỉ có 1 cart
        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            return await Task.FromResult(BadRequest("Use GET /odata/Carts to get your cart"));
        }

        // POST /odata/Carts - Add item to cart
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CartAddItemRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            var cart = await _facade.CartService.AddItemAsync(userId, dto.ProductId, dto.Quantity);
            return Created(cart);
        }

        // PUT /odata/Carts(1) - Update item quantity
        [HttpPut]
        public async Task<IActionResult> Put(
            [FromODataUri] int key,
            [FromBody] CartUpdateQtyRequest dto
        )
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            var cart = await _facade.CartService.UpdateItemQtyAsync(
                userId,
                dto.CartItemId,
                dto.Quantity
            );
            return Ok(cart);
        }

        // DELETE /odata/Carts(1) - Clear cart
        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            var cart = await _facade.CartService.ClearAsync(userId);
            return NoContent();
        }
    }
}

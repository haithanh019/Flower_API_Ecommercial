using System.Security.Claims;
using DataAccess.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.FacadeService;

namespace API.Controllers
{
    [Authorize]
    public class PaymentsController : ODataController
    {
        private readonly IFacadeService _facade;

        public PaymentsController(IFacadeService facade) => _facade = facade;

        // GET /odata/Payments
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Forbid();

                var customerPayments = await _facade.PaymentService.GetByCustomerIdAsync(userId);
                return Ok(customerPayments);
            }

            var data = await _facade.PaymentService.GetAllAsync();
            return Ok(data);
        }

        // GET /odata/Payments(1)
        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            var dto = await _facade.PaymentService.GetByIdAsync(key);
            if (dto is null)
                return NotFound();

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Forbid();

                var hasAccess = await _facade.PaymentService.CanCustomerAccessPaymentAsync(
                    key,
                    userId
                );
                if (!hasAccess)
                    return Forbid();
            }

            return Ok(dto);
        }

        // POST /odata/Payments
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] PaymentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _facade.PaymentService.CreateAsync(dto);
            var created = await _facade.PaymentService.GetByIdAsync(id);
            return Created(created);
        }

        // PUT /odata/Payments(1)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(
            [FromODataUri] int key,
            [FromBody] PaymentUpdateDto dto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.PaymentId = key;
            var ok = await _facade.PaymentService.UpdateAsync(dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /odata/Payments(1)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var ok = await _facade.PaymentService.DeleteAsync(key);
            return ok ? NoContent() : NotFound();
        }
    }
}

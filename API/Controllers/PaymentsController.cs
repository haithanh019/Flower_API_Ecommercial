using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.FacadeService;

namespace API.Controllers
{
    [Authorize]
    public class PaymentsController : ODataController
    {
        private readonly IFacadeService _facade;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PaymentsController(IFacadeService facade, IUnitOfWork uow, IMapper mapper)
        {
            _facade = facade;
            _uow = uow;
            _mapper = mapper;
        }

        // GET /odata/Payments
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Forbid();
                var userId = int.Parse(userIdStr);

                var mine = await _uow
                    .PaymentRepository.Query()
                    .Where(p => p.Order.CustomerId == userId)
                    .ProjectTo<PaymentDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(mine);
            }

            var all = await _uow
                .PaymentRepository.Query()
                .ProjectTo<PaymentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(all);
        }

        // GET /odata/Payments(1)
        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Forbid();
                var userId = int.Parse(userIdStr);

                var dto = await _uow
                    .PaymentRepository.Query()
                    .Where(p => p.PaymentId == key && p.Order.CustomerId == userId)
                    .ProjectTo<PaymentDto>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();

                return dto is null ? NotFound() : Ok(dto);
            }

            var adminDto = await _facade.PaymentService.GetByIdAsync(key);
            return adminDto is null ? NotFound() : Ok(adminDto);
        }

        // POST /odata/Payments
        // (OrderService đã tạo Payment Pending khi đặt hàng;
        // cho phép Admin tạo thủ công nếu cần)
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
            var entity = await _uow.PaymentRepository.GetByIdAsync(key);
            if (entity == null)
                return NotFound();
            await _uow.PaymentRepository.DeleteAsync(entity);
            return NoContent();
        }
    }
}

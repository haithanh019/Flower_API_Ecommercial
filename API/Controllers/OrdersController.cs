using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess.DTOs.OrderDTOs;
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
    [Authorize] // có thể đổi thành [Authorize(Roles = "Customer,Staff,Admin")] nếu muốn
    public class OrdersController : ODataController
    {
        private readonly IFacadeService _facade;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrdersController(IFacadeService facade, IUnitOfWork uow, IMapper mapper)
        {
            _facade = facade;
            _uow = uow;
            _mapper = mapper;
        }

        // GET /odata/Orders
        // Trả IQueryable<ProjectTo<OrderDto>> để OData xử lý server-side
        [EnableQuery]
        public IActionResult Get()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var q = _uow.OrderRepository.Query(); // IQueryable<Order>

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Forbid();

                q = q.Where(o => o.CustomerId == userId);
            }

            var projected = q.ProjectTo<OrderDto>(_mapper.ConfigurationProvider);
            return Ok(projected); // KHÔNG ToListAsync -> để OData áp dụng filter/paging
        }

        // GET /odata/Orders(1)
        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            var dto = await _uow
                .OrderRepository.Query()
                .Where(o => o.OrderId == key)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

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
        // Nếu Staff cũng được đặt hộ KH, thêm "Staff" vào Roles.
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
    }
}

using DataAccess.DTOs.ProductDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.FacadeService;

namespace API.Controllers
{
    [Authorize]
    public class ProductsController : ODataController
    {
        private readonly IFacadeService _facade;

        public ProductsController(IFacadeService facade) => _facade = facade;

        // GET /odata/Products
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var data = await _facade.ProductService.GetAllAsync();
            return Ok(data);
        }

        // GET /odata/Products(1)
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            var dto = await _facade.ProductService.GetByIdAsync(key);
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /odata/Products
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var id = await _facade.ProductService.CreateAsync(dto);
            var created = await _facade.ProductService.GetByIdAsync(id);
            return Created(created);
        }

        // PUT /odata/Products(1)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(
            [FromODataUri] int key,
            [FromBody] ProductUpdateDto dto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var ok = await _facade.ProductService.UpdateAsync(key, dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /odata/Products(1)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var ok = await _facade.ProductService.DeleteAsync(key);
            return ok ? NoContent() : NotFound();
        }
    }
}

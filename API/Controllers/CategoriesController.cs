using DataAccess.DTOs.CategoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.FacadeService;

namespace API.Controllers
{
    [Authorize]
    public class CategoriesController : ODataController
    {
        private readonly IFacadeService _facade;

        public CategoriesController(IFacadeService facade) => _facade = facade;

        // GET /odata/Categories
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var data = await _facade.CategoryService.GetAllAsync();
            return Ok(data);
        }

        // GET /odata/Categories(1)
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromODataUri] int key)
        {
            var dto = await _facade.CategoryService.GetByIdAsync(key);
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /odata/Categories
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var id = await _facade.CategoryService.CreateAsync(dto);
            var created = await _facade.CategoryService.GetByIdAsync(id);
            return Created(created);
        }

        // PUT /odata/Categories(1)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(
            [FromODataUri] int key,
            [FromBody] CategoryUpdateDto dto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var ok = await _facade.CategoryService.UpdateAsync(key, dto);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /odata/Categories(1)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var ok = await _facade.CategoryService.DeleteAsync(key);
            return ok ? NoContent() : NotFound();
        }
    }
}

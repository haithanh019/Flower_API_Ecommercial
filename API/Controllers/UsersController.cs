using DataAccess.Auth;
using DataAccess.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.FacadeService;
using Services.Interfaces;

namespace API.Controllers
{
    public class UsersController : ODataController
    {
        private readonly IFacadeService _facadeService;
        private readonly IConfiguration _config;

        public UsersController(IFacadeService facadeService, IConfiguration config)
        {
            _facadeService = facadeService;
            _config = config;
        }

        // POST /odata/Users
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] LoginDTO login)
        {
            if (login is null)
                return BadRequest("Login body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userDto = await _facadeService.UserService.LoginAsync(login);
            if (userDto == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // JwtHelper của bạn nhận role kiểu int
            var token = JwtHelper.GenerateJwtToken(
                userDto.UserId,
                userDto.Email,
                userDto.Role,
                _config
            );

            return Ok(
                new
                {
                    token,
                    user = new
                    {
                        userDto.UserId,
                        userDto.FullName,
                        userDto.Email,
                        userDto.Role,
                    },
                }
            );
        }
    }
}

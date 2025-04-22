using Application.DTOs.UserDTO;
using Application.Interfaces.Services;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/user
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ResponseUserDTO>>> GetUsers([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetPaginatedUsers(pageIndex, pageSize);
            return Ok(result);
        }
    }
}

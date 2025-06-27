using Application.DTOs.SearchFilters;
using Application.DTOs.UserDTO;
using Application.Interfaces.Services;
using Application.Paggings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for managing user-related operations.  
    /// </summary>  
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="UserController"/> class.  
        /// </summary>  
        /// <param name="userService">The user service to handle user operations.</param>  
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>  
        /// Retrieves a paginated list of users based on the provided filters.  
        /// </summary>
        /// <param name="filter"></param>  
        /// <param name="pageIndex">The index of the page to retrieve.</param>  
        /// <param name="pageSize">The number of items per page.</param>  
        /// <returns>A paginated list of users.</returns>  
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ResponseUserDTO>>> GetUsers(
            [FromQuery] UserSearchFilterDTO filter,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest("pageIndex and pageSize must be greater than 0.");
            }

            var result = await _userService.GetPaginatedUsers(pageIndex, pageSize, filter);
            return Ok(result);
        }


        /// <summary>  
        /// Retrieves a paginated list of nearby artists.  
        /// </summary>  
        /// <param name="pageIndex">The index of the page to retrieve.</param>  
        /// <param name="pageSize">The number of items per page.</param>  
        /// <returns>A paginated list of nearby artists.</returns>  
        [HttpGet("nearby-artists")]
        [Authorize(AuthenticationSchemes = "Jwt")]
        public async Task<ActionResult<PaginatedList<ResponseUserDTO>>> GetNearbyArtists(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetNearbyArtistsAsync(pageIndex, pageSize);
            return Ok(result);
        }




        /// <summary>  
        /// Retrieves a full list of all users without pagination.  
        /// </summary>  
        /// <returns>A list of all users.</returns>  
        [HttpGet("all")]
        public async Task<ActionResult<List<ResponseUserDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>  
        /// Retrieves a summary of users created grouped by date.  
        /// </summary>  
        /// <returns>A list of created summaries containing date and count.</returns>  
        [HttpGet("created-summary")]
        public async Task<IActionResult> GetUsersCreatedSummary()
        {
            var data = await _userService.GetUsersCreatedSummaryAsync();
            return Ok(data);
        }


        /// <summary>  
        /// Retrieves a user by their unique identifier.  
        /// </summary>  
        /// <param name="id">The unique identifier of the user.</param>  
        /// <returns>The user details if found; otherwise, a NotFound result.</returns>  
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseUserDTO>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = $"User with ID {id} was not found." });

            return Ok(user);
        }
        /// <summary>  
        /// Updates a user's information by their ID.  
        /// </summary>  
        /// <param name="id">The ID of the user to update.</param>  
        /// <param name="dto">The updated user data.</param>  
        /// <returns>NoContent if successful; otherwise, NotFound or BadRequest.</returns>  
        [HttpPut("{id:guid}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDTO dto)
        {
            var updated = await _userService.UpdateUserAsync(id, dto);
            if (!updated)
                return NotFound(new { Message = $"User with ID {id} was not found." });

            return NoContent();
        }

        /// <summary>  
        /// Soft deletes a user by their ID.  
        /// </summary>  
        /// <param name="id">The ID of the user to delete.</param>  
        /// <returns>NoContent if successful; otherwise, NotFound.</returns>  
        [HttpDelete("{id:guid}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
                return NotFound(new { Message = $"User with ID {id} was not found or already deleted." });

            return NoContent();
        }

        /// <summary>
        /// Retrieves a paginated list of unapproved artists.
        /// </summary>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of unapproved artists.</returns>
        [HttpGet("unapproved-artists")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<ActionResult<PaginatedList<ResponseUserDTO>>> GetUnapprovedArtists(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest("pageIndex and pageSize must be greater than 0.");
            }

            var result = await _userService.GetUnapprovedArtistsAsync(pageIndex, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Approves an artist by their user ID.
        /// </summary>
        /// <param name="id">The user ID of the artist to approve.</param>
        /// <returns>NoContent if successful; otherwise, NotFound.</returns>
        [HttpPut("approve-artist/{id:guid}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> ApproveArtist(Guid id)
        {
            var success = await _userService.ApproveArtistAsync(id);
            if (!success)
                return NotFound(new { message = $"Artist with ID {id} was not found or already approved." });

            return NoContent();
        }

        /// <summary>
        /// Allows a customer to rate an artist they have completed a booking with.
        /// </summary>
        /// <param name="artistId">The ID of the artist to rate.</param>
        /// <param name="rating">The rating value (0.0 - 5.0).</param>
        /// <returns>NoContent if successful.</returns>
        [HttpPost("rate/{artistId:guid}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "User")]
        public async Task<IActionResult> RateArtist(Guid artistId, [FromQuery] double rating)
        {
            if (rating < 0 || rating > 5)
                return BadRequest("Rating must be between 0 and 5.");

            await _userService.RateArtistAsync(artistId, rating);
            return NoContent();
        }


    }
}

using Application.DTOs.ServiceDTO;
using Application.Interfaces.Services;
using Application.Paggings;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for managing services.  
    /// </summary>  
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IUserService _userService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="ServiceController"/> class.  
        /// </summary>  
        /// <param name="serviceService">The service service instance.</param>  
        /// <param name="userService">The user service instance.</param>  
        public ServiceController(IServiceService serviceService, IUserService userService)
        {
            _serviceService = serviceService;
            _userService = userService;
        }



        /// <summary>  
        /// Retrieves all services with pagination and optional filtering by service name, price, and status.  
        /// </summary>  
        /// <param name="pageIndex">The index of the page to retrieve.</param>  
        /// <param name="pageSize">The number of items per page.</param>  
        /// <param name="serviceName">Optional filter for service name.</param>  
        /// <param name="price">Optional filter for service price.</param>  
        /// <param name="status">Optional filter for service status (Available, Unavailable, Discontinued).</param>
        /// <param name="artistId"></param>  
        /// <returns>A paginated list of services.</returns>  
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ResponseServiceDTO>>> GetServices(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? serviceName = null,
            [FromQuery] decimal? price = null,
            [FromQuery] ServiceStatus? status = null,
            [FromQuery] Guid? artistId = null)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _serviceService.GetServicesAsync(pageIndex, pageSize, serviceName, price, status, artistId);
            return Ok(result);
        }

        /// <summary>  
        /// Retrieves all services for an artist with pagination and optional filtering by service name, price, and status.  
        /// </summary>  
        /// <param name="pageIndex">The index of the page to retrieve.</param>  
        /// <param name="pageSize">The number of items per page.</param>  
        /// <param name="serviceName">Optional filter for service name.</param>  
        /// <param name="price">Optional filter for service price.</param>  
        /// <param name="status">Optional filter for service status (Available, Unavailable, Discontinued).</param>  
        /// <returns>A paginated list of services for the artist.</returns>  
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Artist")]
        [HttpGet("by-artist")]
        public async Task<ActionResult<PaginatedList<ResponseServiceDTO>>> GetServicesForArtist(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? serviceName = null,
            [FromQuery] decimal? price = null,
            [FromQuery] ServiceStatus? status = null)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _serviceService.GetServicesForArtistAsync(pageIndex, pageSize, serviceName, price, status);
            return Ok(result);
        }

        ///// <summary>  
        ///// Retrieves all services for a specific artist with pagination.  
        ///// </summary>  
        ///// <param name="pageIndex">The index of the page to retrieve.</param>  
        ///// <param name="pageSize">The number of items per page.</param>  
        ///// <param name="artistId">The artist ID to filter services by.</param>  
        ///// <returns>A paginated list of services for the artist.</returns>  
        //[HttpGet("by-artist/{artistId}")]
        //public async Task<ActionResult<PaginatedList<ResponseServiceDTO>>> GetAllServicesByArtistId(
        //    Guid artistId,
        //    [FromQuery] int pageIndex = 1,
        //    [FromQuery] int pageSize = 10
        //    )
        //{
        //    if (pageIndex <= 0 || pageSize <= 0)
        //        return BadRequest("Page index and size must be greater than 0.");

        //    var result = await _serviceService.GetAllServicesByArtistIdAsync(pageIndex, pageSize, artistId);
        //    return Ok(result);
        //}

        /// <summary>
        /// Creates a new service.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseServiceDTO>> Create([FromBody] CreateServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _serviceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetServices), new { id = created.Id }, created);
        }

        /// <summary>  
        /// Creates a new service for an artist.  
        /// </summary>  
        /// <param name="dto">The data transfer object containing service details.</param>  
        /// <returns>The created service details.</returns>  
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Artist")]
        [HttpPost("by-artist")]
        public async Task<ActionResult<ResponseServiceDTO>> CreateForArtist([FromBody] CreateServiceForArtistDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _serviceService.CreateServiceForArtistAsync(dto);
            return CreatedAtAction(nameof(GetServices), new { id = created.Id }, created);
        }

        /// <summary>  
        /// Updates an existing service for an artist.  
        /// </summary>  
        /// <param name="id">The ID of the service to update.</param>  
        /// <param name="dto">The data transfer object containing updated service details.</param>  
        /// <returns>No content if the update is successful.</returns>  
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Artist")]
        [HttpPut("by-artist/{id}")]
        public async Task<IActionResult> UpdateServiceForArtist(Guid id, [FromBody] UpdateServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("ID in the URL must match ID in the body.");

            await _serviceService.UpdateServiceForArtistAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Updates an existing service.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("ID in the URL must match ID in the body.");

            await _serviceService.UpdateAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a service by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _serviceService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>  
        /// Deletes a service for an artist by ID.  
        /// </summary>  
        /// <param name="id">The ID of the service to delete.</param>  
        /// <returns>No content if the deletion is successful.</returns>  
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Artist")]
        [HttpDelete("by-artist/{id}")]
        public async Task<IActionResult> DeleteServiceForArtist(Guid id)
        {
            await _serviceService.DeleteForArtistAsync(id);
            return NoContent();
        }
    }
}

using Application.DTOs.ServiceDTO;
using Application.Interfaces.Services;
using Application.Paggings;
using Domain.Entities;
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

        /// <summary>  
        /// Initializes a new instance of the <see cref="ServiceController"/> class.  
        /// </summary>  
        /// <param name="serviceService">The service for managing services.</param>  
        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        /// <summary>  
        /// Retrieves all services with pagination and optional filtering by service name, price, and status.  
        /// </summary>  
        /// <param name="pageIndex">The index of the page to retrieve.</param>  
        /// <param name="pageSize">The number of items per page.</param>  
        /// <param name="serviceName">Optional filter for service name.</param>  
        /// <param name="price">Optional filter for service price.</param>  
        /// <param name="status">Optional filter for service status (Available, Unavailable, Discontinued).</param>  
        /// <returns>A paginated list of services.</returns>  
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ResponseServiceDTO>>> GetAllServices(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? serviceName = null,
            [FromQuery] decimal? price = null,
            [FromQuery] ServiceStatus? status = null)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _serviceService.GetAllServicesAsync(pageIndex, pageSize, serviceName, price, status);
            return Ok(result);
        }
        /// <summary>  
        /// Retrieves all services for a specific artist with pagination.  
        /// </summary>  
        /// <param name="pageIndex">The index of the page to retrieve.</param>  
        /// <param name="pageSize">The number of items per page.</param>  
        /// <param name="artistId">The artist ID to filter services by.</param>  
        /// <returns>A paginated list of services for the artist.</returns>  
        [HttpGet("by-artist/{artistId}")]
        public async Task<ActionResult<PaginatedList<ResponseServiceDTO>>> GetAllServicesByArtistId(
            Guid artistId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10
            )
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var result = await _serviceService.GetAllServicesByArtistIdAsync(pageIndex, pageSize, artistId);
            return Ok(result);
        }
    }
}

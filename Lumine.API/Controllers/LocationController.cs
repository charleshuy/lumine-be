using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>
    /// Controller for managing location-related operations like provinces and districts.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationController"/> class.
        /// </summary>
        /// <param name="locationService">The location service to handle operations.</param>
        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Retrieves all provinces.
        /// </summary>
        /// <returns>A list of provinces.</returns>
        [HttpGet("provinces")]
        public async Task<ActionResult<IEnumerable<ProvinceDto>>> GetAllProvinces()
        {
            var provinces = await _locationService.GetAllProvincesAsync();
            return Ok(provinces);
        }

        /// <summary>
        /// Retrieves all districts belonging to a specific province.
        /// </summary>
        /// <param name="provinceId">The unique identifier of the province.</param>
        /// <returns>A list of districts.</returns>
        [HttpGet("provinces/{provinceId:guid}/districts")]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetDistrictsByProvinceId(Guid provinceId)
        {
            var districts = await _locationService.GetDistrictsByProvinceIdAsync(provinceId);
            return Ok(districts);
        }
    }
}

using Application.DTOs.ServiceDTO;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for managing service types.  
    /// </summary>  
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="ServiceTypeController"/> class.  
        /// </summary>  
        /// <param name="serviceTypeService">The service type service.</param>  
        public ServiceTypeController(IServiceTypeService serviceTypeService)
        {
            _serviceTypeService = serviceTypeService;
        }

        /// <summary>  
        /// Gets all service types.  
        /// </summary>  
        /// <returns>A list of service types.</returns>  
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceTypeDTO>>> GetAll()
        {
            var types = await _serviceTypeService.GetAllAsync();
            return Ok(types);
        }

        /// <summary>  
        /// Gets a service type by its ID.  
        /// </summary>  
        /// <param name="id">The ID of the service type.</param>  
        /// <returns>The service type with the specified ID.</returns>  
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ServiceTypeDTO>> GetById(Guid id)
        {
            var result = await _serviceTypeService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>  
        /// Creates a new service type.  
        /// </summary>  
        /// <param name="dto">The service type to create.</param>  
        /// <returns>The created service type.</returns>  
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<ActionResult<ServiceTypeDTO>> Create([FromBody] CreateServiceTypeDTO dto)
        {
            var created = await _serviceTypeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>  
        /// Updates an existing service type.  
        /// </summary>  
        /// <param name="id">The ID of the service type to update.</param>  
        /// <param name="dto">The updated service type data.</param>  
        /// <returns>No content if the update is successful.</returns>  
        [HttpPut("{id:guid}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceTypeDTO dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch between route and payload.");

            await _serviceTypeService.UpdateAsync(dto);
            return NoContent();
        }

        /// <summary>  
        /// Deletes a service type by its ID.  
        /// </summary>  
        /// <param name="id">The ID of the service type to delete.</param>  
        /// <returns>No content if the deletion is successful.</returns>  
        [HttpDelete("{id:guid}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _serviceTypeService.DeleteAsync(id);
            return NoContent();
        }
    }
}

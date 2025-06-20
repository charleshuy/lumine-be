using Application.DTOs.UserDTO;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for managing roles.  
    /// </summary>  
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="RoleController"/> class.  
        /// </summary>  
        /// <param name="roleService">The role service.</param>  
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>  
        /// Retrieves all roles.  
        /// </summary>  
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>  
        /// Retrieves a specific role by name.  
        /// </summary>  
        /// <param name="roleName">The name of the role.</param>  
        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            var role = await _roleService.GetRoleByNameAsync(roleName);
            if (role == null)
                return NotFound(new { Message = $"Role '{roleName}' not found." });

            return Ok(role);
        }

        /// <summary>  
        /// Creates a new role.  
        /// </summary>  
        /// <param name="dto">The role data transfer object.</param>  
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDTO dto)
        {
            var result = await _roleService.CreateRoleAsync(dto);
            return Ok(new { Message = "Role created successfully." });
        }

        /// <summary>  
        /// Updates an existing role name.  
        /// </summary>  
        /// <param name="roleName">The current name of the role.</param>  
        /// <param name="dto">The new role data.</param>  
        /// <returns>Returns a success message or NotFound.</returns>  
        [HttpPut("{roleName}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(string roleName, [FromBody] RoleDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { Message = "New role name cannot be empty." });

            var updated = await _roleService.UpdateRoleNameAsync(roleName, dto.Name!);
            if (!updated)
                return NotFound(new { Message = $"Role '{roleName}' not found." });

            return Ok(new { Message = "Role updated successfully." });
        }

        /// <summary>  
        /// Deletes a role by name.  
        /// </summary>  
        /// <param name="roleName">The name of the role to delete.</param>  
        [HttpDelete("{roleName}")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var deleted = await _roleService.DeleteRoleAsync(roleName);
            if (!deleted)
                return NotFound(new { Message = $"Role '{roleName}' not found." });

            return Ok(new { Message = "Role deleted successfully." });
        }
    }
}

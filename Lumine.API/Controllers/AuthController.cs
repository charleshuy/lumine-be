using Application.DTOs.Auth;
using Application.DTOs.UserDTO;
using Application.Interfaces.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for handling authentication-related operations.  
    /// </summary>  
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IUserService _userService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="AuthController"/> class.  
        /// </summary>  
        /// <param name="firebaseAuthService">Service for Firebase authentication.</param>  
        /// <param name="userService">Service for user-related operations.</param>  
        public AuthController(IFirebaseAuthService firebaseAuthService, IUserService userService)
        {
            _firebaseAuthService = firebaseAuthService;
            _userService = userService;
        }

        /// <summary>
        /// Logs in an admin using email and password.
        /// </summary>
        /// <param name="request">The login request containing email and password.</param>
        /// <returns>A JWT token if login is successful.</returns>
        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            var jwtToken = await _firebaseAuthService.SignInWithEmailPasswordAsync(request.Email, request.Password);
            return Ok(new { token = jwtToken });
        }


        /// <summary>  
        /// Logs in a user using Firebase authentication.  
        /// </summary>  
        /// <param name="request">The Firebase login request containing the ID token and optional FCM token.</param>  
        /// <returns>A JWT token if the login is successful.</returns>  
        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest(new { message = "ID token is required." });
            }

            var jwtToken = await _firebaseAuthService.SignInWithFirebaseAsync(request.IdToken, request.fmcToken);
            return Ok(new { token = jwtToken });
        }

        /// <summary>  
        /// Retrieves the profile of the currently authenticated user.  
        /// </summary>  
        /// <returns>The profile of the current user.</returns>  
        [Authorize(AuthenticationSchemes = "Jwt")]
        [HttpGet("profile")]
        public async Task<ActionResult<ResponseUserDTO>> GetCurrentUser()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User is not authenticated." });

            return Ok(user);
        }

        /// <summary>
        /// Updates the profile of the currently authenticated user.
        /// </summary>
        /// <param name="dto">The profile data to update.</param>
        /// <returns>Status of the update operation.</returns>
        [Authorize(AuthenticationSchemes = "Jwt")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var success = await _userService.UpdateCurrentUserProfileAsync(dto);
            if (!success)
                return Unauthorized(new { message = "User is not authenticated or does not exist." });

            return NoContent();
        }

    }
}

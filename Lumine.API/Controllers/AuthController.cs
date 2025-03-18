using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IFirebaseAuthService _firebaseAuthService;

        public AuthController(IFirebaseAuthService firebaseAuthService)
        {
            _firebaseAuthService = firebaseAuthService;
        }

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
    }
}

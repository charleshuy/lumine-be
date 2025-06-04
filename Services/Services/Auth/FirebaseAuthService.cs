using Application.Interfaces.Auth;
using Domain.Base;
using Domain.Entities;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Auth
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken)
        {
            // ✅ Verify Firebase Token
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string firebaseUserId = decodedToken.Uid;
            string? email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
            string? firstName = decodedToken.Claims.ContainsKey("given_name") ? decodedToken.Claims["given_name"].ToString() : "";
            string? lastName = decodedToken.Claims.ContainsKey("family_name") ? decodedToken.Claims["family_name"].ToString() : "";
            string? username = decodedToken.Claims.ContainsKey("nickname") ? decodedToken.Claims["nickname"].ToString() : email?.Split('@')[0];

            if (string.IsNullOrEmpty(email))
            {
                throw new BaseException.BadRequestException("missing_email", "Firebase token does not contain an email.");
            }

            // ✅ Check if the user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // ✅ Create a new user
                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    FcmToken = fcmToken
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new BaseException.BadRequestException("user_creation_failed",
                        $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                if (!string.IsNullOrEmpty(fcmToken))
                {
                    user.FcmToken = fcmToken;
                    await _userManager.UpdateAsync(user);
                }
            }

            return await GenerateJwtToken(user, _userManager);
        }

        public async Task<string> SignInWithEmailPasswordAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new BaseException.UnauthorizedException("invalid_credentials", "Invalid email or password.");
            }

            // Optional: Ensure the user has the "Admin" role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                throw new BaseException.UnauthorizedException("access_denied", "User is not authorized as admin.");
            }

            return await GenerateJwtToken(user, _userManager);
        }


        private async Task<string> GenerateJwtToken(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new BaseException.NotFoundException("not_found", "Jwt key not found")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? "")
            };

            // ✅ Fetch roles from Identity and add them as claims
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

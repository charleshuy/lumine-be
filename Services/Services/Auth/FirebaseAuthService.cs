using Application.DTOs.Auth;
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

            if (user == null || !await _userManager.CheckPasswordAsync(user, password) || !user.IsActive)
            {
                throw new BaseException.UnauthorizedException("invalid_credentials", "Invalid email or password.");
            }

            // Optional: Ensure the user has the "Admin" role
            //if (!await _userManager.IsInRoleAsync(user, "Admin"))
            //{
            //    throw new BaseException.UnauthorizedException("access_denied", "User is not authorized as admin.");
            //}

            return await GenerateJwtToken(user, _userManager);
        }

        public async Task RegisterWithEmailPasswordFireBaseAsync(RegisterEmailRequest request)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var signUpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";

            var payload = new
            {
                email = request.Email,
                password = request.Password,
                returnSecureToken = true
            };

            using var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(signUpUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (responseContent.Contains("EMAIL_EXISTS"))
                {
                    throw new BaseException.BadRequestException("email_exists", "Email đã được sử dụng.");
                }

                throw new BaseException.BadRequestException("registration_failed", "Đăng ký không thành công. Vui lòng thử lại.");
            }

            var responseData = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var idToken = responseData.RootElement.GetProperty("idToken").GetString();

            // ✅ Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new BaseException.BadRequestException("user_exists", "Tài khoản đã tồn tại.");

            // ✅ Create ApplicationUser
            var newUser = new ApplicationUser
            {
                UserName = request.Email.Split('@')[0],
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                FirstName = request.FullName, // You can split into First/Last if needed
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BaseException.BadRequestException("create_user_failed", $"Không thể tạo người dùng: {errors}");
            }

            await _userManager.AddToRoleAsync(newUser, "User");

            // ✅ Send Email Verification via Firebase
            await SendEmailVerificationAsync(idToken);
        }

        public async Task RegisterArtistWithEmailPasswordFireBaseAsync(RegisterEmailRequest request)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var signUpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";

            var payload = new
            {
                email = request.Email,
                password = request.Password,
                returnSecureToken = true
            };

            using var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(signUpUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (responseContent.Contains("EMAIL_EXISTS"))
                {
                    throw new BaseException.BadRequestException("email_exists", "Email đã được sử dụng.");
                }

                throw new BaseException.BadRequestException("registration_failed", "Đăng ký không thành công. Vui lòng thử lại.");
            }


            var responseData = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var idToken = responseData.RootElement.GetProperty("idToken").GetString();

            // ✅ Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new BaseException.BadRequestException("user_exists", "Tài khoản đã tồn tại.");

            // ✅ Create ApplicationUser
            var newUser = new ApplicationUser
            {
                UserName = request.Email.Split('@')[0],
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                FirstName = request.FullName, // You can split into First/Last if needed
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                isApproved = false,
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BaseException.BadRequestException("create_user_failed", $"Không thể tạo người dùng: {errors}");
            }

            await _userManager.AddToRoleAsync(newUser, "Artist");

            // ✅ Send Email Verification via Firebase
            await SendEmailVerificationAsync(idToken);
        }


        private async Task SendEmailVerificationAsync(string idToken)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var verificationUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";

            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken = idToken
            };

            var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(verificationUrl, content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = System.Text.Json.JsonDocument.Parse(responseBody);
                var message = error.RootElement.GetProperty("error").GetProperty("message").GetString();
                throw new BaseException.CoreException("email_verification_failed", $"Email verification failed: {message}");
            }

            // Optional: log for success debug
            Console.WriteLine("Email verification sent successfully");
        }


        public async Task<string> LoginWithEmailPasswordFirebaseAsync(string email, string password, string? fcmToken = null)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var signInUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(signInUrl, content);

            if (!response.IsSuccessStatusCode)
                throw new BaseException.UnauthorizedException("firebase_login_failed", "Email hoặc mật khẩu không đúng.");

            var responseData = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var idToken = responseData.RootElement.GetProperty("idToken").GetString();
            var firebaseEmail = responseData.RootElement.GetProperty("email").GetString();

            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(firebaseEmail))
                throw new BaseException.UnauthorizedException("token_missing", "Không thể lấy token từ Firebase.");

            // 🔐 Validate against Identity
            var user = await _userManager.FindByEmailAsync(firebaseEmail);
            if (user == null || user.IsDeleted)
                throw new BaseException.UnauthorizedException("user_not_found", "Tài khoản không tồn tại.");

            if (!user.IsActive)
                throw new BaseException.UnauthorizedException("inactive_user", "Tài khoản của bạn chưa được kích hoạt.");

            if (!user.isApproved)
                throw new BaseException.UnauthorizedException("unapproved_user", "Tài khoản của bạn chưa được duyệt.");

            // ✅ Optional: Update FCM Token if provided
            if (!string.IsNullOrEmpty(fcmToken))
            {
                user.FcmToken = fcmToken;
                await _userManager.UpdateAsync(user);
            }

            // 🔁 Continue with Identity sync and return local JWT
            return await SignInWithFirebaseAsync(idToken, fcmToken);
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

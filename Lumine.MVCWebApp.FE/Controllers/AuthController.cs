using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.DTOs.Auth;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7216/api/";
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient();

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBaseUrl}auth/admin-login", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(request);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonDocument.Parse(responseContent);
            var token = responseData.RootElement.GetProperty("token").GetString();

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var role = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            var email = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "";

            var claims = jwtToken.Claims.ToList();
            claims.Add(new Claim("TokenString", token));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            Response.Cookies.Append("TokenString", token);
            Response.Cookies.Append("UserName", email);
            Response.Cookies.Append("Role", role ?? "");

            if (!string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Accounts");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("Role");
            Response.Cookies.Delete("TokenString");

            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Forbidden()
        {
            return View();
        }
    }
}

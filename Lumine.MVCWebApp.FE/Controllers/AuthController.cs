﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly string _apiBaseUrl;

        public AuthController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = apiSettings.Value.BaseUrl ?? "https://localhost:7216/";
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBaseUrl}/auth/admin-login", content);

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

            if (role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (role?.Equals("Artist", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("Profile", "Profile");
            }
            else // default to User
            {
                return RedirectToAction("Index", "UserHomePage");
            }

        }

        public IActionResult LoginUser() => View();

        [HttpPost]
        public async Task<IActionResult> LoginUser(AdminLoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBaseUrl}/auth/email-firebase-login", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorDoc = JsonDocument.Parse(responseContent);
                    if (errorDoc.RootElement.TryGetProperty("errorMessage", out var errorMessageProp))
                    {
                        string message = errorMessageProp.GetString() ?? "Login failed. Please try again.";
                        ModelState.AddModelError(string.Empty, message);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
                }

                return View(request);
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonDocument.Parse(tokenJson);
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

            if (role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (role?.Equals("Artist", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("Index", "Profile");
            }
            else // default to User
            {
                return RedirectToAction("Index", "UserHomePage");
            }
        }


        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterEmailRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBaseUrl}/auth/register-email", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorDoc = JsonDocument.Parse(errorContent);

                    if (errorDoc.RootElement.TryGetProperty("errors", out var errorsElement))
                    {
                        foreach (var prop in errorsElement.EnumerateObject())
                        {
                            foreach (var error in prop.Value.EnumerateArray())
                            {
                                ModelState.AddModelError(prop.Name, error.GetString() ?? "An error occurred.");
                            }
                        }
                    }
                    else if (errorDoc.RootElement.TryGetProperty("errorMessage", out var msgElement))
                    {
                        ModelState.AddModelError(string.Empty, msgElement.GetString() ?? "Registration failed.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }

                return View(request);
            }

            // ✅ Set success message
            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng xác minh email trước khi đăng nhập.";

            // ✅ Redirect to LoginUser
            return RedirectToAction("LoginUser", "Auth");
        }

        public async Task<IActionResult> Logout()
        {
            var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("Role");
            Response.Cookies.Delete("TokenString");

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                return RedirectToAction("LoginUser", "Auth");
            }
        }


        public IActionResult Forbidden()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegisterArtist() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterArtist(RegisterEmailRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiBaseUrl}/auth/register-artist-email", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorDoc = JsonDocument.Parse(errorContent);

                    if (errorDoc.RootElement.TryGetProperty("errors", out var errorsElement))
                    {
                        foreach (var prop in errorsElement.EnumerateObject())
                        {
                            foreach (var error in prop.Value.EnumerateArray())
                            {
                                ModelState.AddModelError(prop.Name, error.GetString() ?? "Đã xảy ra lỗi.");
                            }
                        }
                    }
                    else if (errorDoc.RootElement.TryGetProperty("errorMessage", out var msgElement))
                    {
                        ModelState.AddModelError(string.Empty, msgElement.GetString() ?? "Đăng ký thất bại.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Vui lòng thử lại.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Vui lòng thử lại.");
                }

                return View(request);
            }

            // ✅ Redirect to LoginUser with success message
            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng xác minh email trước khi đăng nhập.";
            return RedirectToAction("LoginUser", "Auth");
        }

        public IActionResult RegistOption()
        {
            return View();
        }


    }
}

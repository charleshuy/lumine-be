using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class AccountsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountsController> _logger;
        private readonly string _userApiBase;
        private readonly string _roleApiBase;

        public AccountsController(
            IHttpClientFactory httpClientFactory,
            ILogger<AccountsController> logger,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            var baseUrl = apiSettings.Value.BaseUrl.TrimEnd('/');
            _userApiBase = $"{baseUrl}/User";
            _roleApiBase = $"{baseUrl}/Role";
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? username, string? email, string? phoneNumber, string? role, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                // Fetch roles
                var rolesResponse = await _httpClient.GetAsync($"{_roleApiBase}");
                rolesResponse.EnsureSuccessStatusCode();

                var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
                var roleList = JsonConvert.DeserializeObject<List<RoleDTO>>(rolesContent);

                // Build query string
                var query = new List<string>
                {
                    $"pageIndex={pageIndex}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(username)) query.Add($"username={Uri.EscapeDataString(username)}");
                if (!string.IsNullOrWhiteSpace(email)) query.Add($"email={Uri.EscapeDataString(email)}");
                if (!string.IsNullOrWhiteSpace(phoneNumber)) query.Add($"phoneNumber={Uri.EscapeDataString(phoneNumber)}");
                if (!string.IsNullOrWhiteSpace(role)) query.Add($"role={Uri.EscapeDataString(role)}");

                var queryString = string.Join("&", query);
                var response = await _httpClient.GetAsync($"{_userApiBase}?{queryString}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedList<ResponseUserDTO>>(content);

                ViewBag.Roles = roleList ?? new List<RoleDTO>();
                ViewBag.SearchUsername = username;
                ViewBag.SearchEmail = email;
                ViewBag.SearchPhoneNumber = phoneNumber;
                ViewBag.SearchRole = role;
                ViewBag.PageIndex = pageIndex;
                ViewBag.TotalPages = result?.TotalPages ?? 1;
                ViewBag.PageSize = pageSize;

                return View(result?.Items ?? new List<ResponseUserDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users.");
                ViewBag.Roles = new List<RoleDTO>();
                return View(new List<ResponseUserDTO>());
            }
        }

        public async Task<IActionResult> All()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_roleApiBase}/all");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<ResponseUserDTO>>(json);

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch all users.");
                return View(new List<ResponseUserDTO>());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_userApiBase}/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ResponseUserDTO>(json);
            return View(user);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_userApiBase}/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ResponseUserDTO>(json);

            var updateDto = new UpdateUserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
                // Add other editable fields if needed
            };

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_userApiBase}/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMsg);
            return View(dto);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_userApiBase}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load user.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ResponseUserDTO>(json);
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_userApiBase}/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to delete user.";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}

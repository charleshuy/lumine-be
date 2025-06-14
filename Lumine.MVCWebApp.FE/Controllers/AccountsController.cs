using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class AccountsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountsController> _logger;
        private readonly string _apiBaseUrl = "https://localhost:7216/api/User"; // adjust if different

        public AccountsController(IHttpClientFactory httpClientFactory, ILogger<AccountsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        // GET: /Accounts
        public async Task<IActionResult> Index(string? username, string? email, string? phoneNumber, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var query = new List<string>
        {
            $"pageIndex={pageIndex}",
            $"pageSize={pageSize}"
        };

                if (!string.IsNullOrWhiteSpace(username))
                    query.Add($"username={Uri.EscapeDataString(username)}");

                if (!string.IsNullOrWhiteSpace(email))
                    query.Add($"email={Uri.EscapeDataString(email)}");

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                    query.Add($"phoneNumber={Uri.EscapeDataString(phoneNumber)}");

                var queryString = string.Join("&", query);
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}?{queryString}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedList<ResponseUserDTO>>(content);

                ViewBag.SearchUsername = username;
                ViewBag.SearchEmail = email;
                ViewBag.SearchPhoneNumber = phoneNumber;

                return View(result.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users.");
                return View(new List<ResponseUserDTO>());
            }
        }

        // GET: /Accounts/All
        public async Task<IActionResult> All()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/all");
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



        // GET: /Accounts/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ResponseUserDTO>(json);

            return View(user);
        }

        // GET: /Accounts/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ResponseUserDTO>(json);

            var updateDto = new UpdateUserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                // include other editable fields
            };

            return View(updateDto);
        }

        // POST: /Accounts/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMsg);

            return View(dto);
        }

        // GET: /Accounts/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
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
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

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

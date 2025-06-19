using Application.DTOs;
using Application.DTOs.ServiceDTO;
using Application.DTOs.UserDTO;
using Application.Paggings;
using Lumine.MVCWebApp.FE;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Lumine.MVCWebApp.FE.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProfileController> _logger;
        private readonly string _apiBaseUrl;

        public ProfileController(
            IHttpClientFactory httpClientFactory,
            ILogger<ProfileController> logger,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public async Task<IActionResult> Index(int bookingPage = 1, int servicePage = 1)
        {
            try
            {
                var token = Request.Cookies["TokenString"];
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Get profile
                var profileResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/auth/profile");
                if (!profileResponse.IsSuccessStatusCode)
                    return RedirectToAction("Login", "Auth");

                var profileJson = await profileResponse.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<ResponseUserDTO>(profileJson);

                if (profile == null)
                    return RedirectToAction("Login", "Auth");

                var roles = profile.Roles.Select(r => r.Name).ToList();

                if (roles.Contains("User"))
                {
                    var bookingsResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/Booking/customer?pageIndex={bookingPage}&pageSize=5");
                    if (bookingsResponse.IsSuccessStatusCode)
                    {
                        var bookingsJson = await bookingsResponse.Content.ReadAsStringAsync();
                        var bookings = JsonConvert.DeserializeObject<PaginatedList<BookingDTO>>(bookingsJson);
                        ViewBag.Bookings = bookings;
                    }
                }
                else if (roles.Contains("Artist"))
                {
                    var servicesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/Service/by-artist?pageIndex={servicePage}&pageSize=3");
                    if (servicesResponse.IsSuccessStatusCode)
                    {
                        var servicesJson = await servicesResponse.Content.ReadAsStringAsync();
                        var services = JsonConvert.DeserializeObject<PaginatedList<ResponseServiceDTO>>(servicesJson);
                        ViewBag.Services = services;
                    }
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load profile.");
                TempData["Error"] = "Không thể tải thông tin hồ sơ.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateProfileDTO dto)
        {
            try
            {
                var token = Request.Cookies["TokenString"];
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/auth/profile", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Cập nhật thất bại: {errorMsg}";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile update failed.");
                TempData["Error"] = "Đã xảy ra lỗi khi cập nhật thông tin.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var token = Request.Cookies["TokenString"];
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Booking/customer?pageIndex=1&pageSize=10");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không thể tải danh sách lịch hẹn.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var bookings = JsonConvert.DeserializeObject<PaginatedList<BookingDTO>>(json);
                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings.");
                TempData["Error"] = "Đã xảy ra lỗi khi tải lịch hẹn.";
                return RedirectToAction("Index");
            }
        }
    }
}

using Application.DTOs;
using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Lumine.MVCWebApp.FE.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7216/";

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["TokenString"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Fetch profile
            var response = await client.GetAsync("api/auth/profile");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Login", "Auth");

            var json = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ResponseUserDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch bookings
            var bookingsResponse = await client.GetAsync("api/Booking/customer?pageIndex=1&pageSize=5");
            if (bookingsResponse.IsSuccessStatusCode)
            {
                var bookingsJson = await bookingsResponse.Content.ReadAsStringAsync();
                var bookings = JsonSerializer.Deserialize<PaginatedList<BookingDTO>>(bookingsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewBag.Bookings = bookings;
            }

            return View(profile);
        }


        [HttpPost]
        public async Task<IActionResult> Update(UpdateProfileDTO dto)
        {
            var token = Request.Cookies["TokenString"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/auth/profile", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Cập nhật thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MyBookings()
        {
            var token = Request.Cookies["TokenString"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Booking/customer?pageIndex=1&pageSize=10");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải danh sách lịch hẹn.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var bookings = JsonSerializer.Deserialize<PaginatedList<BookingDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(bookings);
        }

    }
}

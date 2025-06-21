using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class ApprovesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApprovesController> _logger;
        private readonly string _apiBaseUrl;

        public ApprovesController(IHttpClientFactory httpClientFactory, ILogger<ApprovesController> logger, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiBaseUrl = $"{apiSettings.Value.BaseUrl}/User";
        }

        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var token = Request.Cookies["TokenString"];
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Bạn chưa đăng nhập.";
                    return RedirectToAction("Login", "Auth");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/unapproved-artists?pageIndex={pageIndex}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedList<ResponseUserDTO>>(content);

                ViewBag.PageIndex = pageIndex;
                ViewBag.TotalPages = result?.TotalPages ?? 1;

                return View(result?.Items ?? new List<ResponseUserDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unapproved artists.");
                TempData["Error"] = "Unable to fetch unapproved artists.";
                return View(new List<ResponseUserDTO>());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var token = Request.Cookies["TokenString"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Missing token");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/approve-artist/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to approve artist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving artist.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}

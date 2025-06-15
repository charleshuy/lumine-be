using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class ApprovesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApprovesController> _logger;
        private readonly string _apiBaseUrl = "https://localhost:7216/api/User"; // Adjust if needed

        public ApprovesController(IHttpClientFactory httpClientFactory, ILogger<ApprovesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
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

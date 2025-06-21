using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class UserHomePageController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserHomePageController> _logger;
        private readonly string _apiBaseUrl;

        public UserHomePageController(
            IHttpClientFactory httpClientFactory,
            ILogger<UserHomePageController> logger,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiBaseUrl = apiSettings.Value.BaseUrl?.TrimEnd('/') ?? "https://localhost:7216/api";
        }

        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 6)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/User/?pageIndex={pageIndex}&pageSize={pageSize}&Role=Artist");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch artists for UserHomePage. Status: {StatusCode}", response.StatusCode);
                    return View("Error");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedList<ResponseUserDTO>>(content);

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching artists for UserHomePage");
                return View("Error");
            }
        }
    }
}

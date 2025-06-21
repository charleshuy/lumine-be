using Application.DTOs.ServiceDTO;
using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Lumine.MVCWebApp.FE.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ArtistsController> _logger;
        private readonly string _apiBaseUrl;

        public ArtistsController(
            IHttpClientFactory httpClientFactory,
            ILogger<ArtistsController> logger,
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
                    _logger.LogWarning("Failed to fetch artists. Status: {StatusCode}", response.StatusCode);
                    return View("Error");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedList<ResponseUserDTO>>(content);

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching artists.");
                return View("Error");
            }
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            try
            {
                // Get artist info
                var artistResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/User/{id}");
                if (!artistResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch artist detail. Status: {StatusCode}", artistResponse.StatusCode);
                    return View("Error");
                }

                var artistContent = await artistResponse.Content.ReadAsStringAsync();
                var artist = JsonConvert.DeserializeObject<ResponseUserDTO>(artistContent);

                // Get artist's services
                var serviceResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/Service?pageIndex=1&pageSize=10&artistId={id}");
                var services = new List<ResponseServiceDTO>();

                if (serviceResponse.IsSuccessStatusCode)
                {
                    var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
                    var serviceResult = JsonConvert.DeserializeObject<PaginatedList<ResponseServiceDTO>>(serviceContent);
                    services = serviceResult?.Items.ToList() ?? new List<ResponseServiceDTO>();
                }


                ViewBag.Services = services;

                return View(artist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching artist detail or services.");
                return View("Error");
            }
        }


    }
}

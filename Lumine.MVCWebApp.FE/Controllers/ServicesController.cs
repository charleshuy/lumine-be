using Application.DTOs.ServiceDTO;
using Application.Paggings;
using Lumine.MVCWebApp.FE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Lumine.MVC.Controllers
{
    public class ServicesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServicesController> _logger;
        private readonly string _apiBaseUrl;

        public ServicesController(
            IHttpClientFactory httpClientFactory,
            ILogger<ServicesController> logger,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiBaseUrl = $"{apiSettings.Value.BaseUrl}/Service";
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}?pageIndex=1&pageSize=20");
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedList<ResponseServiceDTO>>(jsonString);

                return View(result.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services.");
                return View(new List<ResponseServiceDTO>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiBaseUrl, content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, error);
            return View(dto);
        }

        //public async Task<IActionResult> Edit(Guid id)
        //{
        //    var response = await _httpClient.GetAsync($"{_apiBaseUrlUser}?pageIndex=1&pageSize=100");
        //    var jsonString = await response.Content.ReadAsStringAsync();
        //    var data = JsonConvert.DeserializeObject<PaginatedList<ResponseServiceDTO>>(jsonString);
        //    var service = data.Items.FirstOrDefault(s => s.Id == id);

        //    if (service == null) return NotFound();

        //    var dto = new UpdateServiceDTO
        //    {
        //        Id = service.Id,
        //        ServiceName = service.ServiceName,
        //        Description = service.ServiceDescription,
        //        Price = service.Price,
        //        Duration = service.Duration,
        //        Status = service.Status,
        //        ArtistID = service.ArtistID,
        //        ServiceTypeID = service.ServiceTypeID
        //    };

        //    return View(dto);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateServiceDTO dto)
        {
            if (!ModelState.IsValid || id != dto.Id)
                return View(dto);

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, error);
            return View(dto);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

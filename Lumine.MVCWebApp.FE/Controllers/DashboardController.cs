using Application.DTOs;
using Lumine.MVCWebApp.FE.Controllers;
using Lumine.MVCWebApp.FE;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class DashboardController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountsController> _logger;
    private readonly string _apiBaseUrl;

    public DashboardController(
        IHttpClientFactory httpClientFactory,
        ILogger<AccountsController> logger,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _apiBaseUrl = apiSettings.Value.BaseUrl ?? "https://localhost:7216/";
    }

    public async Task<IActionResult> Index()
    {
        // Fetch overview data
        var overviewResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/Overview");
        AppOverviewDTO? overview = null;

        if (overviewResponse.IsSuccessStatusCode)
        {
            var jsonOverview = await overviewResponse.Content.ReadAsStringAsync();
            overview = JsonConvert.DeserializeObject<AppOverviewDTO>(jsonOverview);
        }

        // Fetch chart data
        var chartResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/Booking/status-summary");
        if (chartResponse.IsSuccessStatusCode)
        {
            var chartJson = await chartResponse.Content.ReadAsStringAsync();
            ViewBag.BookingChartData = chartJson;
        }
        else
        {
            ViewBag.BookingChartData = "[]";
        }

        return View(overview);
    }
}

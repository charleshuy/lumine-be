using Application.DTOs;
using Lumine.MVCWebApp.FE.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class DashboardController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountsController> _logger;
    private readonly string _apiBaseUrlBooking = "https://localhost:7216/api/Booking";

    public DashboardController(IHttpClientFactory httpClientFactory, ILogger<AccountsController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Fetch overview data
        var overviewResponse = await _httpClient.GetAsync("https://localhost:7216/api/Overview");
        AppOverviewDTO? overview = null;

        if (overviewResponse.IsSuccessStatusCode)
        {
            var jsonOverview = await overviewResponse.Content.ReadAsStringAsync();
            overview = JsonConvert.DeserializeObject<AppOverviewDTO>(jsonOverview);
        }

        // Fetch chart data
        var chartResponse = await _httpClient.GetAsync($"{_apiBaseUrlBooking}/status-summary");
        if (chartResponse.IsSuccessStatusCode)
        {
            var chartJson = await chartResponse.Content.ReadAsStringAsync();
            ViewBag.BookingChartData = chartJson;
        }
        else
        {
            ViewBag.BookingChartData = "[]";
        }

        return View(overview); // Now passes correct model!
    }

}

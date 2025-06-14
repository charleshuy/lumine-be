using Lumine.MVCWebApp.FE.Controllers;
using Microsoft.AspNetCore.Mvc;

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
        var response = await _httpClient.GetAsync($"{_apiBaseUrlBooking}/status-summary");
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.BookingChartData = "[]";
            return View();
        }

        var json = await response.Content.ReadAsStringAsync();
        ViewBag.BookingChartData = json;
        return View();
    }
}

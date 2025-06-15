using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lumine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OverviewController : ControllerBase
    {
        private readonly IAppOverviewService _overviewService;

        public OverviewController(IAppOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        // GET: api/overview
        [HttpGet]
        public async Task<ActionResult<AppOverviewDTO>> GetOverview()
        {
            var overview = await _overviewService.GetOverviewSummaryAsync();
            return Ok(overview);
        }
    }
}

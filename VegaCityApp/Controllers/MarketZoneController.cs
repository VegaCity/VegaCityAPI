using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class MarketZoneController : BaseController<MarketZoneController>
    {
        private readonly IMarketZoneService _service;

        public MarketZoneController(ILogger<MarketZoneController> logger, IMarketZoneService service) : base(logger)
        {
            _service = service;
        }
        [HttpPost(MarketZoneEndpoint.CreateMarketZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> CreateMarketZone([FromBody] MarketZoneRequest request)
        {
            var result = await _service.CreateMarketZone(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}

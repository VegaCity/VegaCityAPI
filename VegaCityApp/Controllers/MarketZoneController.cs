using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Domain.Models;
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
        [CustomAuthorize(RoleEnum.AdminSystem)]
        public async Task<IActionResult> CreateMarketZone([FromBody] MarketZoneRequest request)
        {
            var result = await _service.CreateMarketZone(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(MarketZoneEndpoint.UpdateMarketZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> UpdateMarketZone([FromBody] MarketZoneRequest request)
        {
            var result = await _service.UpdateMarketZone(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(MarketZoneEndpoint.DeleteMarketZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.AdminSystem)]
        public async Task<IActionResult> DeleteMarketZone(Guid id)
        {
            var result = await _service.DeleteMarketZone(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(MarketZoneEndpoint.GetMarketZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetMarketZone(Guid id)
        {
            var result = await _service.GetMarketZone(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(MarketZoneEndpoint.GetListMarketZone)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<GetMarketZoneResponse>>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetListMarketZone([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllOrders(size, page);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(MarketZoneEndpoint.CreateRole)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.AdminSystem)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> CreateRole(string name)
        {
            var result = await _service.CreateRole(name);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(MarketZoneEndpoint.UpdateRole)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.AdminSystem)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var result = await _service.DeleteRole(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(MarketZoneEndpoint.UpdateRole)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.AdminSystem)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromQuery] string name)
        {
            var result = await _service.UpdateRole(id, name);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(MarketZoneEndpoint.GetListRole)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<Object>>), HttpStatusCodes.OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetListRole([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.GetListRole(size, page);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(MarketZoneEndpoint.CreateMarketZoneConfig)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.AdminSystem, RoleEnum.Admin)]
        public async Task<IActionResult> CreateMarketZoneConfig([FromQuery] Guid apiKey, [FromQuery] double storeTransferRate, [FromQuery] double withrawRate)
        {
            var result = await _service.CreateMarketZoneConfig(apiKey, storeTransferRate, withrawRate);
            return StatusCode(result.StatusCode, result);
        }
    }
}

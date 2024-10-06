using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Zone;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.GetZoneResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class ZoneController : BaseController<ZoneController>
    {
        private readonly IZoneService _zoneService;

        public ZoneController(ILogger<ZoneController> logger, IZoneService zoneService) : base(logger)
        {
            _zoneService = zoneService;
        }
        [HttpPost(ZoneEndPoint.CreateZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> CreateZone([FromBody] CreateZoneRequest request)
        {
            var result = await _zoneService.CreateZone(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(ZoneEndPoint.UpdateZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> UpdateZone(Guid id, [FromBody] UpdateZoneRequest request)
        {
            var result = await _zoneService.UpdateZone(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(ZoneEndPoint.SearchAllZone)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<GetZoneResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> SearchZones([FromQuery]int size = 10,[FromQuery] int page = 1)
        {
            var result = await _zoneService.SearchZones(size, page);
            return Ok(result);
        }
        [HttpGet(ZoneEndPoint.SearchZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> SearchZone(Guid id)
        {
            var result = await _zoneService.SearchZone(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(ZoneEndPoint.DeleteZone)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> DeleteZone(Guid id)
        {
            var result = await _zoneService.DeleteZone(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

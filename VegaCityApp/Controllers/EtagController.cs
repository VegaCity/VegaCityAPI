using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Implement;
using VegaCityApp.API.Services.Interface;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;
using VegaCityApp.API.Validators;
using VegaCityApp.API.Payload.Request.Etag;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class EtagController : BaseController<EtagController>
    {
        private readonly IEtagService _service;

        public EtagController(ILogger<EtagController> logger, IEtagService service) : base(logger)
        {
            _service = service;
        }
        [HttpPost(EtagTypeEndpoint.CreateEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> CreateEtagType([FromBody] EtagTypeRequest request)
        {
            var result = await _service.CreateEtagType(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(EtagTypeEndpoint.UpdateEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateEtagType(Guid id, [FromBody] UpdateEtagTypeRequest request)
        {
            var result = await _service.UpdateEtagType(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(EtagTypeEndpoint.DeleteEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteEtagType( Guid id)
        {
            var result = await _service.DeleteEtagType(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagTypeEndpoint.SearchEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchEtagType( Guid id)
        {
            var result = await _service.SearchEtagType(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagTypeEndpoint.SearchAllEtagType)]
        [ProducesResponseType(typeof(EtagTypeResponse), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllEtagType([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllEtagType(size, page);
            return Ok(result);
        }
        [HttpPost(EtagEndpoint.CreateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreateEtag([FromBody] EtagRequest request)
        {
            var result = await _service.CreateEtag(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(EtagTypeEndpoint.AddEtagTypeToPackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> AddEtagTypeToPackage(Guid etagTypeId, Guid packageId)
        {
            var result = await _service.AddEtagTypeToPackage(etagTypeId, packageId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(EtagTypeEndpoint.RemoveEtagTypeFromPackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> RemoveEtagTypeFromPackage(Guid etagTypeId, Guid packageId)
        {
            var result = await _service.RemoveEtagTypeFromPackage(etagTypeId, packageId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(EtagEndpoint.GenerateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> GenerateEtag([FromQuery]int quantity,[FromQuery] Guid etagTypeId, [FromBody] GenerateEtagRequest req)
        {
            var result = await _service.GenerateEtag(quantity, etagTypeId, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(EtagEndpoint.ActivateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> ActivateEtag(Guid id, [FromBody] ActivateEtagRequest request)
        {
            var result = await _service.ActivateEtag(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(EtagEndpoint.UpdateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateEtag(Guid id, [FromBody] UpdateEtagRequest request)
        {
            var result = await _service.UpdateEtag(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(EtagEndpoint.DeleteEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteEtag(Guid id)
        {
            var result = await _service.DeleteEtag(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagEndpoint.SearchEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchEtag(Guid id)
        {
            var result = await _service.SearchEtag(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagEndpoint.SearchAllEtag)]
        [ProducesResponseType(typeof(EtagResponse), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllEtag([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllEtag(size, page);
            return Ok(result);
        }
    }
}

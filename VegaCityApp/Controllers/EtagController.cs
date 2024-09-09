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
        public async Task<IActionResult> SearchAllEtagType([FromQuery] int size, [FromQuery] int page)
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
    }
}

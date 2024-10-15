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
using VegaCityApp.Domain.Paginate;

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
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> UpdateEtagType(Guid id, [FromBody] UpdateEtagTypeRequest request)
        {
            var result = await _service.UpdateEtagType(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(EtagTypeEndpoint.DeleteEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> DeleteEtagType( Guid id)
        {
            var result = await _service.DeleteEtagType(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagTypeEndpoint.SearchEtagType)]
        [ProducesResponseType(typeof(EtagTypeResponse), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> SearchEtagType( Guid id)
        {
            var result = await _service.SearchEtagType(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagTypeEndpoint.SearchAllEtagType)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<EtagTypeResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> SearchAllEtagType([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllEtagType(size, page);
            return Ok(result);
        }
        [HttpPost(EtagEndpoint.CreateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb)]
        public async Task<IActionResult> CreateEtag([FromBody] EtagRequest request)
        {
            var result = await _service.CreateEtag(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(EtagTypeEndpoint.AddEtagTypeToPackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> AddEtagTypeToPackage(Guid etagTypeId, Guid packageId,[FromQuery] int quantityEtagType)
        {
            var result = await _service.AddEtagTypeToPackage(etagTypeId, packageId, quantityEtagType);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(EtagTypeEndpoint.RemoveEtagTypeFromPackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> RemoveEtagTypeFromPackage(Guid etagTypeId, Guid packageId)
        {
            var result = await _service.RemoveEtagTypeFromPackage(etagTypeId, packageId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(EtagEndpoint.GenerateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        public async Task<IActionResult> GenerateEtag([FromQuery]int quantity,[FromQuery] Guid etagTypeId, [FromBody] GenerateEtagRequest req)
        {
            var result = await _service.GenerateEtag(quantity, etagTypeId, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(EtagEndpoint.ActivateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        public async Task<IActionResult> ActivateEtag(Guid id, [FromBody] ActivateEtagRequest request)
        {
            var result = await _service.ActivateEtag(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(EtagEndpoint.UpdateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        // có bug nhưng ko đáng kể, fix sau 
        public async Task<IActionResult> UpdateEtag(Guid id, [FromBody] UpdateEtagRequest request)
        {
            var result = await _service.UpdateEtag(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(EtagEndpoint.DeleteEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb)]
        public async Task<IActionResult> DeleteEtag(Guid id)
        {
            var result = await _service.DeleteEtag(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagEndpoint.SearchEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchEtag(Guid? id , string? etagCode)
        {
            var result = await _service.SearchEtag(id, etagCode);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(EtagEndpoint.SearchAllEtag)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<EtagResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> SearchAllEtag([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllEtag(size, page);
            return Ok(result);
        }
        [HttpPost(EtagEndpoint.ChargeMoneyETag)]
        [ProducesResponseType(typeof(ResponseAPI<ChargeMoneyEtagRequest>), HttpStatusCodes.OK)]
        //[CustomAuthorize(RoleEnum.CashierApp, RoleEnum.CashierWeb)]
        public async Task<IActionResult> PrepareChargeMoneyEtag([FromBody] ChargeMoneyEtagRequest req)
        {
            var result = await _service.PrepareChargeMoneyEtag(req);
            return Ok(result);
        }

    }
}

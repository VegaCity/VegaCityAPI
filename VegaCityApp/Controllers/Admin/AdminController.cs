using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;
using static VegaCityApp.API.Constants.ApiEndPointConstant;

namespace VegaCityApp.API.Controllers.Admin
{
    [ApiController]
    public class AdminController : BaseController<AdminController>
    {
        private readonly IAccountService _service;
        private readonly IEtagService _etagService;

        public AdminController(ILogger<AdminController> logger, IAccountService accountService, IEtagService etagService) : base(logger)
        {
            _service = accountService;
            _etagService = etagService;
        }

        [HttpPost(EtagTypeEndpoint.CreateEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateEtagType([FromBody] EtagTypeRequest request)
        {
            var result = await _etagService.CreateEtagType(request);
            return result.StatusCode == HttpStatusCodes.Created ? Created("", result) : BadRequest(result);
        }
        [HttpPatch(EtagTypeEndpoint.UpdateEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> UpdateEtagType([FromBody] UpdateEtagTypeRequest request)
        {
            var result = await _etagService.UpdateEtagType(request);
            return result.StatusCode == HttpStatusCodes.OK? Ok(result): BadRequest(result);
        }
        [HttpDelete(EtagTypeEndpoint.DeleteEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> DeleteEtagType([FromQuery] Guid etagTypeId)
        {
            var result = await _etagService.DeleteEtagType(etagTypeId);
            return result.StatusCode == HttpStatusCodes.OK ? Ok(result) : BadRequest(result);
        }
        [HttpGet(EtagTypeEndpoint.SearchEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> SearchEtagType([FromQuery] Guid etagTypeId)
        {
            var result = await _etagService.SearchEtagType(etagTypeId);
            return result.StatusCode == HttpStatusCodes.OK ? Ok(result) : BadRequest(result);
        }
        [HttpGet(EtagTypeEndpoint.SearchAllEtagType)]
        [ProducesResponseType(typeof(EtagTypeResponse), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(EtagTypeResponse), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> SearchAllEtagType([FromQuery] int size, [FromQuery] int page)
        {
            var result = await _etagService.SearchAllEtagType(size, page);
            return Ok(result);
        }
        [HttpPost(EtagEndpoint.CreateEtag)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateEtag([FromBody] EtagRequest request)
        {
            var result = await _etagService.CreateEtag(request);
            return Ok(result);
        }
        [HttpPost(UserEndpoint.ApproveUser)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> ApproveUser([FromBody] ApproveRequest request)
        {
            var result = await _service.ApproveUser(request);
            return Ok(result);
        }

    }
}

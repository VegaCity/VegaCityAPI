using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Service.Interface;
using static Pos_System.API.Constants.MessageConstant;
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

        [HttpPost("account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var result = await _service.CreateAccount(request);
            return Ok(result);
        }
        [HttpPost(EtagTypeEndpoint.CreateEtagType)]
        [ProducesResponseType(typeof(CreateEtagTypeResponse), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(CreateEtagTypeResponse), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateEtagType([FromBody] EtagTypeRequest request)
        {
            var result = await _etagService.CreateEtagType(request);
            return Ok(result);
        }
        [HttpPost(EtagEndpoint.CreateEtag)]
        [ProducesResponseType(typeof(CreateEtagResponse), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(CreateEtagResponse), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateEtag([FromBody] EtagRequest request)
        {
            var result = await _etagService.CreateEtag(request);
            return Ok(result);
        }

    }
}

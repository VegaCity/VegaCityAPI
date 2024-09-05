using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Payload.Request;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.ApiEndPointConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class AuthController : BaseController<AuthController>
    {
        private readonly IAccountService _accountService;
        public AuthController(ILogger<AuthController> logger, IAccountService service) : base(logger)
        {
            _accountService = service;
        }

        [HttpPost(AuthenticationEndpoint.Login)]

        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.Login(request);
            return Ok(result);
        }

        [HttpPost(AuthenticationEndpoint.Register)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _accountService.Register(request);
            return Ok(result);
        }
        [HttpPost(AuthenticationEndpoint.ChangePassword)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _accountService.ChangePassword(request);
            return Ok(result);
        }
    }
}

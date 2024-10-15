using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

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
        [ProducesResponseType(typeof(LoginResponse), HttpStatusCodes.OK)]
        [SwaggerOperation(Summary = "Login for user")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.Login(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(AuthenticationEndpoint.Register)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [SwaggerOperation(Summary = "Register new user for Store")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _accountService.Register(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(AuthenticationEndpoint.ChangePassword)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _accountService.ChangePassword(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(AuthenticationEndpoint.RefreshToken)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> RefreshToken([FromBody] ReFreshTokenRequest request)
        {
            var result = await _accountService.RefreshToken(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(AuthenticationEndpoint.GetRefreshTokenByEmail)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetRefreshTokenByEmail(string email, [FromBody] GetApiKey req)
        {
            var result = await _accountService.GetRefreshTokenByEmail(email, req);
            return StatusCode(result.StatusCode, result);
        }
    }
}

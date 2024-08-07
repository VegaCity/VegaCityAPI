using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.Payload.Request;
using VegaCityApp.Service.Interface;

namespace VegaCityApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController<AuthController>
    {
        private readonly IAccountService _accountService;
        public AuthController(ILogger<AuthController> logger, IAccountService service) : base(logger)
        {
            _accountService = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            //var result = await _accountService.Login(request);
            return Ok("ok");
        }
    }
}

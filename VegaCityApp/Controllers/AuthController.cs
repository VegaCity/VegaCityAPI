﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Payload.Request;
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.Login(request);
            return result.StatusCode == HttpStatusCodes.OK ? Ok(result) : BadRequest(result);
        }

        [HttpPost(AuthenticationEndpoint.Register)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _accountService.Register(request);
            return Ok(result);
        }
        [HttpPost(AuthenticationEndpoint.ChangePassword)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _accountService.ChangePassword(request);
            return Ok(result);
        }
    }
}

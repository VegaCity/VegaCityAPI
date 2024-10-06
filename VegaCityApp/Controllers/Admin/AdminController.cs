﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using VegaCityApp.API.Validators;
using VegaCityApp.API.Enums;
using Microsoft.AspNetCore.Authorization;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;
using VegaCityApp.Service.Implement;
using Swashbuckle.AspNetCore.Annotations;

namespace VegaCityApp.API.Controllers.Admin
{
    [ApiController]
    public class AdminController : BaseController<AdminController>
    {
        private readonly IAccountService _service;

        public AdminController(ILogger<AdminController> logger, IAccountService service) : base(logger)
        {
            _service = service;
        }
        [HttpPost(UserEndpoint.CreateUser)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [SwaggerOperation(Summary = "Create new user for cashier web, cashier app")]
        //[CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> AdminCreateUser([FromBody] RegisterRequest request)
        {
            var result = await _service.AdminCreateUser(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(UserEndpoint.ApproveUser)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> ApproveUser(Guid userId,[FromBody] ApproveRequest request)
        {
            var result = await _service.ApproveUser(userId,request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet(UserEndpoint.GetListUser)]
        [ProducesResponseType(typeof(ResponseAPI<IPaginate<GetUserResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> SearchAllUser([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllUser(size, page);
            return Ok(result);
        }
        [HttpGet(UserEndpoint.GetUserInfo)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp, RoleEnum.CashierWeb)]
        public async Task<IActionResult> SearchUser(Guid id)
        {
            var result = await _service.SearchUser(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(UserEndpoint.UpdateUserProfile)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp, RoleEnum.CashierWeb)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserAccountRequest request)
        {
            var result = await _service.UpdateUser(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(UserEndpoint.DeleteUser)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _service.DeleteUser(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

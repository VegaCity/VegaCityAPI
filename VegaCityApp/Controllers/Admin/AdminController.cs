using Microsoft.AspNetCore.Http;
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
    public class UserController : BaseController<UserController>
    {
        private readonly IAccountService _service;

        public UserController(ILogger<UserController> logger, IAccountService service) : base(logger)
        {
            _service = service;
        }
        [HttpPost(UserEndpoint.CreateUser)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [SwaggerOperation(Summary = "Create new user for admin, cashier web, cashier app")]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> AdminCreateUser([FromBody] RegisterRequest request)
        {
            var result = await _service.AdminCreateUser(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(UserEndpoint.ApproveUser)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Admin)]
        [SwaggerOperation(Summary = "Approve user !! Get Ready !!")]
        public async Task<IActionResult> ApproveUser(Guid userId, [FromBody] ApproveRequest request)
        {
            var result = await _service.ApproveUser(userId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet(UserEndpoint.GetListUser)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<GetUserResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> SearchAllUser([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _service.SearchAllUser(size, page);
            return Ok(result);
        }
        [HttpGet(UserEndpoint.GetUserInfo)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp, RoleEnum.CashierWeb, RoleEnum.Store)]
        [SwaggerOperation(Summary = "Search user by id !! Get Ready !!")]
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
        //get admin wallet 
        [HttpGet(UserEndpoint.GetAdminWallet)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp)]
        public async Task<IActionResult> GetAdminWallet()
        {
            var result = await _service.GetAdminWallet();
            return Ok(result);
        }
        [HttpPost(UserEndpoint.GetChartDashboard)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp, RoleEnum.Store)]
        public async Task<IActionResult> GetChartByDuration(AdminChartDurationRequest req)
        {
            var response = await _service.GetChartByDuration(req);
            return Ok(response);
        }
        [HttpPost(UserEndpoint.ReAssignEmail)]
        [ProducesResponseType(typeof(string), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        [SwaggerOperation(Summary = "Re-assign email for user has status pending verify")]
        public async Task<IActionResult> ReAssignEmail(Guid userId, [FromBody] ReAssignEmail email)
        {
            var result = await _service.ReAssignEmail(userId, email);
            return Ok(result);
        }
    }
}

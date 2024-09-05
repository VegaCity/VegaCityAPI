using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using Microsoft.AspNetCore.Authorization;

namespace VegaCityApp.API.Controllers.Admin
{
    [ApiController]
    public class AdminController : BaseController<AdminController>
    {
        private readonly IAccountService _service;
        private readonly IEtagService _etagService;
        private readonly IPackageService _packageService;

        public AdminController(ILogger<AdminController> logger, IAccountService accountService, IEtagService etagService, IPackageService packgeService) : base(logger)
        {
            _service = accountService;
            _etagService = etagService;
            _packageService = packgeService;
        }

        [HttpPost(EtagTypeEndpoint.CreateEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateEtagType([FromBody] EtagTypeRequest request)
        {
            var result = await _etagService.CreateEtagType(request);
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

        [HttpGet(UserEndpoint.GetListUser)]
        //[Authorize]
        public async Task<IActionResult> GetUserList([FromQuery] GetListParameterRequest request)
        {
            var result = await _service.GetUserList(request);
            return Ok(result);
        }
        [HttpGet(UserEndpoint.GetListUserByRoleId)]
        //[Authorize]
        public async Task<IActionResult> GetListUserByUserRoleId(Guid RoleId)
        {
            var result = await _service.GetListUserByUserRoleId(RoleId);
            return Ok(result);
        }
        //[HttpPut(UserEndpoint.UpdateUserRoleById)]
        //[Authorize]
        //[ProducesResponseType(typeof(UpdateUserRoleResponse), HttpStatusCodes.Created)]
        //[ProducesResponseType(typeof(UpdateUserRoleResponse), HttpStatusCodes.BadRequest)]
        //public async Task<IActionResult> UpdateUserRoleById([FromBody] UpdateUserRoleByIdRequest request)
        //{
        //    var result = await _service.UpdateUserRoleById(request);
        //    return Ok(result);
        //}
        ////user
        [HttpGet(UserEndpoint.GetUserInfo)]
       // [Authorize]
        public async Task<IActionResult> GetUserDetail(Guid UserId)
        {
            var result = await _service.GetUserDetail(UserId);
            return Ok(result);
        }
        [HttpPut(UserEndpoint.UpdateUserProfile)]
        //[Authorize]
        public async Task<IActionResult> UpdateUserById([FromBody] UpdateUserAccountRequest request, Guid UserId)
        {
            var result = await _service.UpdateUserById(request, UserId);
            return Ok(result);
        }
        //[HttpDelete(UserEndpoint.DeleteUser)]
        ////  [Authorize]
        //public async Task<IActionResult> DeleteUser(Guid UserId)
        //{
        //    var result = await _service.DeleteUserById(UserId);
        //    return Ok(result);
        //}
        [HttpPost(packageEndpoint.CreatePackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request, Guid UserId)
        {
            var result = await _packageService.CreatePackage(request, UserId);
            return Ok(result);
        }

        [HttpPut(packageEndpoint.UpdatePackage)]
        //[Authorize]
        public async Task<IActionResult> UpdatePackage([FromBody] UpddatePackageRequest request, Guid UserId)
        {
            var result = await _packageService.UpdatePackage(request, UserId);
            return Ok(result);
        }

        [HttpGet(packageEndpoint.GetListPackage)]
        //[Authorize]
        public async Task<IActionResult> GetListPackage([FromQuery] GetListParameterRequest request)
        {
            var result = await _packageService.GetListPackage(request);
            return Ok(result);
        }

        [HttpGet(packageEndpoint.GetPackageById)]
        //[Authorize]
        public async Task<IActionResult> GetPackageDetail(Guid PackageId)
        {
            var result = await _packageService.GetPackageDetail(PackageId);
            return Ok(result);
        }






    }
}

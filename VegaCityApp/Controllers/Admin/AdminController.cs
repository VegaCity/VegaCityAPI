using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using VegaCityApp.API.Validators;
using VegaCityApp.API.Enums;
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
        [CustomAuthorize(RoleEnum.Admin)]
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

        [HttpGet(UserEndpoint.GetListUser)]
        //[Authorize]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> SearchAllUser([FromQuery] int size, [FromQuery] int page )
        {
            var result = await _service.SearchAllUser(size, page);
            return Ok(result);
        }
        // [HttpGet(UserEndpoint.GetListUserByRoleId)]
        // //[Authorize]
        // public async Task<IActionResult> GetListUserByUserRoleId(Guid RoleId)
        // {
        //     var result = await _service.GetListUserByUserRoleId(RoleId);
        //     return Ok(result);
        // }
        // //[HttpPut(UserEndpoint.UpdateUserRoleById)]
        // //[Authorize]
        // //[ProducesResponseType(typeof(UpdateUserRoleResponse), HttpStatusCodes.Created)]
        // //[ProducesResponseType(typeof(UpdateUserRoleResponse), HttpStatusCodes.BadRequest)]
        // //public async Task<IActionResult> UpdateUserRoleById([FromBody] UpdateUserRoleByIdRequest request)
        // //{
        // //    var result = await _service.UpdateUserRoleById(request);
        // //    return Ok(result);
        // //}
        // ////user
        [HttpGet(UserEndpoint.GetUserInfo)]
        // [Authorize]
        public async Task<IActionResult> SearchUser(Guid UserId)
        {
            var result = await _service.SearchUser(UserId);
            return Ok(result);
        }
        [HttpPatch(UserEndpoint.UpdateUserProfile)]
        //[Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserAccountRequest request)
        {
            var result = await _service.UpdateUser(request);
            return Ok(result);
        }
        [HttpDelete(UserEndpoint.DeleteUser)]
        //  [Authorize]
        public async Task<IActionResult> DeleteUser(Guid UserId)
        {
            var result = await _service.DeleteUser(UserId);
            return Ok(result);
        }
        [HttpPost(packageEndpoint.CreatePackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
        {
            var result = await _packageService.CreatePackage(request);
            return Ok(result);
        }
        [HttpGet(packageEndpoint.GetListPackage)]
        //[Authorize]
        public async Task<IActionResult> SearchAllPackage(int size, int page)
        {
            var result = await _packageService.SearchAllPackage(size, page);
            return Ok(result);
        }


        [HttpGet(packageEndpoint.GetPackageById)]
        //[Authorize]
        public async Task<IActionResult> SearchPackage(Guid PackageId)
        {
            var result = await _packageService.SearchPackage(PackageId);
            return Ok(result);
        }

        [HttpPatch(packageEndpoint.UpdatePackage)]
        //[Authorize]
        public async Task<IActionResult> UpdatePackage([FromBody] UpdatePackageRequest request)
        {
            var result = await _packageService.UpdatePackage(request);
            return Ok(result);
        }
        [HttpDelete(packageEndpoint.DeletePackage)]
        //  [Authorize]
        public async Task<IActionResult> DeletePackage(Guid PackageId)
        {
            var result = await _packageService.DeletePackage(PackageId);
            return Ok(result);
        }

        [HttpGet(storeEndpoint.GetListStore)]
        //[Authorize]
        public async Task<IActionResult> SearchAllStore(int size, int page)
        {
            var result = await _storeService.SearchAllStore(size, page);
            return Ok(result);
        }
        [HttpGet(storeEndpoint.GetStore)]
        //[Authorize]
        public async Task<IActionResult> SearchStore(Guid StoreId)
        {
            var result = await _storeService.SearchStore(StoreId);
            return Ok(result);
        }
     
        [HttpPatch(storeEndpoint.UpdateStore)]
        //[Authorize]
        public async Task<IActionResult> UpdateStore(UpdateStoreRequest req)
        {
            var result = await _storeService.UpdateStore(req);
            return Ok(result);
        }

        [HttpDelete(storeEndpoint.DeleteStore)]
        //  [Authorize]
        public async Task<IActionResult> DeleteStore(Guid StoreId)
        {
            var result = await _storeService.DeleteStore(StoreId);
            return Ok(result);
        }
    }
}

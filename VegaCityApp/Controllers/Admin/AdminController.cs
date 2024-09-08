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
        private readonly IStoreService _storeService;

        public AdminController(ILogger<AdminController> logger, IAccountService accountService, IEtagService etagService, IPackageService packageService, IStoreService storeService) : base(logger)
        {
            _service = accountService;
            _etagService = etagService;
            _packageService = packageService;
            _storeService = storeService;
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
        public async Task<IActionResult> UpdateEtagType(Guid id,[FromBody] UpdateEtagTypeRequest request)
        {
            var result = await _etagService.UpdateEtagType(id,request);
            return result.StatusCode == HttpStatusCodes.OK? Ok(result): BadRequest(result);
        }
        [HttpDelete(EtagTypeEndpoint.DeleteEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> DeleteEtagType([FromQuery] Guid id)
        {
            var result = await _etagService.DeleteEtagType(id);
            return result.StatusCode == HttpStatusCodes.OK ? Ok(result) : BadRequest(result);
        }
        [HttpGet(EtagTypeEndpoint.SearchEtagType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> SearchEtagType([FromQuery] Guid id)
        {
            var result = await _etagService.SearchEtagType(id);
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
        public async Task<IActionResult> ApproveUser(Guid userId,[FromBody] ApproveRequest request)
        {
            var result = await _service.ApproveUser(userId,request);
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
        [HttpGet(UserEndpoint.GetUserInfo)]
        public async Task<IActionResult> SearchUser(Guid id)
        {
            var result = await _service.SearchUser(id);
            return Ok(result);
        }
        [HttpPatch(UserEndpoint.UpdateUserProfile)]
        //[Authorize]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserAccountRequest request)
        {
            var result = await _service.UpdateUser(id, request);
            return Ok(result);
        }
        [HttpDelete(UserEndpoint.DeleteUser)]
        //  [Authorize]
        public async Task<IActionResult> DeleteUser(Guid UserId)
        {
            var result = await _service.DeleteUser(UserId);
            return Ok(result);
        }
        [HttpPost(PackageEndpoint.CreatePackage)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
        {
            var result = await _packageService.CreatePackage(request);
            return Ok(result);
        }
        [HttpGet(PackageEndpoint.GetListPackage)]
        //[Authorize]
        public async Task<IActionResult> SearchAllPackage(int size, int page)
        {
            var result = await _packageService.SearchAllPackage(size, page);
            return Ok(result);
        }


        [HttpGet(PackageEndpoint.GetPackageById)]
        //[Authorize]
        public async Task<IActionResult> SearchPackage(Guid id)
        {
            var result = await _packageService.SearchPackage(id);
            return Ok(result);
        }

        [HttpPatch(PackageEndpoint.UpdatePackage)]
        public async Task<IActionResult> UpdatePackage(Guid id,[FromBody] UpdatePackageRequest request)
        {
            var result = await _packageService.UpdatePackage(id, request);
            return Ok(result);
        }
        [HttpDelete(PackageEndpoint.DeletePackage)]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            var result = await _packageService.DeletePackage(id);
            return Ok(result);
        }

        [HttpGet(StoreEndpoint.GetListStore)]
        public async Task<IActionResult> SearchAllStore(int size, int page)
        {
            var result = await _storeService.SearchAllStore(size, page);
            return Ok(result);
        }
        [HttpGet(StoreEndpoint.GetStore)]
        public async Task<IActionResult> SearchStore(Guid id)
        {
            var result = await _storeService.SearchStore(id);
            return Ok(result);
        }
     
        [HttpPatch(StoreEndpoint.UpdateStore)]
        public async Task<IActionResult> UpdateStore(Guid id,UpdateStoreRequest req)
        {
            var result = await _storeService.UpdateStore(id,req);
            return Ok(result);
        }

        [HttpDelete(StoreEndpoint.DeleteStore)]
        public async Task<IActionResult> DeleteStore(Guid id)
        {
            var result = await _storeService.DeleteStore(id);
            return Ok(result);
        }
    }
}

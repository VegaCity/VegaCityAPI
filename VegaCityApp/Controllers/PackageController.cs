using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class PackageController : BaseController<PackageController>
    {
        private readonly IPackageService _packageService;

        public PackageController(ILogger<PackageController> logger, IPackageService packageService) : base(logger)
        {
            _packageService = packageService;
        }
        [HttpPost(PackageEndpoint.CreatePackage)]
        [CustomAuthorize(RoleEnum.Admin)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
        {
            var result = await _packageService.CreatePackage(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(PackageEndpoint.GetListPackage)]
        [CustomAuthorize(RoleEnum.Admin,RoleEnum.CashierApp,RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllPackage([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _packageService.SearchAllPackage(size, page);
            return Ok(result);
        }
        [HttpGet(PackageEndpoint.GetPackageById)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchPackage(Guid id)
        {
            var result = await _packageService.SearchPackage(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(PackageEndpoint.UpdatePackage)]
        [CustomAuthorize(RoleEnum.Admin)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdatePackageRequest request)
        {
            var result = await _packageService.UpdatePackage(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(PackageEndpoint.DeletePackage)]
        [CustomAuthorize(RoleEnum.Admin)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            var result = await _packageService.DeletePackage(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

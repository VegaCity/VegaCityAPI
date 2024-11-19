using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Order;
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
        [SwaggerOperation(Summary = "Create Package", Description = "Type: SpecificPackage, ServicePackage")]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
        {
            var result = await _packageService.CreatePackage(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(PackageEndpoint.GetListPackage)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp, RoleEnum.CashierWeb)]
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
        [SwaggerOperation(Summary = "If delete Package, Everything in package will be deleted")]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            var result = await _packageService.DeletePackage(id);
            return StatusCode(result.StatusCode, result);
        }
        //PackageType
        //[HttpPost(PackageEndpoint.CreatePackageType)]
        //[CustomAuthorize(RoleEnum.Admin)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        //public async Task<IActionResult> CreatePackageType([FromBody] CreatePackageTypeRequest request)
        //{
        //    var result = await _packageService.CreatePackageType(request);
        //    return StatusCode(result.StatusCode, result);
        //}
        //[HttpPatch(PackageEndpoint.UpdatePackageType)]
        //[CustomAuthorize(RoleEnum.Admin)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //public async Task<IActionResult> UpdatePackageType(Guid id, [FromBody] UpdatePackageTypeRequest request)
        //{
        //    var result = await _packageService.UpdatePackageType(id, request);
        //    return StatusCode(result.StatusCode, result);
        //}
        //[HttpGet(PackageEndpoint.GetListPackageType)]
        //[CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp, RoleEnum.CashierWeb)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //public async Task<IActionResult> SearchAllPackageType([FromQuery] int size = 10, [FromQuery] int page = 1)
        //{
        //    var result = await _packageService.SearchAllPackageType(size, page);
        //    return Ok(result);
        //}
        //[HttpGet(PackageEndpoint.GetPackageTypeById)]
        //[CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //public async Task<IActionResult> SearchPackageType(Guid id)
        //{
        //    var result = await _packageService.SearchPackageType(id);
        //    return StatusCode(result.StatusCode, result);
        //}
        //[HttpDelete(PackageEndpoint.DeletePackageType)]
        //[CustomAuthorize(RoleEnum.Admin)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //[SwaggerOperation(Summary = "If delete PackageType, Everything in package type will be deleted")]
        //public async Task<IActionResult> DeletePackageType(Guid id)
        //{
        //    var result = await _packageService.DeletePackageType(id);
        //    return StatusCode(result.StatusCode, result);
        //}
        //PackageItem
        [HttpPost(PackageEndpoint.CreatePackageItem)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [SwaggerOperation(Summary = "Generate package item as v-card")]
        public async Task<IActionResult> CreatePackageItem([FromQuery] int quantity, [FromBody] CreatePackageItemRequest request)
        {
            var result = await _packageService.CreatePackageItem(quantity, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(PackageEndpoint.UpdatePackageItem)]
        //[CustomAuthorize(RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdatePackageItem(Guid id, [FromBody] UpdatePackageItemRequest request)
        {
            var result = await _packageService.UpdatePackageItem(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(PackageEndpoint.GetListPackageItem)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierApp, RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllPackageItem([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _packageService.SearchAllPackageItem(size, page);
            return Ok(result);
        }
        [HttpGet(PackageEndpoint.GetPackageItemById)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchPackageItem([FromQuery] Guid? id, [FromQuery] string? rfId)
        {
            var result = await _packageService.SearchPackageItem(id, rfId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(PackageEndpoint.ActivePackageItem)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [SwaggerOperation(Summary = "Get ready to active")]
        public async Task<IActionResult> ActivePackageItem(Guid id, [FromBody] CustomerInfo req)
        {
            var result = await _packageService.ActivePackageItem(id, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(PackageEndpoint.PrepareChargeMoney)]
        [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        [SwaggerOperation(Summary = "Get ready to prepare charging")]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> PrepareChargeMoneyEtag([FromBody] ChargeMoneyRequest request)
        {
            var result = await _packageService.PrepareChargeMoneyEtag(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(PackageEndpoint.PackageItemPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> PackageItemPayment([FromQuery] Guid packageItemId,
            [FromQuery] int totalPrice, [FromQuery] Guid storeId, [FromBody] List<OrderProduct> products)
        {
            var result = await _packageService.PackageItemPayment(packageItemId, totalPrice, storeId, products);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(PackageEndpoint.UpdateRfId)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateRfIdPackageItem(Guid id, [FromQuery] string rfId)
        {
            var result = await _packageService.UpdateRfIdPackageItem(id, rfId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(PackageEndpoint.MarkPackageItemLost)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetLostPackageItem([FromBody] GetLostPackageItemRequest req)
        {
            var result = await _packageService.GetLostPackageItem(req);
            return StatusCode(result.StatusCode, result);
        }
    }
}

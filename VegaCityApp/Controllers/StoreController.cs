using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class StoreController : BaseController<StoreController>
    {
        private readonly IStoreService _storeService;

        public StoreController(ILogger<StoreController> logger, IStoreService storeService) : base(logger)
        {
            _storeService = storeService;
        }
        [HttpGet(StoreEndpoint.GetListStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //[CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        public async Task<IActionResult> SearchAllStore([FromQuery] Guid apiKey, [FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _storeService.SearchAllStore(apiKey, size, page);
            return Ok(result);
        }
        [HttpGet(StoreEndpoint.GetStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //[CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        public async Task<IActionResult> SearchStore(Guid id)
        {
            var result = await _storeService.SearchStore(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch(StoreEndpoint.UpdateStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        public async Task<IActionResult> UpdateStore(Guid id, UpdateStoreRequest req)
        {
            var result = await _storeService.UpdateStore(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete(StoreEndpoint.DeleteStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        [SwaggerOperation(Summary = "If delete Store, Everything in Store will be deleted")]
        public async Task<IActionResult> DeleteStore(Guid id)
        {
            var result = await _storeService.DeleteStore(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetMenu)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //[CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> GetMenu(string phone)
        {
            var result = await _storeService.GetMenuFromPos(phone);
            return StatusCode(result.StatusCode, result);
        }
    }
}

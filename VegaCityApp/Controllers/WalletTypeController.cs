using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class WalletTypeController : BaseController<WalletTypeController>
    {
        private readonly IWalletTypeService _walletTypeService;
        public WalletTypeController(ILogger<WalletTypeController> logger, IWalletTypeService service) : base(logger)
        {
            _walletTypeService = service;
        }

        [HttpPost(WalletTypeEndpoint.CreateWalletType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreateWalletType([FromBody] WalletTypeRequest request)
        {
            var result = await _walletTypeService.CreateWalletType(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch(WalletTypeEndpoint.UpdateWalletType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateWalletType(Guid id, [FromBody] UpDateWalletTypeRequest request)
        {
            var result = await _walletTypeService.UpdateWalletType(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete(WalletTypeEndpoint.DeleteWalletType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteWalletType(Guid id)
        {
            var result = await _walletTypeService.DeleteWalletType(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet(WalletTypeEndpoint.GetWalletTypeById)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetWalletTypeById(Guid id)
        {
            var result = await _walletTypeService.GetWalletTypeById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet(WalletTypeEndpoint.GetAllWalletType)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<WalletTypeResponse>>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllWalletType(int size = 10, int page = 1)
        {
            var result = await _walletTypeService.GetAllWalletType(size, page);
            return Ok(result);
        }
        [HttpPost(WalletTypeEndpoint.AddServiceStoreToWalletType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> AddServiceStoreToWalletType(Guid id, Guid serviceStoreId)
        {
            var result = await _walletTypeService.AddServiceStoreToWalletType(id, serviceStoreId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(WalletTypeEndpoint.RemoveServiceStoreToWalletType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> RemoveServiceStoreToWalletType(Guid id, Guid serviceStoreId)
        {
            var result = await _walletTypeService.RemoveServiceStoreToWalletType(id, serviceStoreId);
            return StatusCode(result.StatusCode, result);
        }
    }
}

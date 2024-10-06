using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class ServiceStoreController : BaseController<ServiceStoreController>
    {
        private readonly IServiceStore _serviceStore;

        public ServiceStoreController(IServiceStore serviceStore, ILogger<ServiceStoreController> logger) : base(logger)
        {
            _serviceStore = serviceStore;
        }
        [HttpPost(ServiceStoreEndpoint.CreateServiceStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreateServiceStore([FromBody] ServiceStoreRequest request)
        {
            var result = await _serviceStore.CreateServiceStore(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(ServiceStoreEndpoint.UpdateServiceStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateServiceStore(Guid id,[FromBody] UpDateServiceStoreRequest request)
        {
            var result = await _serviceStore.UpdateServiceStore(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(ServiceStoreEndpoint.DeleteServiceStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteServiceStore(Guid id)
        {
            var result = await _serviceStore.DeleteServiceStore(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(ServiceStoreEndpoint.GetServiceStoreById)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetServiceStoreById(Guid id)
        {
            var result = await _serviceStore.GetServiceStoreById(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(ServiceStoreEndpoint.GetAllServiceStore)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ServiceStoreResponse>>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllServiceStore([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _serviceStore.GetAllServiceStore(size, page);
            return Ok(result);
        }
    }
}

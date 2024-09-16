using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class OrderController : BaseController<OrderController>
    {
        private readonly IOrderService _orderService;

        public OrderController(ILogger<OrderController> logger, IOrderService orderService) : base(logger)
        {
            _orderService = orderService;
        }

        [HttpPost(OrderEndpoint.CreateOrder)]
        [ProducesResponseType(typeof(IPaginate<ResponseAPI>), HttpStatusCodes.OK)]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest req)
        {
            var result = await _orderService.CreateOrder(req);
            return Ok(result);
        }

        [HttpGet(OrderEndpoint.GetListOrder)]
        [ProducesResponseType(typeof(IPaginate<GetOrderResponse>), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllOrder(int size, int page)
        {
            var result = await _orderService.SearchAllOrders(size, page);
            return Ok(result);
        }

        [HttpGet(OrderEndpoint.GetOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchOrder(Guid id)
        {
            var result = await _orderService.SearchOrder(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch(OrderEndpoint.UpdateOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateOrder(Guid id, UpdateOrderRequest req)
        {
            var result = await _orderService.UpdateOrder(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete(OrderEndpoint.DeleteOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var result = await _orderService.DeleteOrder(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

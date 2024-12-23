﻿using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
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
        [ProducesResponseType(typeof(ResponseAPI<CreateOrderRequest>), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest req)
        {
            var result = await _orderService.CreateOrder(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet(OrderEndpoint.GetListOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store, RoleEnum.CashierWeb, RoleEnum.CashierApp, RoleEnum.Admin)]
        public async Task<IActionResult> SearchAllOrder([FromQuery] int size = 10, [FromQuery] int page = 1, [FromQuery] string status = "ALL")
        {
            var result = await _orderService.SearchAllOrders(size, page, status);
            return Ok(result);
        }

        [HttpGet(OrderEndpoint.GetOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchOrder(Guid? id,  string? InvoiceId)
        {
            var result = await _orderService.SearchOrder(id, InvoiceId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch(OrderEndpoint.UpdateOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateOrder([FromQuery]string InvoiceId, UpdateOrderRequest req)
        {
            var result = await _orderService.UpdateOrder(InvoiceId, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete(OrderEndpoint.CancelOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var result = await _orderService.DeleteOrder(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(OrderEndpoint.CreateOrderForCashier)]
        [ProducesResponseType(typeof(ResponseAPI<CreateOrderForCashierRequest>), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> CreateOrderForCashier(CreateOrderForCashierRequest req)
        {
            var result = await _orderService.CreateOrderForCashier(req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(OrderEndpoint.ConfirmOrderForCashier)]
        [ProducesResponseType(typeof(ResponseAPI<ConfirmOrderForCashierRequest>), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> ConfirmOrderForCashier(ConfirmOrderForCashierRequest req)
        {
            var result = await _orderService.ConfirmOrderForCashier(req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(OrderEndpoint.ConfirmOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> ConfirmOrder(ConfirmOrderRequest req)
        {
            var result = await _orderService.ConfirmOrder(req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(OrderEndpoint.GetOrderDetail)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> GetDetailMoneyFromOrder(Guid id)
        {
            var result = await _orderService.GetDetailMoneyFromOrder(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

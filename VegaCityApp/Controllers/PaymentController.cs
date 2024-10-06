using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PaymentResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Repository.Implement;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;
using VegaCityApp.Repository.Interfaces;
namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class PaymentController : BaseController<PaymentController>
    {
        private readonly IPaymentService _service;
        private readonly PayOS _payOs;
        private readonly IUnitOfWork _unitOfWork;
        public PaymentController(ILogger<PaymentController> logger, IPaymentService service, PayOS payOS) : base(logger)
        {
            _service = service;
            _payOs = payOS;
        }

        [HttpPost(PaymentEndpoint.MomoPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
       // [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> MomoPayment([FromBody] PaymentRequest request)
        {
            var result = await _service.MomoPayment(request);
            return StatusCode(result.StatusCode, result);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(PaymentEndpoint.UpdateOrderPaid)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateOrderPaid([FromQuery] IPNMomoRequest req)
        {
            var result = await _service.UpdateOrderPaid(req);
            if(result.StatusCode == HttpStatusCodes.NoContent)
            {
                return Redirect(result.MessageResponse);
            }
            return BadRequest();
        }
        [HttpPost(PaymentEndpoint.VnPayPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _service.VnPayment(request, HttpContext);
            return StatusCode(result.StatusCode, result);

        }
        [HttpPost(PaymentEndpoint.PayOSPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        // [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> PayOSPayment([FromBody] PaymentRequest request)
        {

            var result = await _service.PayOSPayment(request);
            return StatusCode(result.StatusCode, result);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(PaymentEndpoint.UpdateVnPayOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateOrderPaid([FromQuery] VnPayPaymentResponse req)
        {
            var result = await _service.UpdateVnPayOrder(req);
            if (result.StatusCode == HttpStatusCodes.NoContent)
            {
                return Redirect(result.MessageResponse);
            }
            return BadRequest();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [HttpGet(PaymentEndpoint.UpdateOrderPaidForChargingMoney)]
        public async Task<IActionResult> UpdateOrderPaidForChargingMoney([FromQuery] IPNMomoRequest req)
        {
            var result = await _service.UpdateOrderPaidForChargingMoney(req);
            if (result.StatusCode == HttpStatusCodes.NoContent)
            {
                return Redirect(result.MessageResponse);
            }
            return BadRequest();
        }

    }
}

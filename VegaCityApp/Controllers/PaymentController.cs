using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PaymentResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;
namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class PaymentController : BaseController<PaymentController>
    {
        private readonly IPaymentService _service;
        public PaymentController(ILogger<PaymentController> logger, IPaymentService service) : base(logger)
        {
            _service = service;
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
            var result = await _service.UpdateOrderPaidForCashier(req);
            if(result.StatusCode == HttpStatusCodes.NoContent)
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


        [HttpPost(PaymentEndpoint.VnPayPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        //[CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _service.VnPayment(request, HttpContext);
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
        [HttpGet(PaymentEndpoint.UpdateOrderVnPaidForChargingMoney)]
        public async Task<IActionResult> UpdateOrderPaidForChargingMoney([FromQuery] VnPayPaymentResponse req)
        {
            var result = await _service.UpdateOrderPaidForChargingMoney(req);
            if (result.StatusCode == HttpStatusCodes.NoContent)
            {
                return Redirect(result.MessageResponse);
            }
            return BadRequest();
        }

        //payos 
        [HttpPost(PaymentEndpoint.PayOSPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        // [CustomAuthorize(RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> PayOSPayment([FromBody] PaymentRequest request)
        {

            var result = await _service.PayOSPayment(request);
            return StatusCode(result.StatusCode, result);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(PaymentEndpoint.UpdatePayOSOrder)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdatePayOSOrder([FromQuery] string code, [FromQuery] string id, [FromQuery] string status, [FromQuery] string orderCode)
        {
            try
            {
                // Kiểm tra nếu mã trả về của PayOS là thành công
                if (code == "00" && status == "PAID")
                {
                    // Gọi service để cập nhật trạng thái đơn hàng theo orderCode
                    var result = await _service.UpdatePayOSOrder(code, id, status, orderCode);

                    if (result.StatusCode == HttpStatusCodes.NoContent)
                    {
                        // return Ok(new { message = "Order updated successfully." });
                        return Redirect(result.MessageResponse);
                    }
                    else
                    {
                        return BadRequest(new { message = "Failed to update order." });
                    }
                }
                else if (status == "CANCELED")
                {
                    return BadRequest(new { message = "Payment was not successful or canceled." });
                }
                else
                {
                    var result = await _service.UpdatePayOSOrder(code, id, status, orderCode);
                    return Redirect(result.MessageResponse);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing payment.");
            }
        }
       
        
        //below is charge money payos
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [HttpGet(PaymentEndpoint.UpdateOrderPaidOSForChargingMoney)]
        public async Task<IActionResult> UpdateOrderPaidForChargingMoney([FromQuery] string code, [FromQuery] string id, [FromQuery] string status, [FromQuery] string orderCode)
        {
            try
            {
                // Kiểm tra nếu mã trả về của PayOS là thành công
                if (code == "00" && status == "PAID")
                {
                    // Gọi service để cập nhật trạng thái đơn hàng theo orderCode
                    var result = await _service.UpdateOrderPaidOSForChargingMoney(code, id, status, orderCode);

                    if (result.StatusCode == HttpStatusCodes.NoContent)
                    {
                        // return Ok(new { message = "Order updated successfully." });
                        return Redirect(result.MessageResponse);
                    }
                    else
                    {
                        return BadRequest(new { message = "Failed to update order." });
                    }
                }
                else if(status == "CANCELED")
                {
                    return BadRequest(new { message = "Payment was not successful or canceled." });
                }
                else
                {
                    var result = await _service.UpdateOrderPaidOSForChargingMoney(code, id, status, orderCode);
                    return Redirect(result.MessageResponse);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing payment.");
            }
        }
        [HttpPost(PaymentEndpoint.ZaloPayPayment)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> ZaloPayPayment([FromBody] PaymentRequest request)
        {
            var result = await _service.ZaloPayPayment(request);
            return StatusCode(result.StatusCode, result);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(PaymentEndpoint.UpdateOrderPaidZaloPay)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateOrderPaidZaloPay([FromQuery] IPNZaloPayRequest req)
        {
            var result = await _service.UpdateOrderPaid(req);
            if (result.StatusCode == HttpStatusCodes.NoContent)
            {
                return Redirect(result.MessageResponse);
            }
            return Redirect(result.MessageResponse);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [HttpGet(PaymentEndpoint.UpdateOrderPaidForChargingMoneyZaloPay)]
        public async Task<IActionResult> UpdateOrderPaidForChargingMoney([FromQuery] IPNZaloPayRequest req)
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

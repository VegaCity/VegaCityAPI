using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
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
        public async Task<IActionResult> MomoPayment([FromBody] PaymentRequest request)
        {
            var result = await _service.MomoPayment(request);
            return StatusCode(result.StatusCode, result);
        }
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
        public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _service.VnPayment(request, HttpContext);
            return StatusCode(result.StatusCode, result);

        }
    }
}

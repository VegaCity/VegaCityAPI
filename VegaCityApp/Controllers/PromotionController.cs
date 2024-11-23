using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Promotion;
using VegaCityApp.API.Payload.Request.Zone;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.GetZoneResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class PromotionController : BaseController<PromotionController>
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(ILogger<PromotionController> logger, IPromotionService promotionService) : base(logger)
        {
            _promotionService = promotionService;
        }
        [HttpPost(PromotionEndPoint.CreatePromotion)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionRequest request)
        {
            var result = await _promotionService.CreatePromotion(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(PromotionEndPoint.UpdatePromotion)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> UpdatePromotion(Guid id, [FromBody] UpdatePromotionRequest request)
        {
            var result = await _promotionService.UpdatePromotion(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(PromotionEndPoint.SearchAllPromotions)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<GetZoneResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp)]
        public async Task<IActionResult> SearchPromotions([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _promotionService.SearchPromotions(size, page);
            return Ok(result);
        }
        [HttpGet(PromotionEndPoint.SearchPromotion)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchPromotion(Guid id)
        {
            var result = await _promotionService.SearchPromotion(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(PromotionEndPoint.DeletePromotion)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        [SwaggerOperation(Summary = "If delete Promotion, Everything in zone will be deleted")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var result = await _promotionService.DeletePromotion(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}

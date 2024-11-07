using VegaCityApp.API.Payload.Request.Promotion;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.GetZoneResponse;
using VegaCityApp.API.Payload.Response.PromotionResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPromotionService
    {
        Task<ResponseAPI> CreatePromotion(PromotionRequest req);
        Task<ResponseAPI> UpdatePromotion(Guid PromotionId, UpdatePromotionRequest req);
        Task<ResponseAPI<IEnumerable<GetListPromotionResponse>>> SearchPromotions(int size, int page);
        Task<ResponseAPI> SearchPromotion(Guid promotionId);
        Task<ResponseAPI> DeletePromotion(Guid promotionId);
        Task CheckExpiredPromotion();
    }
}

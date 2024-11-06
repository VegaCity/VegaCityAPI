using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Services.Interface
{
    public interface IMarketZoneService
    {
        Task<ResponseAPI> CreateMarketZone(MarketZoneRequest request);
        Task<ResponseAPI> UpdateMarketZone(MarketZoneRequest request);
        Task<ResponseAPI> DeleteMarketZone(Guid id);
        Task<ResponseAPI<MarketZone>> GetMarketZone(Guid id);
        Task<ResponseAPI<IEnumerable<GetMarketZoneResponse>>> SearchAllOrders(int size, int page);
    }
}

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
        Task<ResponseAPI> CreateRole(string name);
        Task<ResponseAPI> DeleteRole(Guid id);
        Task<ResponseAPI> UpdateRole(Guid id, string name);
        Task<ResponseAPI<IEnumerable<Object>>> GetListRole(int size, int page);
        Task<ResponseAPI> CreateMarketZoneConfig(Guid apiKey, double storeTransferRate, double withrawRate);
    }
}

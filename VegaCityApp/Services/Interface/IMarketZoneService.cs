using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Response;

namespace VegaCityApp.API.Services.Interface
{
    public interface IMarketZoneService
    {
        Task<ResponseAPI> CreateMarketZone(MarketZoneRequest request);
    }
}

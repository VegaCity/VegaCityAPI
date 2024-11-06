using AutoMapper;

namespace VegaCityApp.API.Mapper
{
    public class MarketZoneMapper : Profile
    {
        public MarketZoneMapper()
        {
            CreateMap<VegaCityApp.API.Payload.Request.Admin.MarketZoneRequest, VegaCityApp.Domain.Models.MarketZone>();
            CreateMap<VegaCityApp.Domain.Models.MarketZone, VegaCityApp.API.Payload.Request.Admin.MarketZoneRequest>();
        }
    }
}

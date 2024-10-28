using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Services.Interface
{
    public interface IUtilService
    {
        Task<MarketZone> GetMarketZone(Guid marketZoneId);
        Task<User> GetUserPhone(string phone, Guid marketZoneId);
        Task<User> GetUserCCCDPassport(string cccd_passport, Guid marketZoneId);
        Task<User> GetUser(string email, Guid marketZoneId);
        Task<House> GetHouse(string location, string address);
    }
}

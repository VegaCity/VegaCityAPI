using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;

namespace VegaCityApp.API.Services.Implement
{
    public class UtilService : BaseService<UtilService>, IUtilService
    {
        public UtilService(IUnitOfWork<VegaCityAppContext> unitOfWork, 
                           ILogger<UtilService> logger, 
                           IHttpContextAccessor httpContextAccessor, 
                           IMapper mapper) 
        : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }
        public async Task<MarketZone> GetMarketZone(Guid marketZoneId)
        {
            MarketZone marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == marketZoneId && x.Deflag == false);
            return marketZone;
        }
        public async Task<User> GetUserPhone(string phone, Guid marketZoneId)
        {
            User user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.PhoneNumber == phone.Trim() && x.MarketZoneId == marketZoneId,
                 include: rf => rf.Include(y => y.Wallets)
                                   .Include(y => y.Store)
                                   .Include(y => y.Role));
            return user;
        }
        public async Task<User> GetUserCCCDPassport(string cccd_passport, Guid marketZoneId)
        {
            User user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.CccdPassport == cccd_passport.Trim() && x.MarketZoneId == marketZoneId,
                 include: rf => rf.Include(y => y.Wallets)
                                   .Include(y => y.Store)
                                   .Include(y => y.Role));
            return user;
        }
        public async Task<User> GetUser(string email, Guid marketZoneId)
        {
            User user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == email.Trim() && x.MarketZoneId == marketZoneId,
                 include: rf => rf.Include(y => y.Wallets)
                                   .Include(y => y.Store)
                                   .Include(y => y.Role));
            return user;
        }
        public async Task<House> GetHouse(string location, string address)
        {
            House house = await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync
                (predicate: x => x.Location == location.Trim() && x.Address == address.Trim() && !x.Deflag);
            return house;
        }
    }
}

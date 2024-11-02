using AutoMapper;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class MarketZoneService : BaseService<MarketZoneService>, IMarketZoneService
    {
        public MarketZoneService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<MarketZoneService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateMarketZone(MarketZoneRequest request)
        {
            if(!ValidationUtils.IsPhoneNumber(request.PhoneNumber)) throw new BadHttpRequestException("Invalid phone number");
            if (!ValidationUtils.IsEmail(request.Email)) throw new BadHttpRequestException("Invalid email");
            //create admin marketZone
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FullName = "Administator",
                RoleId = Guid.Parse(EnvironmentVariableConstant.AdminId),
                Gender = (int)GenderEnum.Other,
                MarketZoneId = request.Id,
                
            };
            var marketZoneMap = _mapper.Map<MarketZone>(request);
            await _unitOfWork.GetRepository<MarketZone>().InsertAsync(marketZoneMap);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = "Create market zone successfully",
                Data = marketZoneMap
            };
        }
    }
}

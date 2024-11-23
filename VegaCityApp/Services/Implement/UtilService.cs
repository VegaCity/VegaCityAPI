using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;
using VegaCityApp.Service.Implement;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class UtilService : BaseService<UtilService>, IUtilService
    {
        public UtilService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<UtilService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<UserSession> CheckUserSession(Guid userId)
        {
            var userSession = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                (predicate: x => x.UserId == userId && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum())
                ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            return userSession;
        }
    }
}

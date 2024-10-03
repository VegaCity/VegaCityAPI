using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class WalletTypeService : BaseService<WalletTypeService>, IWalletTypeService
    {
        public WalletTypeService(
            IUnitOfWork<VegaCityAppContext> unitOfWork, 
            ILogger<WalletTypeService> logger, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateWalletType(WalletTypeRequest walletTypeRequest)
        {
            walletTypeRequest.Name = walletTypeRequest.Name.Trim();
            var newWalletType = _mapper.Map<WalletType>(walletTypeRequest);
            newWalletType.Id = Guid.NewGuid();
            newWalletType.CrDate = TimeUtils.GetCurrentSEATime();
            newWalletType.UpsDate = TimeUtils.GetCurrentSEATime();
            newWalletType.Deflag = false;
            newWalletType.MarketZoneId = Guid.Parse(EnvironmentVariableConstant.MarketZoneId);
            await _unitOfWork.GetRepository<WalletType>().InsertAsync(newWalletType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = WalletTypeMessage.CreateWalletTypeSuccessfully,
                Data = newWalletType
            }: new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.CreateWalletTypeFail
            };
        }

        public async Task<ResponseAPI> DeleteWalletType(Guid id)
        {
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == id && !x.Deflag);
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            walletType.Deflag = true;
            walletType.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<WalletType>().UpdateAsync(walletType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = WalletTypeMessage.DeleteWalletTypeSuccess
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.DeleteWalletTypeFail
            };
        }

        public async Task<IPaginate<WalletTypeResponse>> GetAllWalletType(int size, int page)
        {
            var data = await _unitOfWork.GetRepository<WalletType>().GetPagingListAsync(
                predicate: x => !x.Deflag,
                selector: z => new WalletTypeResponse
                {
                    Id = z.Id,
                    Name = z.Name,
                    Deflag = z.Deflag,
                    crDate = z.CrDate,
                    upsDate = z.UpsDate,
                    MarketZoneId = z.MarketZoneId
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name));
            return data;
        }

        public async Task<ResponseAPI> GetWalletTypeById(Guid id)
        {
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(
                predicate: x => x.Id == id && !x.Deflag,
                include: z => z.Include(a => a.StoreServices)
                                .Include(b => b.MarketZone));
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                Data = walletType
            };
        }

        public async Task<ResponseAPI> UpdateWalletType(Guid Id, UpDateWalletTypeRequest walletTypeRequest)
        {
            walletTypeRequest.Name = walletTypeRequest.Name.Trim();
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == Id && !x.Deflag);
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            walletType.Name = walletTypeRequest.Name;
            walletType.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<WalletType>().UpdateAsync(walletType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = WalletTypeMessage.UpdateWalletTypeSuccessfully,
                Data = walletType
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.UpdateWalletTypeFailed
            };
        }
    }
}

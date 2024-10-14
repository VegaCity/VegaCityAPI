using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
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

        public async Task<ResponseAPI> AddServiceStoreToWalletType(Guid id, Guid serviceStoreId)
        {
            //check wallet type
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == id && !x.Deflag);
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            //check service store
            var serviceStore = await _unitOfWork.GetRepository<Domain.Models.StoreService>().SingleOrDefaultAsync(
                predicate: x => x.Id == serviceStoreId && !x.Deflag,
                include: z => z.Include(a => a.WalletTypeStoreServiceMappings));
            if (serviceStore == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundServiceStore
                };
            }
            //add service store to wallet type
            var newMapping = new WalletTypeStoreServiceMapping
            {
                Id = Guid.NewGuid(),
                WalletTypeId = id,
                StoreServiceId = serviceStoreId,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
            };
            await _unitOfWork.GetRepository<WalletTypeStoreServiceMapping>().InsertAsync(newMapping);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = WalletTypeMessage.AddServiceStoreToWalletTypeSuccess,
                Data = serviceStore
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.AddServiceStoreToWalletTypeFail
            };
        }
        public async Task<ResponseAPI> RemoveServiceStoreToWalletType(Guid id, Guid serviceStoreId)
        {
            //check wallet type
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == id && !x.Deflag);
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            //check service store
            var serviceStore = await _unitOfWork.GetRepository<Domain.Models.StoreService>().SingleOrDefaultAsync(predicate: x => x.Id == serviceStoreId && !x.Deflag);
            if (serviceStore == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundServiceStore
                };
            }
            //remove service store to wallet type
            var mapping = await _unitOfWork.GetRepository<WalletTypeStoreServiceMapping>().SingleOrDefaultAsync(
                predicate: x => x.WalletTypeId == id && x.StoreServiceId == serviceStoreId);
            if (mapping == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundServiceStoreInWalletType
                };
            }
            _unitOfWork.GetRepository<WalletTypeStoreServiceMapping>().DeleteAsync(mapping);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = WalletTypeMessage.RemoveServiceStoreToWalletTypeSuccess,
                Data = serviceStore
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.RemoveServiceStoreToWalletTypeFail
            };
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

        public async Task<ResponseAPI<IEnumerable<WalletTypeResponse>>> GetAllWalletType(int size, int page)
        {
            try
            {
                IPaginate<WalletTypeResponse> data = await _unitOfWork.GetRepository<WalletType>().GetPagingListAsync(
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
                return new ResponseAPI<IEnumerable<WalletTypeResponse>>
                {
                    MessageResponse = WalletTypeMessage.GetWalletTypesSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = data.Items,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<WalletTypeResponse>>
                {
                    MessageResponse = WalletTypeMessage.GetWalletTypesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

        public async Task<ResponseAPI> GetWalletTypeById(Guid id)
        {
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(
                predicate: x => x.Id == id && !x.Deflag,
                include: z => z.Include(y => y.WalletTypeStoreServiceMappings));
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

        public async Task CheckExpireWallet()
        {
            var currentTime = TimeUtils.GetCurrentSEATime();
            var wallet =(List<Wallet>) await _unitOfWork.GetRepository<Wallet>().GetListAsync
                (predicate: x => x.ExpireDate < currentTime && !x.Deflag);
            if (wallet.Count > 0)
            {
                foreach (var item in wallet)
                {
                    item.Deflag = true;
                    item.UpsDate = currentTime;
                }
                _unitOfWork.GetRepository<Wallet>().UpdateRange(wallet);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}

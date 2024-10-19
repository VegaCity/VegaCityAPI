using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
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
            Guid apiKey = GetMarketZoneIdFromJwt();
            walletTypeRequest.Name = walletTypeRequest.Name.Trim();
            var newWalletType = _mapper.Map<WalletType>(walletTypeRequest);
            newWalletType.Id = Guid.NewGuid();
            newWalletType.CrDate = TimeUtils.GetCurrentSEATime();
            newWalletType.UpsDate = TimeUtils.GetCurrentSEATime();
            newWalletType.Deflag = false;
            newWalletType.MarketZoneId = apiKey;
            await _unitOfWork.GetRepository<WalletType>().InsertAsync(newWalletType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = WalletTypeMessage.CreateWalletTypeSuccessfully,
                Data = newWalletType
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.CreateWalletTypeFail
            };
        }

        public async Task<ResponseAPI> DeleteWalletType(Guid id)
        {
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync
                (predicate: x => x.Id == id && !x.Deflag,
                 include: mapping => mapping.Include(a => a.WalletTypeStoreServiceMappings));
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            if (walletType.WalletTypeStoreServiceMappings.Count > 0)
            {
                foreach (var item in walletType.WalletTypeStoreServiceMappings)
                {
                    _unitOfWork.GetRepository<WalletTypeStoreServiceMapping>().DeleteAsync(item);
                }
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
                predicate: x => !x.Deflag && x.MarketZoneId == GetMarketZoneIdFromJwt(),
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
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == Id && !x.Deflag);
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            walletType.Name = walletTypeRequest.Name != null ? walletTypeRequest.Name.Trim() : walletType.Name;
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
            var wallet = (List<Wallet>)await _unitOfWork.GetRepository<Wallet>().GetListAsync
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
        public async Task EndDayCheckWalletCashier(Guid apiKey)
        {
            var data = (List<User>)await _unitOfWork.GetRepository<User>().GetListAsync
                (predicate: x => (x.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierAppId)
                                || x.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierWebId))
                                && x.Status == (int)UserStatusEnum.Active);
            var maketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync
                        (predicate: x => x.Id == apiKey);
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == maketZone.Email && x.Status == (int)UserStatusEnum.Active,
                    include: wallet => wallet.Include(z => z.Wallets));
            foreach (var user in data)
            {
                //wallet cashier
                var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == user.Id && !x.Deflag);
                if (wallet != null)
                {
                    if (admin.Wallets.Count() > 0)
                    {
                        foreach (var item in admin.Wallets)
                        {
                            item.Balance += wallet.Balance;
                            wallet.Balance = 0;
                            item.BalanceHistory += wallet.BalanceHistory;
                            wallet.BalanceHistory = 0;
                            item.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                        }
                    }
                }
            }
            //create transaction
            foreach (var item in admin.Wallets)
            {
                var transactionBalance = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Type = TransactionType.EndDayCheckWalletCashier,
                    WalletId = item.Id,
                    Amount = item.Balance,
                    IsIncrease = true,
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    CrDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionBalance);
                var transactionBalanceHistory = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Type = TransactionType.EndDayCheckWalletCashier,
                    WalletId = item.Id,
                    Amount = item.BalanceHistory,
                    IsIncrease = true,
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    CrDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionBalanceHistory);
            }
            _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
            await _unitOfWork.CommitAsync();
        }
    }
}

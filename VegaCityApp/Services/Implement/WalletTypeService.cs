using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
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
                predicate: x => x.Id == serviceStoreId && !x.Deflag);
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
                CrDate = TimeUtils.GetCurrentSEATime()
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
                (predicate: x => x.Id == id && !x.Deflag, include: z => z.Include(a => a.WalletTypeStoreServiceMappings)
                );
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
                predicate: x => x.Id == id && !x.Deflag);
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
                (predicate: x => x.EndDate < currentTime && !x.Deflag);
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
                (predicate: x => (x.Role.Name == RoleEnum.CashierApp.GetDescriptionFromEnum()
                                || x.Role.Name == RoleEnum.CashierWeb.GetDescriptionFromEnum())
                                && x.Status == (int)UserStatusEnum.Active, include: z => z.Include(a => a.Role));
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
                            var transaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                Type = TransactionType.EndDayCheckWalletCashier,
                                WalletId = wallet.Id,
                                Amount = wallet.Balance,
                                IsIncrease = false,
                                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                Status = TransactionStatus.Success,
                                Description = "End day check wallet cashier: Balance",
                            };
                            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);
                            item.BalanceHistory += wallet.BalanceHistory;
                            wallet.BalanceHistory = 0;
                            var transactionHistory = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                Type = TransactionType.EndDayCheckWalletCashier,
                                WalletId = wallet.Id,
                                Amount = wallet.BalanceHistory,
                                IsIncrease = false,
                                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                Status = TransactionStatus.Success,
                                Description = "End day check wallet cashier: Balance History",
                            };
                            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionHistory);
                            item.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                        }
                    }
                }
            }
            _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
            await _unitOfWork.CommitAsync();
        }
        //withraw money wallet
        public async Task<ResponseAPI> RequestWithdrawMoneyWallet(Guid id, WithdrawMoneyRequest request)
        {
            if(!ValidationUtils.CheckNumber(request.Amount))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = WalletTypeMessage.AmountInvalid
                };
            }
            Transaction transaction = null;
            Guid cashierWebId = GetUserIdFromJwt();
            string role = GetRoleFromJwt();
            var cashierWeb = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Id == cashierWebId && x.Status == (int)UserStatusEnum.Active,
                 include: wl => wl.Include(a => a.Role));
            if (cashierWeb == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundCashierWeb
                };
            }
            if (cashierWeb.Role.Name != role)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Forbidden,
                    MessageResponse = WalletTypeMessage.RoleNotAllow
                };
            }
            //wallet user
            var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync
                (predicate: x => x.Id == id && !x.Deflag,
                 include: userStore => userStore.Include(z => z.User).Include(z => z.PackageItems));
            if (wallet == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWallet
                };
            }
            if(wallet.User != null)
            {
                if (wallet.User.StoreId != wallet.StoreId)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = WalletTypeMessage.NotAllowWithdraw
                    };
                }
                else if (wallet.User.StoreId == wallet.StoreId)
                {
                    if (wallet.BalanceHistory < request.Amount)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = WalletTypeMessage.NotEnoughBalance
                        };
                    }
                    transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        Type = TransactionType.WithdrawMoney,
                        WalletId = wallet.Id,
                        Amount = request.Amount, // wallet user owner store
                        IsIncrease = false,
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Status = TransactionStatus.Pending,
                        Description = "Withdraw money for owner store: " + wallet.User.FullName,
                        StoreId = wallet.User.StoreId,
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        UserId = cashierWebId
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);
                }
            }
            else if (wallet.PackageItems.Count > 0)
            {
                foreach (var item in wallet.PackageItems)
                {
                    if (item.WalletId == wallet.Id)
                    {
                        transaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            Type = TransactionType.WithdrawMoney,
                            WalletId = wallet.Id,
                            Amount = request.Amount, // wallet etag
                            IsIncrease = false,
                            Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            Status = TransactionStatus.Pending,
                            Description = "Withdraw money for package item",
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            UserId = cashierWebId
                        };
                        await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);
                    }
                    else throw new BadHttpRequestException(WalletTypeMessage.NotAllowWithdraw);
                }
            }
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = WalletTypeMessage.RequestWithdrawMoneySuccessfully,
                Data = new
                {
                    TransactionId = transaction.Id,
                    WalletId = wallet.Id,
                }
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.RequestWithdrawMoneyFail
            };
        }
        //confirm withdraw money
        public async Task<ResponseAPI> WithdrawMoneyWallet(Guid id, Guid transactionId)
        {
            Guid cashierWebId = GetUserIdFromJwt();
            string role = GetRoleFromJwt();
            var transactionAvailable = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync(predicate: x => x.Id == transactionId && x.Status == TransactionStatus.Pending);
            if(transactionAvailable == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = TransactionMessage.NotFoundTransaction,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var cashierWeb = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Id == cashierWebId && x.Status == (int)UserStatusEnum.Active,
                 include: wl => wl.Include(z => z.Wallets).Include(a => a.Role));
            if (cashierWeb == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundCashierWeb
                };
            }
            if (cashierWeb.Role.Name != role)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Forbidden,
                    MessageResponse = WalletTypeMessage.RoleNotAllow
                };
            }
            //wallet user
            var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync
                (predicate: x => x.Id == id && !x.Deflag,
                 include: userStore => userStore.Include(z => z.User).Include(z => z.PackageItems));
            if (wallet == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWallet
                };
            }
            
            if(wallet.User != null)
            {
                if (wallet.User.StoreId != wallet.StoreId)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = WalletTypeMessage.NotAllowWithdraw
                    };
                }
                else if (wallet.User.StoreId == wallet.StoreId)
                {
                    if (wallet.BalanceHistory < transactionAvailable.Amount)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = WalletTypeMessage.NotEnoughBalance
                        };
                    }
                    transactionAvailable.Status = TransactionStatus.Success;
                    transactionAvailable.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionAvailable);
                    wallet.BalanceHistory -= transactionAvailable.Amount;
                    wallet.Balance += transactionAvailable.Amount;
                    wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                }
            }
            else if (wallet.PackageItems.Count > 0)
            {
                foreach (var item in wallet.PackageItems)
                {

                    if (item.WalletId == wallet.Id)
                    {
                        transactionAvailable.Status = TransactionStatus.Success;
                        transactionAvailable.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionAvailable);
                        wallet.Balance -= transactionAvailable.Amount;
                        wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                    }
                    else
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = WalletTypeMessage.NotAllowWithdraw
                        };
                    }
                }

            }
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync
                (predicate: x => x.Id == cashierWeb.MarketZoneId);
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == marketZone.Email && x.Status == (int)UserStatusEnum.Active,
                    include: wallet => wallet.Include(z => z.Wallets));
            var transactionBalanceAdmin = new Transaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.WithdrawMoney,
                WalletId = admin.Wallets.SingleOrDefault().Id,
                Amount =  transactionAvailable.Amount, //return money from etag admin.Wallets.SingleOrDefault().Balance -
                IsIncrease = false,
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Status = TransactionStatus.Success,
                Description = "Withdraw balance from admin: " + admin.FullName,
            };
            //admin wallet
            admin.Wallets.SingleOrDefault().Balance -= transactionAvailable.Amount;
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(admin.Wallets.SingleOrDefault());
            var transactionBalanceHistory = new Transaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.WithdrawMoney,
                WalletId = cashierWeb.Wallets.SingleOrDefault().Id,
                Amount =  transactionAvailable.Amount, //cashierWeb.Wallets.SingleOrDefault().BalanceHistory +
                IsIncrease = true,
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Status = TransactionStatus.Success,
                Description = "Add balance history to cashier web: " + cashierWeb.FullName,
            };
            cashierWeb.Wallets.SingleOrDefault().BalanceHistory += transactionAvailable.Amount;
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(cashierWeb.Wallets.SingleOrDefault());
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionBalanceAdmin);
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionBalanceHistory);
            return await _unitOfWork.CommitAsync() > 0? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = WalletTypeMessage.WithdrawMoneySuccessfully,
                Data = new
                {
                    TransactionId = transactionAvailable.Id,
                    WalletId = wallet.Id,
                }
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = WalletTypeMessage.WithdrawMoneyFail
            };
        }
    }
}

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
        private readonly IUtilService _util;

        public WalletTypeService(
            IUnitOfWork<VegaCityAppContext> unitOfWork,
            ILogger<WalletTypeService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUtilService util) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _util = util;
        }
        public async Task<ResponseAPI> CreateWalletType(WalletTypeRequest walletTypeRequest)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
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
            await _util.CheckUserSession(GetUserIdFromJwt());
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync
                (predicate: x => x.Id == id && !x.Deflag, include: z => z.Include(a => a.WalletTypeMappings)
                );
            if (walletType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            if (walletType.WalletTypeMappings.Count > 0)
            {
                foreach (var item in walletType.WalletTypeMappings)
                {
                    _unitOfWork.GetRepository<WalletTypeMapping>().DeleteAsync(item);
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
            await _util.CheckUserSession(GetUserIdFromJwt());
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
            var listCashiers = (List<User>)await _unitOfWork.GetRepository<User>().GetListAsync
                (predicate: x => (x.Role.Name == RoleEnum.CashierApp.GetDescriptionFromEnum()
                                || x.Role.Name == RoleEnum.CashierWeb.GetDescriptionFromEnum())
                                && x.Status == (int)UserStatusEnum.Active, include: z => z.Include(a => a.Role));
            var maketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync
                        (predicate: x => x.Id == apiKey);
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == maketZone.Email && x.Status == (int)UserStatusEnum.Active,
                    include: wallet => wallet.Include(z => z.Wallets));
            foreach (var user in listCashiers)
            {
                //wallet cashier
                var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == user.Id && !x.Deflag);
                var transactionCheck = await _unitOfWork.GetRepository<Transaction>().GetListAsync(predicate: z => z.WalletId == wallet.Id && z.Status == TransactionStatus.Pending);
                if(transactionCheck.Count() != 0)
                {
                    if (wallet != null)
                    {

                        if (admin.Wallets.Count() > 0)
                        {
                            foreach (var item in admin.Wallets)
                            {

                                var transaction = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Type = TransactionType.EndDayCheckWalletCashier,
                                    WalletId = wallet.Id,
                                    Amount = wallet.Balance,
                                    IsIncrease = false,
                                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                                    CrDate = TimeUtils.GetCurrentSEATime(),
                                    Status = TransactionStatus.Pending,
                                    Description = "End day check wallet cashier: Balance " + wallet.Balance,
                                    UpsDate = TimeUtils.GetCurrentSEATime(),
                                    UserId = admin.Id
                                };
                                await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);
                                var transactionHistory = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Type = TransactionType.EndDayCheckWalletCashier,
                                    WalletId = wallet.Id,
                                    Amount = wallet.BalanceHistory,
                                    IsIncrease = false,
                                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                                    CrDate = TimeUtils.GetCurrentSEATime(),
                                    Status = TransactionStatus.Pending,
                                    Description = "End day check wallet cashier: Balance History " + wallet.BalanceHistory,
                                    UserId = admin.Id,
                                    UpsDate = TimeUtils.GetCurrentSEATime()
                                };
                                await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionHistory);
                                //here
                                //item.Balance += wallet.Balance; //wallet admin
                                //wallet.Balance = 0; //wallet cashier
                                //
                                //item.BalanceHistory += wallet.BalanceHistory; //wallet admin
                                //wallet.BalanceHistory = 0; //wallet cashier
                                //item.UpsDate = TimeUtils.GetCurrentSEATime();
                                //_unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                            }
                        }
                    }
                }
            }
            //_unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
            await _unitOfWork.CommitAsync();
        }
        public async Task CheckPendingEndDayCheckWalletCashier()
        {
            var transactions = await _unitOfWork.GetRepository<Transaction>().GetListAsync(
                predicate: z => z.Status == TransactionStatus.Pending && z.Type == TransactionType.EndDayCheckWalletCashier,
                include: z => z.Include(a => a.Wallet).ThenInclude(z => z.User));
            foreach(var item in transactions)
            {
                if (TimeUtils.GetCurrentSEATime().Subtract(item.CrDate).TotalHours >= 3)
                {
                    item.Wallet.User.Status = (int)UserStatusEnum.Blocked;
                    item.Wallet.User.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<User>().UpdateAsync(item.Wallet.User);
                }
            }
            await _unitOfWork.CommitAsync();
        }
        //withraw money wallet
        public async Task<ResponseAPI> RequestWithdrawMoneyWallet(Guid id, WithdrawMoneyRequest request)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            if (!ValidationUtils.CheckNumber(request.Amount))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = WalletTypeMessage.AmountInvalid
                };
            }
            //must using atleast 30% of balanceStart in order to withdraw 
            //case after charge (balance history  larger than balance start => balance se tu bao nhieu la duoc rut? )

            Transaction transaction = null;
            Guid cashierWebId = GetUserIdFromJwt();
            string role = GetRoleFromJwt();
            var cashierWeb = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Id == cashierWebId && x.Status == (int)UserStatusEnum.Active,
                 include: wl => wl.Include(a => a.Role)
                                  .Include(p => p.MarketZone).ThenInclude(m => m.MarketZoneConfig));
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
                 include: userStore => userStore.Include(z => z.User).Include(z => z.PackageOrder));
            if (wallet == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWallet
                };
            }
            if (wallet.User != null)
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
                    var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(
                         predicate: x => x.StoreId == wallet.StoreId && x.Status == OrderStatus.Completed &&
                           x.UpsDate <= TimeUtils.GetCurrentSEATime() && x.SaleType == SaleType.Product, include: z => z.Include(z => z.Payments));
                    if (orders.Count <= 0) throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
                    var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == wallet.StoreId,
                            include: z => z.Include(a => a.Zone).ThenInclude(a => a.MarketZone).ThenInclude(z => z.MarketZoneConfig));
                    List<Payment> payments = new List<Payment>(); //list payment QRCode
                    foreach (var order in orders)
                    {
                        foreach (var payment in order.Payments)
                        {
                            if (payment.Name == PaymentTypeEnum.QRCode.GetDescriptionFromEnum())
                            {
                                payments.Add(payment);
                            }
                        }
                    }
                    int AmountPayment = 0;
                    foreach (var payment in payments)
                    {
                        AmountPayment += payment.FinalAmount;
                    }
                    int AmountTransfered = (int)(AmountPayment - AmountPayment * store.StoreTransferRate);
                    if (wallet.BalanceHistory < request.Amount)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = WalletTypeMessage.NotEnoughBalance
                        };
                    }
                    if (AmountTransfered < request.Amount)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = "Amount you can withdraw is " + AmountTransfered + "VND and your request amount is not enough"
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
            else if (wallet.PackageOrder != null) //End user
            {
                if (request.Amount < 50000)
                    throw new BadHttpRequestException("User must withdraw greater than " + 50000 + "VND", HttpStatusCodes.BadRequest);

                //case * package specific
                // 150 700 
                //case ** package service
                // 160 800 700
                int amountRate = (int)(wallet.BalanceHistory * cashierWeb.MarketZone.MarketZoneConfig.WithdrawRate); // 700 * 0.8 = 560 // 700 - 560 = 140
                int atLeaseUse = amountRate - (wallet.BalanceHistory - wallet.Balance); // 560 - (700 - 150) = 10
                if (request.Amount > wallet.Balance)
                    throw new BadHttpRequestException("Balance is not enough", HttpStatusCodes.BadRequest);
                if (atLeaseUse > 0 && wallet.BalanceHistory - wallet.Balance != amountRate) // 700 - 150 != 560
                    throw new BadHttpRequestException("User must pay more " + atLeaseUse + "VND to withdraw", HttpStatusCodes.BadRequest);

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
            var session = await _util.CheckUserSession(GetUserIdFromJwt());
            Guid cashierWebId = GetUserIdFromJwt();
            string role = GetRoleFromJwt();
            var transactionAvailable = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                (predicate: x => x.Id == transactionId && x.Status == TransactionStatus.Pending);
            if (transactionAvailable == null)
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
                 include: userStore => userStore.Include(z => z.User).Include(z => z.PackageOrder));
            if (wallet == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWallet
                };
            }

            if (wallet.User != null)
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
                    wallet.Balance -= transactionAvailable.Amount;
                    wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                    session.TotalWithrawCash = transactionAvailable.Amount;
                    _unitOfWork.GetRepository<UserSession>().UpdateAsync(session);
                }
            }
            else if (wallet.PackageOrder != null)
            {
                transactionAvailable.Status = TransactionStatus.Success;
                transactionAvailable.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionAvailable);
                wallet.Balance -= transactionAvailable.Amount;
                wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                session.TotalWithrawCash = transactionAvailable.Amount;
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(session);
            }
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync
                (predicate: x => x.Id == cashierWeb.MarketZoneId);
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == marketZone.Email && x.Status == (int)UserStatusEnum.Active,
                    include: wallet => wallet.Include(z => z.Wallets));
            Wallet adminWallet = admin.Wallets.SingleOrDefault();
            string name = wallet.User != null ? wallet.User.FullName : wallet.PackageOrder.CusName;
            var transactionBalanceAdmin = new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = adminWallet.Id,
                Type = TransactionType.WithdrawMoney,
                Amount = transactionAvailable.Amount,
                IsIncrease = false,
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                UserId = admin.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = TransactionStatus.Success,
                Description = "Withdraw money for: " + name
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionBalanceAdmin);
            //admin wallet
            adminWallet.Balance -= transactionAvailable.Amount;
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(adminWallet);
            Wallet cashierWallet = cashierWeb.Wallets.SingleOrDefault();
            var transactionBalanceHistory = new Transaction
            {
                Id = Guid.NewGuid(),
                Type = TransactionType.WithdrawMoney,
                WalletId = cashierWallet.Id,
                Amount = transactionAvailable.Amount, //cashierWeb.Wallets.SingleOrDefault().BalanceHistory +
                IsIncrease = true,
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Status = TransactionStatus.Success,
                Description = "Add balance history to cashier web: " + cashierWeb.FullName,
                UpsDate = TimeUtils.GetCurrentSEATime(),
                UserId = cashierWebId
            };
            cashierWallet.BalanceHistory += transactionAvailable.Amount;
            cashierWallet.Balance -= transactionAvailable.Amount;
            cashierWallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(cashierWallet);

            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionBalanceHistory);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
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
        public async Task BalanceCheckEndDay()
        {
            var wallets = await _unitOfWork.GetRepository<Wallet>().GetListAsync(predicate: z => !z.Deflag);
            foreach (var wallet in wallets)
            {
                if(wallet.UserId != null && wallet.StoreId == null)
                {
                    BalanceEndDay newdata = new BalanceEndDay()
                    {
                        Id = Guid.NewGuid(),
                        Deflag = wallet.Deflag,
                        Balance = wallet.Balance,
                        BalanceHistory = wallet.BalanceHistory,
                        DateCheck = TimeUtils.GetCurrentSEATime(),
                        UserId = wallet.UserId
                    };
                    await _unitOfWork.GetRepository<BalanceEndDay>().InsertAsync(newdata);
                }
                else
                {
                    BalanceEndDay newdata = new BalanceEndDay()
                    {
                        Id = Guid.NewGuid(),
                        Deflag = wallet.Deflag,
                        Balance = wallet.Balance,
                        BalanceHistory = wallet.BalanceHistory,
                        DateCheck = TimeUtils.GetCurrentSEATime(),
                        UserId = wallet.UserId,
                        StoreId = wallet.StoreId
                    };
                    await _unitOfWork.GetRepository<BalanceEndDay>().InsertAsync(newdata);
                }
            }
            await _unitOfWork.CommitAsync();
        }
    }
}

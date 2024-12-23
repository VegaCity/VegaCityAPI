using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.API.Payload.Response.TransactionResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class TransactionService : BaseService<TransactionService>, ITransactionService
    {
        public TransactionService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<TransactionService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> DeleteTransaction(Guid id)
        {
            var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (transaction == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Transaction not found"
                };
            }
            _unitOfWork.GetRepository<Transaction>().DeleteAsync(transaction);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Delete transaction successfully"
            };
        }
        public async Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllTransaction(int size, int page)
        {
            try
            {
                var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                    predicate: x => x.Id == GetUserIdFromJwt(), 
                    include: y => y.Include(s => s.UserStoreMappings).ThenInclude(s => s.Store).Include(w => w.Wallets).Include(a => a.Role));
                if (user.Role.Name == RoleEnum.Admin.GetDescriptionFromEnum())
                {
                    IPaginate<TransactionResponse> data = await _unitOfWork.GetRepository<Transaction>().GetPagingListAsync(
                                selector: x => new TransactionResponse()
                                {
                                    Id = x.Id,
                                    Amount = x.Amount,
                                    Description = x.Description,
                                    CrDate = x.CrDate,
                                    Currency = x.Currency,
                                    Status = x.Status,
                                    IsIncrease = x.IsIncrease,
                                    StoreId = x.StoreId,
                                    Type = x.Type,
                                    WalletId = x.WalletId
                                },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => z.Type == TransactionType.SellingProduct || z.Type == TransactionType.SellingService || z.Type == TransactionType.TransferMoneyToVega && z.StoreId != null);
                    return new ResponseAPI<IEnumerable<TransactionResponse>>
                    {
                        MessageResponse = "Get Transactions success !!",
                        StatusCode = HttpStatusCodes.OK,
                        MetaData = new MetaData
                        {
                            Size = data.Size,
                            Page = data.Page,
                            Total = data.Total,
                            TotalPage = data.TotalPages
                        },
                        Data = data.Items
                    };
                }
                else if (user.Role.Name == RoleEnum.Store.GetDescriptionFromEnum())
                {
                    int? storeType = user.UserStoreMappings.SingleOrDefault().Store.StoreType;

                    IPaginate<TransactionResponse> data = await _unitOfWork.GetRepository<Transaction>().GetPagingListAsync(
                                selector: x => new TransactionResponse()
                                {
                                    Id = x.Id,
                                    Amount = x.Amount,
                                    Description = x.Description,
                                    CrDate = x.CrDate,
                                    Currency = x.Currency,
                                    Status = x.Status,
                                    IsIncrease = x.IsIncrease,
                                    StoreId = x.StoreId,
                                    Type = x.Type,
                                    WalletId = x.WalletId
                                },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => ((storeType == (int)StoreTypeEnum.Service
                              ? z.Type == TransactionType.SellingService
                              : z.Type == TransactionType.SellingProduct)
                              || z.Type == TransactionType.TransferMoneyToStore)
                              && z.StoreId == user.StoreId);
                    return new ResponseAPI<IEnumerable<TransactionResponse>>
                    {
                        MessageResponse = "Get Transactions success !!",
                        StatusCode = HttpStatusCodes.OK,
                        MetaData = new MetaData
                        {
                            Size = data.Size,
                            Page = data.Page,
                            Total = data.Total,
                            TotalPage = data.TotalPages
                        },
                        Data = data.Items
                    };
                }
                else if (user.Role.Name == RoleEnum.CashierWeb.GetDescriptionFromEnum())
                {
                    IPaginate<TransactionResponse> data = await _unitOfWork.GetRepository<Transaction>().GetPagingListAsync(
                                selector: x => new TransactionResponse()
                                {
                                    Id = x.Id,
                                    Amount = x.Amount,
                                    Description = x.Description,
                                    CrDate = x.CrDate,
                                    Currency = x.Currency,
                                    Status = x.Status,
                                    IsIncrease = x.IsIncrease,
                                    StoreId = x.StoreId,
                                    Type = x.Type,
                                    WalletId = x.WalletId
                                },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => z.Type == TransactionType.SellingPackage 
                                             || z.Type == TransactionType.ReceiveMoneyToCashier 
                                             || z.Type == TransactionType.RefundMoney 
                                             || z.Type == TransactionType.ChargeMoney 
                                             || z.Type == TransactionType.EndDayCheckWalletCashierBalance
                                             || z.Type == TransactionType.EndDayCheckWalletCashierBalanceHistory
                                             || z.Type == TransactionType.WithdrawMoney
                                             && z.StoreId != null);
                    return new ResponseAPI<IEnumerable<TransactionResponse>>
                    {
                        MessageResponse = "Get Transactions success !!",
                        StatusCode = HttpStatusCodes.OK,
                        MetaData = new MetaData
                        {
                            Size = data.Size,
                            Page = data.Page,
                            Total = data.Total,
                            TotalPage = data.TotalPages
                        },
                        Data = data.Items
                    };
                }
                else throw new BadHttpRequestException("aaa");
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<TransactionResponse>>
                {
                    MessageResponse = "Get Transactions fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> GetTransactionById(Guid id)
        {
            var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync(
                predicate: x => x.Id == id
                );
            if (transaction == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Transaction not found"
                };
            }
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Get transaction successfully",
                Data = transaction
            };
        }
        public async Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllTransactionByStoreId(Guid storeId, string type, int size, int page)
        {
            try
            {
                IPaginate<TransactionResponse> data = await _unitOfWork.GetRepository<Transaction>().GetPagingListAsync(
                               selector: x => new TransactionResponse()
                               {
                                   Id = x.Id,
                                   Amount = x.Amount,
                                   Description = x.Description,
                                   CrDate = x.CrDate,
                                   Currency = x.Currency,
                                   Status = x.Status,
                                   IsIncrease = x.IsIncrease,
                                   StoreId = x.StoreId,
                                   Type = x.Type,
                                   WalletId = x.WalletId
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => z.StoreId == storeId && z.Type == type);
                return new ResponseAPI<IEnumerable<TransactionResponse>>
                {
                    MessageResponse = "Get Transactions success !!",
                    StatusCode = HttpStatusCodes.OK,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<TransactionResponse>>
                {
                    MessageResponse = "Get Transactions fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<StoreMoneyTransferRes>>> GetAllStoreMoneyTransfer(Guid storeId, int size, int page)
        {
            try
            {
                IPaginate<StoreMoneyTransferRes> data = await _unitOfWork.GetRepository<StoreMoneyTransfer>().GetPagingListAsync(
                               selector: x => new StoreMoneyTransferRes()
                               {
                                   Id = x.Id,
                                   Amount = x.Amount,
                                   Description = x.Description,
                                   CrDate = x.CrDate,
                                   Status = x.Status,
                                   IsIncrease = x.IsIncrease,
                                   StoreId = x.StoreId,
                                   MarketZoneId = x.MarketZoneId,
                                   TransactionId = x.TransactionId,
                                   UpsDate = x.UpsDate
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => z.StoreId == storeId);
                return new ResponseAPI<IEnumerable<StoreMoneyTransferRes>>
                {
                    MessageResponse = "Get Transactions success !!",
                    StatusCode = HttpStatusCodes.OK,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<StoreMoneyTransferRes>>
                {
                    MessageResponse = "Get Transactions fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<CustomerMoneyTransferRes>>> GetAllCustomerMoneyTransfer(Guid PackageOrderId, int size, int page)
        {
            try
            {
                IPaginate<CustomerMoneyTransferRes> data = await _unitOfWork.GetRepository<CustomerMoneyTransfer>().GetPagingListAsync(
                               selector: x => new CustomerMoneyTransferRes()
                               {
                                   Id = x.Id,
                                   Amount = x.Amount,
                                   CrDate = x.CrDate,
                                   Status = x.Status,
                                   IsIncrease = x.IsIncrease,
                                   MarketZoneId = x.MarketZoneId,
                                   PackageOrderId = x.PackageOrderId,
                                   TransactionId = x.TransactionId,
                                   UpsDate = x.UpsDate
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => z.PackageOrderId == PackageOrderId);
                return new ResponseAPI<IEnumerable<CustomerMoneyTransferRes>>
                {
                    MessageResponse = "Get Transactions success !!",
                    StatusCode = HttpStatusCodes.OK,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<CustomerMoneyTransferRes>>
                {
                    MessageResponse = "Get Transactions fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllCustomerMoneyTransaction(Guid PackageOrderId, int size, int page)
        {
            try
            {
                var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync(predicate: x => x.PackageOrderId == PackageOrderId);
                if (wallet == null)
                    throw new BadHttpRequestException("Not found Wallet", HttpStatusCodes.NotFound);
                    IPaginate<TransactionResponse> data = await _unitOfWork.GetRepository<Transaction>().GetPagingListAsync(
                               selector: x => new TransactionResponse()
                               {
                                   Id = x.Id,
                                   Amount = x.Amount,
                                   Description = x.Description,
                                   CrDate = x.CrDate,
                                   Currency = x.Currency,
                                   Status = x.Status,
                                   IsIncrease = x.IsIncrease,
                                   StoreId = x.StoreId,
                                   Type = x.Type,
                                   WalletId = x.WalletId
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate),
                                predicate: z => z.WalletId == wallet.Id );
                    return new ResponseAPI<IEnumerable<TransactionResponse>>
                    {
                        MessageResponse = "Get Transactions success !!",
                        StatusCode = HttpStatusCodes.OK,
                        MetaData = new MetaData
                        {
                            Size = data.Size,
                            Page = data.Page,
                            Total = data.Total,
                            TotalPage = data.TotalPages
                        },
                        Data = data.Items
                    };
                

            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<TransactionResponse>>
                {
                    MessageResponse = "Get Transactions fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

        public async Task CheckTransactionPending()
        {
            var transactions = await _unitOfWork.GetRepository<Transaction>().GetListAsync
                (predicate: x => x.Status == TransactionStatus.Pending);
            foreach (var transaction in transactions)
            {
                if (transactions.Any(t => t.Description.Contains("Charge Fee From Lost PackageItem:")))
                {
                    continue;
                }
                if (TimeUtils.GetCurrentSEATime().Subtract(transaction.CrDate).TotalMinutes > 5)
                {
                    transaction.Status = TransactionStatus.Fail;
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);
                }
            }
            await _unitOfWork.CommitAsync();
        }

        public async Task<ResponseAPI> GetTransactionComponents(Guid TransactionId)
        {
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Get Transaction Components Successfully!",
                Data = { }
            };
        }
    }
}

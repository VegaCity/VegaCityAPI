using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;
using static VegaCityApp.API.Utils.PasswordUtil;

namespace VegaCityApp.API.Services.Implement
{
    public class PackageService : BaseService<PackageService>, IPackageService
    {
        private IUtilService _util;
        public PackageService(IUnitOfWork<VegaCityAppContext> unitOfWork,
                              ILogger<PackageService> logger,
                              IHttpContextAccessor httpContextAccessor,
                              IMapper mapper,
                              IUtilService util) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _util = util;
        }

        #region CRUD Package
        public async Task<ResponseAPI> CreatePackage(CreatePackageRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var packageExist = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name && !x.Deflag);
            if (packageExist != null)
                throw new BadHttpRequestException(PackageMessage.ExistedPackageName, HttpStatusCodes.BadRequest);
            if (req.Price <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            if (req.MoneyStart <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            if (req.Duration <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            if (!EnumUtil.ParseEnum<PackageTypeEnum>(req.Type).Equals(PackageTypeEnum.SpecificPackage)
                && !EnumUtil.ParseEnum<PackageTypeEnum>(req.Type).Equals(PackageTypeEnum.ServicePackage))
                throw new BadHttpRequestException(PackageMessage.InvalidPackageType, HttpStatusCodes.BadRequest);
            var checkZone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Id == req.ZoneId)
                ?? throw new BadHttpRequestException(ZoneMessage.SearchZoneFail, HttpStatusCodes.NotFound);
            var checkWalletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == req.WalletTypeId)
                ?? throw new BadHttpRequestException(WalletTypeMessage.NotFoundWalletType, HttpStatusCodes.NotFound);
            //-------------------------------------
            var newPackage = new Package()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Deflag = false,
                Duration = req.Duration,
                Type = req.Type,
                ImageUrl = req.ImageUrl,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                ZoneId = req.ZoneId
            };
            await _unitOfWork.GetRepository<Package>().InsertAsync(newPackage);
            var newPackageDetail = new PackageDetail()
            {
                Id = Guid.NewGuid(),
                PackageId = newPackage.Id,
                StartMoney = req.MoneyStart,
                WalletTypeId = req.WalletTypeId,
                CrDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<PackageDetail>().InsertAsync(newPackageDetail);
            var response = new ResponseAPI()
            {
                MessageResponse = PackageMessage.CreatePackageSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    PackageId = newPackage.Id
                }

            };
            int check = await _unitOfWork.CommitAsync();

            return check > 0 ? response : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = PackageMessage.CreatePackageFail
            };
        }
        public async Task<ResponseAPI> UpdatePackage(Guid packageId, UpdatePackageRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == packageId && !x.Deflag);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageMessage.NotFoundPackage
                };
            }
            package.Name = req.Name != null ? req.Name.Trim() : package.Name;
            package.Description = req.Description != null ? req.Description.Trim() : package.Description;
            package.Price = req.Price ?? package.Price;
            package.Duration = req.Duration ?? package.Duration;
            package.ImageUrl = req.ImageUrl != null ? req.ImageUrl.Trim() : package.ImageUrl;
            package.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.UpdatePackageSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageId = package.Id
                    }
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.UpdatePackageFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<GetPackageResponse>>> SearchAllPackage(int size, int page)
        {
            try
            {
                var userInZone = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                    (predicate: x => x.Id == GetUserIdFromJwt() && x.Status == (int)UserStatusEnum.Active, include: x => x.Include(z => z.UserSessions))
                    ?? throw new BadHttpRequestException(UserMessage.NotFoundUser, HttpStatusCodes.NotFound);
                Guid zoneId = Guid.Empty;
                foreach (var item in userInZone.UserSessions)
                {
                    if (item.Status == SessionStatusEnum.Active.GetDescriptionFromEnum())
                    {
                        zoneId = item.ZoneId;
                    }
                }
                IPaginate<GetPackageResponse> data = await _unitOfWork.GetRepository<Package>().GetPagingListAsync(

                selector: x => new GetPackageResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    ImageUrl = x.ImageUrl,
                    Duration = x.Duration,
                    Type = x.Type,
                    ZoneId = x.ZoneId
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Deflag == false && x.ZoneId == zoneId,
                include: x => x.Include(a => a.Zone)
                );
                return new ResponseAPI<IEnumerable<GetPackageResponse>>
                {
                    MessageResponse = PackageMessage.GetPackagesSuccessfully,
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
                return new ResponseAPI<IEnumerable<GetPackageResponse>>
                {
                    MessageResponse = PackageMessage.GetPackagesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> DeletePackage(Guid PackageId)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                (predicate: x => x.Id == PackageId);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageMessage.NotFoundPackage
                };
            }
            package.Deflag = true;
            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = PackageMessage.DeleteSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageId = package.Id
                    }
                }
                : new ResponseAPI()
                {
                    MessageResponse = PackageMessage.DeleteFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        public async Task<ResponseAPI<Package>> SearchPackage(Guid? PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageId && x.Deflag == false,
                include: package => package.Include(a => a.PackageDetails).ThenInclude(w => w.WalletType)
                                           .Include(b => b.PackageOrders)
            ) ?? throw new BadHttpRequestException(PackageMessage.NotFoundPackage, HttpStatusCodes.NotFound);

            return new ResponseAPI<Package>()
            {
                MessageResponse = PackageMessage.GetPackagesSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = package

            };
        }
        #endregion
        #region CRUD Package Order - VCard
        public async Task<ResponseAPI> CreatePackageItem(int quantity, CreatePackageItemRequest req)
        {
            if (quantity <= 0) throw new BadHttpRequestException("Number Quantity must be more than 0", HttpStatusCodes.BadRequest);
            #region getlost, child
            if (req.packageOrderId != null)
            {
                //case parent + lost
                var packageOrderExist = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync
                    (predicate: x => x.Id == req.packageOrderId && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(),
                    include: y => y.Include(t => t.Package)
                                   .Include(w => w.Wallets).ThenInclude(t => t.WalletType)
                                   .Include(tr => tr.Wallets).ThenInclude(t => t.Transactions)); //may defect
                if (packageOrderExist.Status == PackageItemStatusEnum.InActive.GetDescriptionFromEnum())
                    throw new BadHttpRequestException(PackageItemMessage.MustActivated, HttpStatusCodes.NotFound);

                var package = await SearchPackage(packageOrderExist.Package.Id);
                #region check 
                // after done main flow, make utils service to shorten this code
                var userId = GetUserIdFromJwt();
                //walet
                var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync(predicate: x => x.UserId == userId);
                if (wallet == null)
                {
                    throw new BadHttpRequestException(WalletTypeMessage.NotFoundWallet, HttpStatusCodes.NotFound);
                }
                var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                    predicate: x => x.UserId == userId && x.ZoneId == package.Data.ZoneId
                                   && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                                   && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
                )
                    ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                                HttpStatusCodes.BadRequest);
                #endregion
                List<GetListPackageItemResponse> packageItems = new List<GetListPackageItemResponse>();

                if (packageOrderExist.EndDate <= TimeUtils.GetCurrentSEATime())
                    throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.NotFound);

                var walletTransaction = packageOrderExist.Wallets.SingleOrDefault().Transactions;
                var hasSuccessTransaction = walletTransaction
                    .Any(t => t.Status == TransactionStatus.Success.GetDescriptionFromEnum() && t.OrderId != null);

                var hasPendingTransaction = walletTransaction
                    .Any(t => t.Status == TransactionStatus.Pending.GetDescriptionFromEnum() && t.OrderId == null);

                var hasPendingOrderCharge = packageOrderExist.Orders
                    .Any(t => t.Status == OrderStatus.Pending.GetDescriptionFromEnum() && packageOrderExist.Orders.SingleOrDefault().SaleType == SaleType.FeeChargeCreate);
                if (packageOrderExist.Status == PackageItemStatusEnum.Blocked.GetDescriptionFromEnum() && hasPendingTransaction)
                {
                    //after run api to get by cccd and mark as Blocked, transfer money, check log transaction
                    //case lost vcard
                    //check cus transfer to create new card
                    //co deposit
                    //phai dc active luon
                    if (quantity > 1)

                        throw new BadHttpRequestException(PackageItemMessage.OneAsATime, HttpStatusCodes.NotFound);

                    if (hasPendingTransaction)
                    {
                        //New CHARGE ORDER OPEN CARD FEE if balance <50
                        if (packageOrderExist.Wallets.SingleOrDefault().Balance > 50000)
                        {
                            var newChargeFeeOderPAID = new Order
                            {
                                Id = Guid.NewGuid(),
                                //PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
                                Name = "Order Charge Fee Create Card Of " + packageOrderExist.CusName,
                                TotalAmount = 50000,
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                Status = OrderStatus.Completed,
                                InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                                StoreId = null,
                                PackageOrderId = packageOrderExist.Id,
                                PackageId = packageOrderExist.PackageId,
                                UserId = userId,
                                SaleType = SaleType.FeeChargeCreate,
                            };
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newChargeFeeOderPAID);

                            //new payment 
                            var newPayment = new Payment()
                            {
                                Id = Guid.NewGuid(),
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                FinalAmount = 50000,
                                Name = "CardPayment",
                                OrderId = newChargeFeeOderPAID.Id,
                                Status = PaymentStatus.Completed,
                            };
                            await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                            //update balance
                            packageOrderExist.Wallets.SingleOrDefault().Balance -= 50000;
                            packageOrderExist.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                            packageOrderExist.Wallets.SingleOrDefault().Deflag = false;
                            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderExist.Wallets.SingleOrDefault());

                            ////UPDATE CASHIER WALLET
                            wallet.Balance += 50000;
                            wallet.BalanceHistory += 50000;
                            wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);

                            //session update
                            session.TotalQuantityOrder += 1;
                            session.TotalCashReceive += 50000;
                            session.TotalFinalAmountOrder += 50000;
                            _unitOfWork.GetRepository<UserSession>().UpdateAsync(session);

                            var transactionIds = packageOrderExist.Wallets.SingleOrDefault().Transactions;
                            var transaction = transactionIds.SingleOrDefault(predicate: x => x.OrderId == null && x.Status == "Pending");
                            transaction.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                            transaction.OrderId = newChargeFeeOderPAID.Id;
                            _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);

                            //CustomerMoneyTransfer
                            var newDepositII = new CustomerMoneyTransfer()
                            {
                                Id = Guid.NewGuid(),
                                Amount = (int)packageOrderExist.Wallets.SingleOrDefault().Balance,
                                IsIncrease = true,
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                                PackageOrderId = packageOrderExist.Id,
                                Status = PaymentStatus.Completed,
                                TransactionId = transaction.Id
                            };
                            await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(newDepositII);

                            packageOrderExist.Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum();
                            packageOrderExist.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrderExist);

                            if (await _unitOfWork.CommitAsync() > 0)
                            {
                                return new ResponseAPI()
                                {
                                    MessageResponse = PackageItemMessage.SuccessGenerateNewPAID,
                                    StatusCode = HttpStatusCodes.OK,
                                    Data = new
                                    {
                                        PackageOrderId = packageOrderExist.Id,
                                    }
                                };
                            }
                            else
                            {
                                return new ResponseAPI()
                                {
                                    MessageResponse = PackageItemMessage.FailedToGenerateNew,
                                    StatusCode = HttpStatusCodes.BadRequest
                                };
                            }
                        }
                        else
                        { //CASE NOT ENOUGH
                            if (hasPendingTransaction && hasPendingOrderCharge)
                            {
                                throw new BadHttpRequestException(PackageItemMessage.orderUNPAID, HttpStatusCodes.BadRequest);

                            }
                            packageOrderExist.Status = PackageItemStatusEnum.InActive.GetDescriptionFromEnum();
                            packageOrderExist.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrderExist);
                            var newChargeFeeOder = new Order
                            {
                                Id = Guid.NewGuid(),
                                //PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
                                Name = "Order Charge Fee Create Card Of " + packageOrderExist.CusName,
                                TotalAmount = 50000,
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                Status = OrderStatus.Pending,
                                InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                                StoreId = null,
                                PackageOrderId = packageOrderExist.Id,
                                PackageId = packageOrderExist.PackageId,
                                UserId = userId,
                                SaleType = SaleType.FeeChargeCreate,
                            };
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newChargeFeeOder);
                            //new payment 
                            var newPayment = new Payment()
                            {
                                Id = Guid.NewGuid(),
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                FinalAmount = 50000,
                                Name = "CardPayment",
                                OrderId = newChargeFeeOder.Id,
                                Status = PaymentStatus.Pending
                            };
                            await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                            var pendingTransaction = packageOrderExist.Wallets.SingleOrDefault().Transactions
                                 .FirstOrDefault(t => t.Status == TransactionStatus.Pending.GetDescriptionFromEnum());

                            if (hasPendingTransaction)
                            {
                                var pendingTransactionId = pendingTransaction.Id;
                                pendingTransaction.OrderId = newChargeFeeOder.Id;
                                //pendingTransaction.DespositId = newDepositII.Id;
                                _unitOfWork.GetRepository<Transaction>().UpdateAsync(pendingTransaction);
                                //CustomerMoneyTransfer
                                var newDepositII = new CustomerMoneyTransfer()
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = (int)packageOrderExist.Wallets.SingleOrDefault().Balance,
                                    IsIncrease = true,
                                    CrDate = TimeUtils.GetCurrentSEATime(),
                                    UpsDate = TimeUtils.GetCurrentSEATime(),
                                    MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                                    PackageOrderId = packageOrderExist.Id,
                                    Status = PaymentStatus.Completed,
                                    TransactionId = pendingTransactionId
                                };
                                await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(newDepositII);

                                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                                {
                                    MessageResponse = PackageItemMessage.SuccessGenerateNewUNPAID,
                                    StatusCode = HttpStatusCodes.OK,
                                    Data = new
                                    {
                                        PackageOrderId = packageOrderExist.Id,
                                        InvoiceId = newChargeFeeOder.InvoiceId,
                                        TransactionId = pendingTransactionId
                                    }
                                } : new ResponseAPI()
                                {
                                    MessageResponse = PackageItemMessage.FailedToGenerateNew,
                                    StatusCode = HttpStatusCodes.BadRequest
                                };
                            }

                        }
                    }
                    else if (packageOrderExist.Wallets.SingleOrDefault().Transactions.Any(t => t.Status == TransactionStatus.Success.GetDescriptionFromEnum()
                    && t.OrderId != null))
                    {
                        throw new BadHttpRequestException(PackageItemMessage.RequestPAID, HttpStatusCodes.BadRequest);
                    }
                }
                //case generate vcard child
                else// for in here
                {
                    if (packageOrderExist.IsAdult == false)
                    {
                        throw new BadHttpRequestException(PackageItemMessage.NotAdult, HttpStatusCodes.BadRequest);
                    }
                    if (req.CusName == null)
                        throw new BadHttpRequestException("Name and PhoneNumber should not be null", HttpStatusCodes.BadRequest);
                    List<GetListPackageItemResponse> packageOrders = new List<GetListPackageItemResponse>();
                    for (var i = 0; i < quantity; i++)
                    {
                        var newPackageOrder = new PackageOrder()
                        {
                            Id = Guid.NewGuid(),
                            PackageId = packageOrderExist.PackageId,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Status = PackageItemStatusEnum.InActive.GetDescriptionFromEnum(),
                            CusName = req.CusName,
                            CusEmail = packageOrderExist.CusEmail,
                            CusCccdpassport = packageOrderExist.CusCccdpassport,
                            PhoneNumber = packageOrderExist.PhoneNumber,
                            IsAdult = false,
                            CustomerId = packageOrderExist.CustomerId,
                        };
                        packageOrders.Add(_mapper.Map<GetListPackageItemResponse>(newPackageOrder));
                        await _unitOfWork.GetRepository<PackageOrder>().InsertAsync(newPackageOrder);
                        var newWallet = new Wallet()
                        {
                            Id = Guid.NewGuid(),
                            Balance = package.Data.PackageDetails.SingleOrDefault().StartMoney,
                            BalanceHistory = package.Data.PackageDetails.SingleOrDefault().StartMoney,
                            BalanceStart = package.Data.PackageDetails.SingleOrDefault().StartMoney,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Deflag = false,
                            WalletTypeId = package.Data.PackageDetails.SingleOrDefault().WalletTypeId,
                            Name = req.CusName,
                            PackageOrderId = newPackageOrder.Id
                        };
                        await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
                    }
                    await _unitOfWork.CommitAsync();
                    return new ResponseAPI()
                    {
                        MessageResponse = PackageItemMessage.CreatePackageItemSuccessfully,
                        StatusCode = HttpStatusCodes.Created,
                        ParentName = packageOrderExist.CusName,
                        Data = packageOrders,
                    };
                }
                // }
                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.CreatePackageItemSuccessfully,
                    StatusCode = HttpStatusCodes.Created,
                    ParentName = packageOrderExist.CusName,
                    Data = packageItems,
                };
            }
            #endregion
            if (req.PackageId != null)
            {
                var package = await SearchPackage(req.PackageId);
                #region check 
                // after done main flow, make utils service to shorten this code
                var userId = GetUserIdFromJwt();
                var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                    predicate: x => x.UserId == userId && x.ZoneId == package.Data.ZoneId
                                   && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                                   && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
                )
                    ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                                HttpStatusCodes.BadRequest);
                #endregion
                List<GetListPackageItemResponse> packageOrders = new List<GetListPackageItemResponse>();
                var customer = await _unitOfWork.GetRepository<Customer>()
                     .SingleOrDefaultAsync(predicate: x => x.Email == req.CusEmail
                                                   && x.Cccdpassport == req.CusCccdpassport
                                                   && x.FullName == req.CusName
                                                   && x.PhoneNumber == req.PhoneNumber);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        Cccdpassport = req.CusCccdpassport,
                        Email = req.CusEmail,
                        FullName = req.CusName,
                        PhoneNumber = req.PhoneNumber
                    };
                    await _unitOfWork.GetRepository<Customer>().InsertAsync(customer);
                }
                for (var i = 0; i < quantity; i++)
                {
                    var newPackageOrder = new PackageOrder()
                    {
                        Id = Guid.NewGuid(),
                        PackageId = req.PackageId,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Status = PackageItemStatusEnum.InActive.GetDescriptionFromEnum(),
                        CusName = quantity == 1 ? req.CusName : "New User" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                        CusEmail = quantity == 1 ? req.CusEmail : "Empty",
                        CusCccdpassport = quantity == 1 ? req.CusCccdpassport : "Empty",
                        PhoneNumber = quantity == 1 ? req.PhoneNumber : "0000000000",
                        IsAdult = true,
                        CustomerId = customer.Id,
                    };
                    packageOrders.Add(_mapper.Map<GetListPackageItemResponse>(newPackageOrder));
                    await _unitOfWork.GetRepository<PackageOrder>().InsertAsync(newPackageOrder);
                    var newWallet = new Wallet()
                    {
                        Id = Guid.NewGuid(),
                        Balance = package.Data.PackageDetails.SingleOrDefault().StartMoney,
                        BalanceHistory = package.Data.PackageDetails.SingleOrDefault().StartMoney,
                        BalanceStart = package.Data.PackageDetails.SingleOrDefault().StartMoney,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Deflag = false,
                        WalletTypeId = package.Data.PackageDetails.SingleOrDefault().WalletTypeId,
                        Name = req.CusName,
                        PackageOrderId = newPackageOrder.Id
                    };
                    await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
                }
                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.CreatePackageItemSuccessfully,
                    StatusCode = HttpStatusCodes.Created,
                    Data = packageOrders
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.CreatePackageItemFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI<IEnumerable<GetListPackageItemResponse>>> SearchAllPackageItem(int size, int page)
        {
            try
            {
                IPaginate<GetListPackageItemResponse> data = await _unitOfWork.GetRepository<PackageOrder>().GetPagingListAsync(

                selector: x => new GetListPackageItemResponse()
                {
                    Id = x.Id,
                    PackageId = x.PackageId,
                    CrDate = x.CrDate,
                    CusCccdpassport = x.CusCccdpassport,
                    CusEmail = x.CusEmail,
                    CusName = x.CusName,
                    VcardId = x.VcardId,
                    EndDate = x.EndDate,
                    PhoneNumber = x.PhoneNumber,
                    StartDate = x.StartDate,
                    Status = x.Status,
                    UpsDate = x.UpsDate,
                    IsAdult = x.IsAdult,
                    WalletTypeName = x.Wallets.SingleOrDefault().WalletType.Name
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.CusName),
                predicate: x => x.Package.Zone.MarketZoneId == GetMarketZoneIdFromJwt(),
                include: x => x.Include(a => a.Package).ThenInclude(c => c.Zone).Include(b => b.Wallets).ThenInclude(t => t.WalletType)
                );
                return new ResponseAPI<IEnumerable<GetListPackageItemResponse>>
                {
                    MessageResponse = PackageItemMessage.GetPackageItemsSuccessfully,
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
                return new ResponseAPI<IEnumerable<GetListPackageItemResponse>>
                {
                    MessageResponse = PackageItemMessage.GetPackageItemsFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }

        }
        public async Task<ResponseAPI<PackageOrder>> SearchPackageItem(Guid? PackageOrderId, string? rfid)
        {
            PackageOrder packageOrder;
            if (PackageOrderId != null)
            {
                packageOrder = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync(
                    predicate: x => x.Id == PackageOrderId,
                    include: z => z.Include(a => a.Package)
                                   .Include(a => a.Vcard)
                                   .Include(a => a.Wallets).ThenInclude(a => a.WalletType)
                                   .Include(c => c.CustomerMoneyTransfers)
                );
            }
            else
            {
                packageOrder = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync(
                    predicate: x => x.VcardId == rfid,
                    include: z => z.Include(a => a.Package)
                                   .Include(a => a.Vcard)
                                   .Include(a => a.Wallets).ThenInclude(a => a.WalletType)
                                   .Include(c => c.CustomerMoneyTransfers)
                );
            }

            if (packageOrder == null)
            {
                return new ResponseAPI<PackageOrder>()
                {
                    MessageResponse = PackageItemMessage.NotFoundPackageItem,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            var packageItemParent = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync(
                predicate: x => x.CusCccdpassport == packageOrder.CusCccdpassport && x.IsAdult == true
            );

            var qrCodeString = EnCodeBase64.EncodeBase64Etag("https://vegacity.id.vn/etagEdit/" + packageOrder.Id);

            return new ResponseAPI<PackageOrder>()
            {
                MessageResponse = PackageItemMessage.GetPackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                ParentName = packageItemParent?.CusName,
                QRCode = qrCodeString,
                Data = packageOrder
            };
        }
        public async Task<ResponseAPI> UpdatePackageItem(Guid packageOrderId, UpdatePackageItemRequest req)
        {
            var packageOrder = await SearchPackageItem(packageOrderId, null);
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageOrder.Data.Package.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            //update
            packageOrder.Data.CusName = req.CusName != null ? req.CusName.Trim() : packageOrder.Data.CusName;
            packageOrder.Data.Status = req.Status != null ? req.Status.Trim() : packageOrder.Data.Status;
            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder.Data);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { packageOrderId = packageOrder.Data.Id }
            } : new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> UpdateRfIdPackageItem(Guid packageOrderId, string rfId)
        {
            var packageOrder = await SearchPackageItem(packageOrderId, rfId);

            if (packageOrder.Data.Status != PackageItemStatusEnum.Active.GetDescriptionFromEnum())
                throw new BadHttpRequestException(PackageItemMessage.MustActivated, HttpStatusCodes.BadRequest);
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageOrder.Data.Package.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion

            if (packageOrder.Data.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum() && packageOrder.Data.VcardId == rfId)
                throw new BadHttpRequestException(PackageItemMessage.RfIdExist, HttpStatusCodes.BadRequest);
            var packageOrdersVCardId = await _unitOfWork.GetRepository<PackageOrder>().GetListAsync(predicate: x => x.VcardId == rfId);
            var packageOrderVCardIdExisted = packageOrdersVCardId.SingleOrDefault(
                x => x.VcardId == rfId
               );
            var newVCard = new Vcard
            {
                Id = rfId,
                Cccdpassport = packageOrder.Data.CusCccdpassport,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum(),
                Email = packageOrder.Data.CusEmail,
                EndDate = packageOrder.Data.EndDate,
                ImageUrl = packageOrder.Data.Package.ImageUrl,
                Name = packageOrder.Data.CusName,
                PhoneNumber = packageOrder.Data.PhoneNumber,
                IsAdult = packageOrder.Data.IsAdult,
                StartDate = packageOrder.Data.StartDate
            };
            await _unitOfWork.GetRepository<Vcard>().InsertAsync(newVCard);

            packageOrder.Data.VcardId = newVCard.Id;
            packageOrder.Data.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder.Data);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { PackageItemId = packageOrder.Data.Id }
            } : new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> ActivePackageItem(Guid packageOrderId, CustomerInfo req)
        {
            var packageOrderExist = await SearchPackageItem(packageOrderId, null);
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageOrderExist.Data.Package.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            //vcard child 
            if (packageOrderExist.Data.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum())
            {
                throw new BadHttpRequestException(PackageItemMessage.AlreadyActivated, HttpStatusCodes.BadRequest);
            }
            packageOrderExist.Data.CusName = req.FullName;
            packageOrderExist.Data.CusEmail = req.Email;
            packageOrderExist.Data.CusCccdpassport = req.CccdPassport;
            packageOrderExist.Data.PhoneNumber = req.PhoneNumber;
            packageOrderExist.Data.Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum();
            packageOrderExist.Data.StartDate = TimeUtils.GetCurrentSEATime();
            packageOrderExist.Data.EndDate = TimeUtils.GetCurrentSEATime().AddDays(packageOrderExist.Data.Package.Duration);
            packageOrderExist.Data.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrderExist.Data);
            //update wallet
            var wallet = packageOrderExist.Data.Wallets.SingleOrDefault()
                ?? throw new BadHttpRequestException(WalletTypeMessage.NotFoundWallet, HttpStatusCodes.NotFound);
            wallet.Name = req.FullName;
            wallet.StartDate = TimeUtils.GetCurrentSEATime();
            wallet.EndDate = TimeUtils.GetCurrentSEATime().AddDays(packageOrderExist.Data.Package.Duration);
            wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
            int check = await _unitOfWork.CommitAsync();
            if (check > 0)
            {
                //send email
                if (packageOrderExist.Data.CusEmail != null)
                {
                    try
                    {
                        string subject = "Activate VCard Vega City Successfully";
                        string body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                            <div style='background-color: #007bff; color: white; padding: 20px; text-align: center;'>
                                <h1 style='margin: 0;'>Welcome to our Vega City!</h1>
                            </div>
                            <div style='padding: 20px; background-color: #f9f9f9;'>
                                <p style='font-size: 16px; color: #333;'>Hello, <strong>{packageOrderExist.Data.CusName}</strong>,</p>
                                <p style='font-size: 16px; color: #333; line-height: 1.5;'>
                                    Thanks for signing up our service. We are happy to accompany you on the upcoming journey.
                                </p>
                                <p style='font-size: 16px; color: #333; line-height: 1.5;'>
                                    Please access link to know more about us <a href='https://vega-city-landing-page.vercel.app/' style='color: #007bff; text-decoration: none;'>our website</a> to learn more about special offers just for you.
                                </p>
                                <div style='margin-top: 20px; text-align: center;'>
                                    <a style='display: inline-block; background-color: #28a745; color: white; padding: 10px 20px; border-radius: 8px; text-decoration: none; font-size: 16px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); font-weight: bold;'>
                                        Your VCard has been activated successfully!
                                    </a>
                                </div>
                                <div style='margin-top: 20px; text-align: center;'>
                                    <a href='https://vegacity.id.vn/etagEdit/{packageOrderExist.Data.Id}' style='display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; border-radius: 5px; text-decoration: none; font-size: 16px;'>
                                         Click here to see more
                                    </a>
                                </div>
                            </div>
                            <div style='background-color: #333; color: white; padding: 10px; text-align: center; font-size: 14px;'>
                                <p style='margin: 0;'>© 2024 Vega City. All rights reserved.</p>
                            </div>
                        </div>";
                        await MailUtil.SendMailAsync(packageOrderExist.Data.CusEmail, subject, body);
                    }
                    catch (Exception ex)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = UserMessage.SendMailError
                        };
                    }
                }
            }
            return new ResponseAPI()
            {
                MessageResponse = EtagMessage.ActivateEtagSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new { packageItemId = packageOrderExist.Data.Id }
            };
        }
        public async Task<ResponseAPI> PrepareChargeMoneyEtag(ChargeMoneyRequest req)
        {
            if (req.ChargeAmount <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            if (!ValidationUtils.IsCCCD(req.CccdPassport))
                throw new BadHttpRequestException("CCCD/Passport is invalid", HttpStatusCodes.BadRequest);
            var promotionAutos = await _unitOfWork.GetRepository<Promotion>().GetListAsync(
                predicate: x => x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == (int)PromotionStatusEnum.Automation
            );
            foreach (var prAuto in promotionAutos)
            {
                if (req.ChargeAmount >= prAuto.RequireAmount)
                {
                    req.PromoCode = prAuto.PromotionCode;
                    break;
                }
            }
            var packageOrderExsit = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageOrderId
            , include: w => w.Include(wallet => wallet.Wallets).ThenInclude(w => w.WalletType).Include(z => z.Package))
                ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            if (packageOrderExsit.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.NotFound);
            }
            if (packageOrderExsit.Wallets.SingleOrDefault().WalletType.Name == "SpecificWallet")
            {
                throw new BadHttpRequestException(PackageItemMessage.InvalidType, HttpStatusCodes.BadRequest);
            }
            #region check session
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageOrderExsit.Package.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            if (packageOrderExsit.Wallets.SingleOrDefault().EndDate < TimeUtils.GetCurrentSEATime())
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.BadRequest);
            if (PaymentTypeHelper.allowedPaymentTypes.Contains(req.PaymentType) == false)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.PaymentTypeInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.PromoCode == null)
            {
                var newOrder = new Order()
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                    Name = "Charge Money for: " + packageOrderExsit.CusName + "with balance: " + req.ChargeAmount,
                    Status = OrderStatus.Pending,
                    PackageOrderId = req.PackageOrderId,
                    UserId = userId,
                    SaleType = SaleType.PackageItemCharge,
                    TotalAmount = req.ChargeAmount,
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                var newPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    FinalAmount = req.ChargeAmount,
                    Name = req.PaymentType,
                    OrderId = newOrder.Id,
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Status = PaymentStatus.Pending,
                };
                await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                string key = req.PaymentType + "_" + newOrder.InvoiceId;
                var transactionCharge = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Amount = req.ChargeAmount,
                    OrderId = newOrder.Id,
                    Status = TransactionStatus.Pending,
                    Type = TransactionType.ChargeMoney,
                    WalletId = packageOrderExsit.Wallets.SingleOrDefault().Id,
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Description = "Charge Money for: " + packageOrderExsit.CusName + "with balance: " + req.ChargeAmount,
                    IsIncrease = true,
                    UserId = newOrder.UserId
                };
                await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionCharge);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.CreateOrderForCharge,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        invoiceId = newOrder.InvoiceId,
                        balance = req.ChargeAmount,
                        transactionChargeId = transactionCharge.Id,
                        packageItemId = packageOrderExsit.Id,
                        Key = key,
                        UrlDirect = $"https://api.vegacity.id.vn/api/v1/payment/{req.PaymentType.ToLower()}/order/charge-money", //https://localhost:44395/api/v1/payment/momo/order, http://14.225.204.144:8000/api/v1/payment/momo/order
                        UrlIpn = $"https://vegacity.id.vn/order-status?status=success&orderId={newOrder.Id}"
                    }
                } : new ResponseAPI()
                {
                    MessageResponse = EtagMessage.CreateOrderForChargeFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            else
            {
                Promotion checkPromo = await CheckPromo(req.PromoCode, req.ChargeAmount)
                    ?? throw new BadHttpRequestException(PromotionMessage.AddPromotionFail, HttpStatusCodes.BadRequest);
                int amountPromo = 0;
                if (checkPromo.Status != (int)PromotionStatusEnum.Automation)
                {
                    if (checkPromo.MaxDiscount <= req.ChargeAmount * (int)checkPromo.DiscountPercent)
                    {
                        amountPromo = (int)checkPromo.MaxDiscount;
                    }
                    else
                    {
                        //amountPromo = req.ChargeAmount * (int)checkPromo.DiscountPercent;
                        amountPromo = (int)(req.ChargeAmount * checkPromo.DiscountPercent + 0.5f);

                    }
                }
                else
                {
                    if (req.ChargeAmount >= checkPromo.RequireAmount)
                    {
                        amountPromo = (int)checkPromo.MaxDiscount;
                    }
                }


                var newOrder = new Order()
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                    Name = "Charge Money for: " + packageOrderExsit.CusName + "with balance: " + req.ChargeAmount,
                    Status = OrderStatus.Pending,
                    PackageOrderId = req.PackageOrderId,
                    UserId = userId,
                    SaleType = SaleType.PackageItemCharge,
                    TotalAmount = req.ChargeAmount - amountPromo,
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                var newPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    FinalAmount = req.ChargeAmount,
                    Name = req.PaymentType,
                    OrderId = newOrder.Id,
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Status = PaymentStatus.Pending,
                };
                await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                var newPromotionOrder = new PromotionOrder()
                {
                    Id = Guid.NewGuid(),
                    PromotionId = checkPromo.Id,
                    OrderId = newOrder.Id,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Deflag = false,
                    DiscountAmount = amountPromo
                };
                await _unitOfWork.GetRepository<PromotionOrder>().InsertAsync(newPromotionOrder);
                var transactionCharge = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Amount = req.ChargeAmount,
                    OrderId = newOrder.Id,
                    Status = TransactionStatus.Pending,
                    Type = TransactionType.ChargeMoney,
                    WalletId = packageOrderExsit.Wallets.SingleOrDefault().Id,
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Description = "Charge Money for: " + packageOrderExsit.CusName + " with balance: " + req.ChargeAmount,
                    IsIncrease = true,
                    UserId = newOrder.UserId
                };
                await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionCharge);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.CreateOrderForCharge,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        invoiceId = newOrder.InvoiceId,
                        packageOrderId = packageOrderExsit.Id,
                        transactionChargeId = transactionCharge.Id,
                        balance = req.ChargeAmount,
                        Key = req.PaymentType + "_" + newOrder.InvoiceId,
                        UrlDirect = $"https://api.vegacity.id.vn/api/v1/payment/{req.PaymentType.ToLower()}/order/charge-money", //https://localhost:44395/api/v1/payment/momo/order, http://
                        UrlIpn = $"https://vegacity.id.vn/order-status?status=success&orderId={newOrder.Id}"

                    }
                };
            }
        }
        private async Task<Promotion> CheckPromo(string promoCode, int amount)
        {
            var checkPromo = await _unitOfWork.GetRepository<Promotion>().SingleOrDefaultAsync
                    (predicate: x => x.PromotionCode == promoCode && x.Status == (int)PromotionStatusEnum.Active || x.Status == (int)PromotionStatusEnum.Automation)
                    ?? throw new BadHttpRequestException(PromotionMessage.NotFoundPromotion, HttpStatusCodes.NotFound);
            if (checkPromo.EndDate < TimeUtils.GetCurrentSEATime())
                throw new BadHttpRequestException(PromotionMessage.PromotionExpired, HttpStatusCodes.BadRequest);
            if (checkPromo.Quantity <= 0)
                throw new BadHttpRequestException(PromotionMessage.PromotionOutOfStock, HttpStatusCodes.BadRequest);
            if (checkPromo.RequireAmount > amount)
                throw new BadHttpRequestException(PromotionMessage.PromotionRequireAmount, HttpStatusCodes.BadRequest);
            return checkPromo;
        }
        //MONEY AND EXPIRE 
        public async Task CheckPackageItemExpire()
        {
            var currentDate = TimeUtils.GetCurrentSEATime();
            var packageOrders = (List<PackageOrder>)await _unitOfWork.GetRepository<PackageOrder>().GetListAsync
                (predicate: x => x.EndDate < currentDate && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum());
            foreach (var item in packageOrders)
            {
                item.Status = PackageItemStatusEnum.Expired.GetDescriptionFromEnum();
                item.UpsDate = currentDate;
            }
            _unitOfWork.GetRepository<PackageOrder>().UpdateRange(packageOrders);
            await _unitOfWork.CommitAsync();
        }
        public async Task<ResponseAPI> PackageItemPayment(Guid packageOrderId, int totalPrice, Guid storeId, List<OrderProduct> products)
        {

            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync
                (predicate: x => !x.Deflag && x.Id == storeId && x.Status == (int)StoreStatusEnum.Opened,
                include: z => z.Include(a => a.UserStoreMappings).Include(z => z.Wallets));
            if (store == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var packageOrder = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync(predicate: x => x.Id == packageOrderId
                && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(), include: etag => etag.Include(y => y.Wallets))
                ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            if (packageOrder.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.NotFound);
            }
            if (packageOrder.Wallets.SingleOrDefault().EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = PackageItemMessage.PackageItemExpired,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (packageOrder.Wallets.SingleOrDefault().Balance < totalPrice)
            {
                return new ResponseAPI
                {
                    MessageResponse = WalletTypeMessage.NotEnoughBalance,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }

            var order = new Order()
            {
                Id = Guid.NewGuid(),
                Name = "Payment From Store of: " + packageOrder.CusName,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                SaleType = SaleType.PackageItemPayment,
                Status = OrderStatus.Completed,
                TotalAmount = totalPrice,
                UserId = store.UserStoreMappings.SingleOrDefault().UserId,
                PackageOrderId = packageOrder.Id,
                InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                StoreId = storeId
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(order);

            foreach (var product in products)
            {
                var orderDetail = new OrderDetail()
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    Amount = product.Price,
                    FinalAmount = product.Price,
                    PromotionAmount = 0,
                    Vatamount = 0,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
            }

            var newPayment = new Payment
            {
                Id = Guid.NewGuid(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                FinalAmount = order.TotalAmount,
                Name = "Charge Money for: " + packageOrder.CusName + "with balance: " + order.TotalAmount,
                OrderId = order.Id,
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = PaymentStatus.Pending,
            };
            await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);

            packageOrder.Wallets.SingleOrDefault().Balance -= totalPrice;
            packageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrder.Wallets.SingleOrDefault());
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = totalPrice,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                WalletId = packageOrder.Wallets.SingleOrDefault().Id,
                IsIncrease = false,
                Type = TransactionType.Payment.GetDescriptionFromEnum(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Description = "Payment From Store of: " + packageOrder.CusName,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                StoreId = storeId,
                UserId = store.UserStoreMappings.SingleOrDefault().UserId
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);

            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync
                (predicate: x => x.Id == store.MarketZoneId, include: z => z.Include(a => a.MarketZoneConfig));
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == marketZone.Email && x.Status == (int)UserStatusEnum.Active, include: a => a.Include(z => z.Wallets));
            var newTransactionMarket = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = (int)(totalPrice * marketZone.MarketZoneConfig.StoreStranferRate),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Description = "Store Transfer: " + store.Name,
                IsIncrease = true,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                WalletId = admin.Wallets.SingleOrDefault().Id,
                Type = TransactionType.StoreTransfer.GetDescriptionFromEnum(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                UserId = admin.Id,
                StoreId = storeId
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransactionMarket);

            admin.Wallets.SingleOrDefault().Balance += (int)(totalPrice * marketZone.MarketZoneConfig.StoreStranferRate);
            admin.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(admin.Wallets.SingleOrDefault());

            var newStoreTransfer = new StoreMoneyTransfer
            {
                Id = Guid.NewGuid(),
                Amount = (int)(totalPrice * marketZone.MarketZoneConfig.StoreStranferRate),
                CrDate = TimeUtils.GetCurrentSEATime(),
                StoreId = storeId,
                UpsDate = TimeUtils.GetCurrentSEATime(),
                TransactionId = newTransactionMarket.Id,
                Description = "Store Transfer: " + store.Name,
                IsIncrease = true,
                MarketZoneId = marketZone.Id,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
            };
            await _unitOfWork.GetRepository<StoreMoneyTransfer>().InsertAsync(newStoreTransfer);

            var newTransactionStore = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = (int)(totalPrice * (1 - marketZone.MarketZoneConfig.StoreStranferRate)),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Description = "Payment From Store of: " + packageOrder.CusName,
                IsIncrease = true,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                WalletId = store.Wallets.SingleOrDefault().Id,
                Type = TransactionType.Payment.GetDescriptionFromEnum(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                UserId = store.UserStoreMappings.SingleOrDefault().UserId,
                StoreId = storeId
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransactionStore);

            store.Wallets.SingleOrDefault().Balance += (int)(totalPrice * (1 - marketZone.MarketZoneConfig.StoreStranferRate));
            store.Wallets.SingleOrDefault().BalanceHistory += (int)(totalPrice * (1 - marketZone.MarketZoneConfig.StoreStranferRate));
            store.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(store.Wallets.SingleOrDefault());

            var newStoreTransaction = new StoreMoneyTransfer
            {
                Id = Guid.NewGuid(),
                Amount = (int)(totalPrice * (1 - marketZone.MarketZoneConfig.StoreStranferRate)),
                CrDate = TimeUtils.GetCurrentSEATime(),
                StoreId = storeId,
                UpsDate = TimeUtils.GetCurrentSEATime(),
                TransactionId = newTransactionStore.Id,
                Description = "Payment From Store of: " + packageOrder.CusName,
                IsIncrease = true,
                MarketZoneId = marketZone.Id,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
            };
            await _unitOfWork.GetRepository<StoreMoneyTransfer>().InsertAsync(newStoreTransaction);
            return await _unitOfWork.CommitAsync() > 0
               ? new ResponseAPI()
               {
                   StatusCode = HttpStatusCodes.OK,
                   MessageResponse = EtagMessage.PaymentQrCodeSuccess
               }
               : new ResponseAPI()
               {
                   StatusCode = HttpStatusCodes.InternalServerError,
                   MessageResponse = EtagMessage.FailedToPay
               };
        }
        public async Task SolveWalletPackageItem(Guid apiKey)
        {
            var packageItems = await _unitOfWork.GetRepository<PackageOrder>().GetListAsync
                (predicate: x => x.Status == PackageItemStatusEnum.Expired.GetDescriptionFromEnum(),
                 include: w => w.Include(z => z.Wallets));
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync
                (predicate: x => x.Id == apiKey, include: z => z.Include(a => a.MarketZoneConfig));
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == marketZone.Email && x.Status == (int)UserStatusEnum.Active && x.MarketZoneId == apiKey,
                include: a => a.Include(z => z.Wallets));
            Wallet adminWallet = admin.Wallets.SingleOrDefault();
            if (packageItems.Count == 0)
            {
                return;
            }
            else
            {
                foreach (var item in packageItems)
                {
                    if (item.Wallets.SingleOrDefault().Balance > 0)
                    {
                        var transaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = item.Wallets.SingleOrDefault().Id,
                            IsIncrease = false,
                            Amount = item.Wallets.SingleOrDefault().Balance,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Description = "Refund for: " + item.CusName,
                            Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                            Status = TransactionStatus.Success,
                            Type = TransactionType.RefundMoney,
                            UserId = admin.Id
                        };
                        await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);

                        var transactionTransfer = new CustomerMoneyTransfer
                        {
                            Id = Guid.NewGuid(),
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Amount = item.Wallets.SingleOrDefault().Balance,
                            MarketZoneId = marketZone.Id,
                            IsIncrease = true,
                            Status = OrderStatus.Completed,
                            PackageOrderId = item.Id,
                            TransactionId = transaction.Id
                        };
                        await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(transactionTransfer);

                        item.Wallets.SingleOrDefault().Balance = 0;
                        item.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Wallet>().UpdateAsync(item.Wallets.SingleOrDefault());

                        adminWallet.BalanceHistory += item.Wallets.SingleOrDefault().Balance;
                        adminWallet.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Wallet>().UpdateAsync(adminWallet);
                    }
                }
            }
            await _unitOfWork.CommitAsync();
        }
        private static string NormalizeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Chuẩn hóa chuỗi để loại bỏ dấu
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Chuẩn hóa chuỗi, chuyển thành chữ thường và loại bỏ khoảng trắng
            var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
            result = Regex.Replace(result, @"\s+", ""); // Loại bỏ tất cả khoảng trắng

            return result;
        }
        public async Task<ResponseAPI> GetLostPackageItem(GetLostPackageItemRequest req)
        {
            //need authorize cashierWeb
            var searchName = NormalizeString(req.FullName);

            var packageOrders = await _unitOfWork.GetRepository<PackageOrder>().GetListAsync(
               predicate: x => x.CusCccdpassport == req.Cccdpassport && x.CusEmail == req.Email
               && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(),
               include: p => p.Include(w => w.Wallets).Include(a => a.Package).Include(v => v.Vcard)
               );

            var packageOrderLost = packageOrders.SingleOrDefault(
                x => NormalizeString(x.CusName) == searchName
               );
            if (packageOrderLost == null)
            {
                throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            }
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == userId)
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't exist in system",
                                                            HttpStatusCodes.BadRequest);
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageOrderLost.Package.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            if (!ValidationUtils.IsCCCD(req.Cccdpassport))
                throw new BadHttpRequestException(PackageItemMessage.CCCDInvalid, HttpStatusCodes.BadRequest);
            if (!ValidationUtils.IsEmail(req.Email))
                throw new BadHttpRequestException(PackageItemMessage.EmailInvalid, HttpStatusCodes.BadRequest);
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                throw new BadHttpRequestException(PackageItemMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);
            if (packageOrderLost.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.BadRequest);
            }
            //var ID = packageItemLost.Id;
            //after run api to get by cccd and mark as Blocked, transfer money, check log transaction
            //case lost vcard
            //check cus transfer to create new card
            packageOrderLost.Status = PackageItemStatusEnum.Blocked.GetDescriptionFromEnum();
            packageOrderLost.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrderLost);

            packageOrderLost.Wallets.SingleOrDefault().Deflag = true;
            packageOrderLost.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderLost.Wallets.SingleOrDefault());

            if (packageOrderLost.Vcard != null)
            {

                packageOrderLost.Vcard.Status = VCardStatus.Blocked.GetDescriptionFromEnum();
                _unitOfWork.GetRepository<Vcard>().DeleteAsync(packageOrderLost.Vcard);
            }

            //Transaction
            var newTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = packageOrderLost.Wallets.SingleOrDefault().Id,
                IsIncrease = false,
                Amount = 50000,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Description = "Charge Fee From Lost PackageItem: " + packageOrderLost.CusName,
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Status = TransactionStatus.Pending,
                Type = TransactionType.TransferMoney,
                UserId = userId,
                //need to update with status is success & add deposit id above
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
            int check = await _unitOfWork.CommitAsync();
            if (check > 0)
            {
                if (packageOrderLost.CusEmail != null)
                {
                    try
                    {
                        string subject = "Lost VCard Vega City";
                        string body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                            <div style='background-color: #ff4d4f; color: white; padding: 20px; text-align: center;'>
                                <h1 style='margin: 0;'>Notification Lost V-Card</h1>
                            </div>
                            <div style='padding: 20px; background-color: #f9f9f9;'>
                                <p style='font-size: 16px; color: #333;'>Hello, <strong>{packageOrderLost.CusName}</strong>,</p>
                                <p style='font-size: 16px; color: #333; line-height: 1.5;'>
                                    We have received your request to report your lost V-Card with the following information:
                                </p>
                                <ul style='font-size: 16px; color: #333; line-height: 1.5;'>
                                    <li><strong>Number V-Card:</strong> {packageOrderLost.VcardId}</li>
                                </ul>
                                <p style='font-size: 16px; color: #333; line-height: 1.5;'>
                                    For security reasons, your card has been temporarily blocked. If this is not what you requested, please contact us immediately via email {user.Email}. hoặc qua số điện thoại hỗ trợ : {user.PhoneNumber}.
                                </p>
                                
                            </div>
                            <div style='background-color: #333; color: white; padding: 10px; text-align: center; font-size: 14px;'>
                                <p style='margin: 0;'>© 2024 Vega City. All rights reserved.</p>
                            </div>
                        </div>";
                        await MailUtil.SendMailAsync(packageOrderLost.CusEmail, subject, body);
                    }
                    catch (Exception ex)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = UserMessage.SendMailLostError
                        };
                    }
                }
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.FailedToMark,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.SuccessfullyReadyToCreate,
                StatusCode = HttpStatusCodes.OK,
                Data = new { packageItemId = packageOrderLost.Id }
            };
        }
        //assign new 
        #endregion
    }
}

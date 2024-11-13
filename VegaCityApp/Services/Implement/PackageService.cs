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

namespace VegaCityApp.API.Services.Implement
{
    public class PackageService: BaseService<PackageService>, IPackageService
    {
        public PackageService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PackageService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        #region admin
        public async Task<ResponseAPI> CreatePackage(CreatePackageRequest req)
        {
            var result = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name && !x.Deflag);
            if (result != null)
            throw new BadHttpRequestException(PackageMessage.ExistedPackageName, HttpStatusCodes.BadRequest);
            if(req.Price <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            if (req.Duration <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            var packageType = await SearchPackageType(req.PackageTypeId);
            var newPackage = new Package()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Deflag = false,
                Duration = req.Duration,
                PackageTypeId = req.PackageTypeId,
                ImageUrl = req.ImageUrl,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
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
          
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == packageId && !x.Deflag);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageMessage.NotFoundPackage
                };
            }
            package.Name = req.Name ?? package.Name;
            package.Description = req.Description ?? package.Description;
            package.Price = req.Price;
            package.Duration = req.Duration ?? package.Duration;
            package.ImageUrl = req.ImageUrl ?? package.ImageUrl;
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
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Deflag == false
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
        public async Task<ResponseAPI> CreatePackageType(CreatePackageTypeRequest req)
        {
            if(req.ZoneId == null)
            {
                throw new BadHttpRequestException(ZoneMessage.SearchZoneFail, HttpStatusCodes.NotFound);
            }
            var zoneExist = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Id == req.ZoneId);
            if (zoneExist == null)
            {
                throw new BadHttpRequestException(ZoneMessage.SearchZoneFail, HttpStatusCodes.NotFound);
            }
            var resultName = NormalizeString(req.Name);
            var results = await _unitOfWork.GetRepository<PackageType>().GetListAsync(predicate: x => x.ZoneId == req.ZoneId);
            var resultTrace = results.SingleOrDefault(y => NormalizeString(y.Name) == resultName); 
            if (resultTrace != null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.ExistedPackageName,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var newPackageType = new PackageType()
            {
                Id = Guid.NewGuid(),
                ZoneId = req.ZoneId, //must be zone
                Name = req.Name,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false
            };
            await _unitOfWork.GetRepository<PackageType>().InsertAsync(newPackageType);
            var response = new ResponseAPI()
            {
                MessageResponse = PackageTypeMessage.CreatePackageTypeSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    PackageId = newPackageType.Id
                }

            };
            int check = await _unitOfWork.CommitAsync();

            return check > 0 ? response : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = PackageTypeMessage.CreatePackageTypeFail
            };
        }
        public async Task<ResponseAPI> UpdatePackageType(Guid packageTypeId, UpdatePackageTypeRequest req)
        {

            var packageType = await _unitOfWork.GetRepository<PackageType>().SingleOrDefaultAsync(predicate: x => x.Id == packageTypeId && x.Deflag == false);
            if (packageType == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageTypeMessage.NotFoundPackageType
                };
            }
            packageType.Name = req.Name;
            packageType.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<PackageType>().UpdateAsync(packageType);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.UpdatePackageSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageTypeId = packageType.Id
                    }
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageTypeMessage.UpdatePackageTypeFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }
        public async Task<ResponseAPI> DeletePackageType(Guid PackageTypeId)
        {
            var packageType = await _unitOfWork.GetRepository<PackageType>().SingleOrDefaultAsync
                (predicate: x => x.Id == PackageTypeId);
            if (packageType == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageTypeMessage.NotFoundPackageType
                };
            }
            packageType.Deflag = true;
            _unitOfWork.GetRepository<PackageType>().UpdateAsync(packageType);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = PackageTypeMessage.DeletePackageTypeSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageTypeId = packageType.Id
                    }
                }
                : new ResponseAPI()
                {
                    MessageResponse = PackageTypeMessage.DeletePackageTypeFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        #endregion
        public async Task<ResponseAPI<Package>> SearchPackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageId && x.Deflag==false,
                include: package => package.Include(a => a.PackageDetails).ThenInclude(w => w.WalletType)
                                           .Include(b => b.PackageOrders)
                                           .Include(c => c.PackageItems)
                                           .Include(x => x.PackageType)
            ) ?? throw new BadHttpRequestException(PackageMessage.NotFoundPackage, HttpStatusCodes.NotFound);

            return new ResponseAPI<Package>()
            {
                MessageResponse = PackageMessage.GetPackagesSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = package
                
            };
        }
        public async Task<ResponseAPI<IEnumerable<GetPackageTypeResponse>>> SearchAllPackageType (int size, int page)
        {
            try
            {
                IPaginate<GetPackageTypeResponse> data = await _unitOfWork.GetRepository<PackageType>().GetPagingListAsync(

                selector: x => new GetPackageTypeResponse()
                {
                    Id = x.Id,
                    ZoneId = x.ZoneId,
                    Name = x.Name,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Deflag == false
            );
                return new ResponseAPI<IEnumerable<GetPackageTypeResponse>>
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
                return new ResponseAPI<IEnumerable<GetPackageTypeResponse>>
                {
                    MessageResponse = PackageMessage.GetPackagesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }

        }
        public async Task<ResponseAPI<PackageType>> SearchPackageType(Guid PackageTypeId)
        {
            var packageType = await _unitOfWork.GetRepository<PackageType>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageTypeId && x.Deflag == false,
                include: package => package.Include(y => y.Packages)
            ) ?? throw new BadHttpRequestException(PackageTypeMessage.NotFoundPackageType, HttpStatusCodes.NotFound);

            return new ResponseAPI<PackageType>()
            {
                MessageResponse = PackageTypeMessage.GetPackageTypesSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = packageType
                
            };
        } 
        public async Task<ResponseAPI> CreatePackageItem(int quantity, CreatePackageItemRequest req)
        {
            if (quantity <= 0) throw new BadHttpRequestException("Number Quantity must be more than 0", HttpStatusCodes.BadRequest);
            if(req.StartDate < TimeUtils.GetCurrentSEATime().AddDays(-1) || req.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.InvalidDuration,
                    StatusCode = HttpStatusCodes.BadRequest,
                };
            }
            if(req.StartDate >= req.EndDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.SameStrAndEndDate,
                    StatusCode = HttpStatusCodes.BadRequest,
                };
            }
            if (req.PackageItemId != null)
            {
                //case parent + lost
                var packageItemExist = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageItemId,
                    include: y => y.Include(t => t.Package).ThenInclude(p => p.PackageType)
                    .Include(d => d.Deposits)
                    .Include(w => w.Wallet).ThenInclude(t => t.WalletType)
                    .Include(tr => tr.Wallet.Transactions));
                if(packageItemExist.Status == PackageItemStatusEnum.Inactive.GetDescriptionFromEnum())
                {
                    throw new BadHttpRequestException(PackageItemMessage.MustActivated, HttpStatusCodes.NotFound);
                }
                var package = await SearchPackage(packageItemExist.Package.Id);
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
                    predicate: x => x.UserId == userId && x.ZoneId == package.Data.PackageType.ZoneId
                                   && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                                   && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
                )
                    ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                                HttpStatusCodes.BadRequest);
                #endregion
                List<GetListPackageItemResponse> packageItems = new List<GetListPackageItemResponse>();
                
                    if (packageItemExist.EndDate <= TimeUtils.GetCurrentSEATime())
                    {
                        throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.NotFound);
                    }
                var hasSuccessTransaction = packageItemExist.Wallet.Transactions
                    .Any(t => t.Status == TransactionStatus.Success.GetDescriptionFromEnum() && t.OrderId != null);

                var hasPendingTransaction = packageItemExist.Wallet.Transactions
                    .Any(t => t.Status == TransactionStatus.Pending.GetDescriptionFromEnum());

                var hasPendingOrderCharge = packageItemExist.Orders
                    .Any(t => t.Status == OrderStatus.Pending.GetDescriptionFromEnum() && packageItemExist.Orders.SingleOrDefault().SaleType == SaleType.FeeChargeCreate);
                if (packageItemExist.Status == PackageItemStatusEnum.Blocked.GetDescriptionFromEnum() && hasPendingTransaction)
                {
                    //after run api to get by cccd and mark as Blocked, transfer money, check log transaction
                    //case lost vcard
                    //check cus transfer to create new card
                    //co deposit
                    //phai dc active luon
                    if (quantity > 1)
                    {
                        throw new BadHttpRequestException(PackageItemMessage.OneAsATime, HttpStatusCodes.NotFound);
                    }
                    
                    //if (hasSuccessTransaction)
                    //{
                    //    throw new BadHttpRequestException(PackageItemMessage.RequestPAID, HttpStatusCodes.BadRequest);
                    //}
                    if (hasPendingTransaction)
                    {

                        var newPackageItemII = new PackageItem()
                        {
                            Id = Guid.NewGuid(),
                            PackageId = packageItemExist.PackageId,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Status = PackageItemStatus.Inactive.GetDescriptionFromEnum(),
                            Gender = packageItemExist.Gender,
                            IsChanged = true,
                            WalletId = packageItemExist.Wallet.Id,
                            StartDate = packageItemExist.StartDate,
                            EndDate = packageItemExist.EndDate,
                            IsAdult = packageItemExist.IsAdult,
                            Name = packageItemExist.Name,
                            PhoneNumber = packageItemExist.PhoneNumber,
                            Cccdpassport = packageItemExist.Cccdpassport,
                            Email = packageItemExist.Email,
                            Rfid = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime())
                        };
                        packageItems.Add(_mapper.Map<GetListPackageItemResponse>(newPackageItemII));
                        await _unitOfWork.GetRepository<PackageItem>().InsertAsync(newPackageItemII);
                        //New CHARGE ORDER OPEN CARD FEE if balance <50
                        if (packageItemExist.Wallet.Balance > 50000)
                        {
                            var newChargeFeeOderPAID = new Order
                            {
                                Id = Guid.NewGuid(),
                                PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
                                Name = "Order Charge Fee Create Card Of " + packageItemExist.Name,
                                TotalAmount = 50000,
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                Status = OrderStatus.Completed,
                                InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                                StoreId = null,
                                PackageItemId = newPackageItemII.Id,
                                PackageId = newPackageItemII.PackageId,
                                UserId = userId,
                                SaleType = SaleType.FeeChargeCreate,
                            };
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newChargeFeeOderPAID);
                            //update balance
                            packageItemExist.Wallet.Balance -= 50000;
                            packageItemExist.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageItemExist.Wallet);


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
                            //deposit
                            var newDepositII = new Deposit()
                            {
                                Id = Guid.NewGuid(),
                                Amount = (int)packageItemExist.Wallet.Balance,
                                IsIncrease = true,
                                Name = "Customer Money Transfer From" + packageItemExist.Name,
                                PaymentType = PaymentTypeHelper.allowedPaymentTypes[6],
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                PackageItemId = newPackageItemII.Id,
                                WalletId = newPackageItemII.WalletId,
                                OrderId = newChargeFeeOderPAID.Id
                            };
                            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDepositII);

                            var transactionId = packageItemExist.Wallet.Transactions.SingleOrDefault().Id;
                            packageItemExist.Wallet.Transactions.SingleOrDefault().Status = TransactionStatus.Success.GetDescriptionFromEnum();
                            packageItemExist.Wallet.Transactions.SingleOrDefault().OrderId = newChargeFeeOderPAID.Id;
                            packageItemExist.Wallet.Transactions.SingleOrDefault().DespositId = newDepositII.Id;
                            _unitOfWork.GetRepository<Transaction>().UpdateAsync(packageItemExist.Wallet.Transactions.SingleOrDefault());
                            if (await _unitOfWork.CommitAsync() > 0)
                            {
                                if (newPackageItemII != null && newPackageItemII.WalletId == packageItemExist.Wallet.Id)
                                {
                                    newPackageItemII.Status = PackageItemStatus.Active.GetDescriptionFromEnum(); 
                                    _unitOfWork.GetRepository<PackageItem>().UpdateAsync(newPackageItemII); 
                                    await _unitOfWork.CommitAsync(); 
                                }

                                return new ResponseAPI()
                                {
                                    MessageResponse = PackageItemMessage.SuccessGenerateNewPAID,
                                    StatusCode = HttpStatusCodes.OK,
                                    Data = new
                                    {
                                        PackageItemIIId = newPackageItemII.Id,
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
                            var newChargeFeeOder = new Order
                            {
                                Id = Guid.NewGuid(),
                                PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
                                Name = "Order Charge Fee Create Card Of " + packageItemExist.Name,
                                TotalAmount = 50000,
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                Status = OrderStatus.Pending,
                                InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                                StoreId = null,
                                PackageItemId = newPackageItemII.Id,
                                PackageId = newPackageItemII.PackageId,
                                UserId = userId,
                                SaleType = SaleType.FeeChargeCreate,
                            };
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newChargeFeeOder);

                            //deposit
                            var newDepositII = new Deposit()
                            {
                                Id = Guid.NewGuid(),
                                Amount = (int)packageItemExist.Wallet.Balance,
                                IsIncrease = true,
                                Name = "Customer Money Transfer From" + packageItemExist.Name,
                                PaymentType = PaymentTypeHelper.allowedPaymentTypes[6],
                                CrDate = TimeUtils.GetCurrentSEATime(),
                                UpsDate = TimeUtils.GetCurrentSEATime(),
                                PackageItemId = packageItemExist.Id,
                                WalletId = packageItemExist.WalletId,
                                OrderId = newChargeFeeOder.Id
                            };
                            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDepositII);
                            var pendingTransaction = packageItemExist.Wallet.Transactions
                                 .FirstOrDefault(t => t.Status == TransactionStatus.Pending.GetDescriptionFromEnum());
                   
                            if (hasPendingTransaction)
                            {
                                var pendingTransactionId = pendingTransaction.Id;
                                pendingTransaction.OrderId = newChargeFeeOder.Id;
                                pendingTransaction.DespositId = newDepositII.Id;
                                _unitOfWork.GetRepository<Transaction>().UpdateAsync(pendingTransaction);
                                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                                {
                                    MessageResponse = PackageItemMessage.SuccessGenerateNewUNPAID,
                                    StatusCode = HttpStatusCodes.OK,
                                    Data = new
                                    {
                                        PackageItemIIId = newPackageItemII.Id,
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
                    else if (packageItemExist.Wallet.Transactions.Any(t => t.Status == TransactionStatus.Success.GetDescriptionFromEnum()
                    && t.OrderId != null))
                    {
                        throw new BadHttpRequestException(PackageItemMessage.RequestPAID, HttpStatusCodes.BadRequest);
                    }
                }
                //case generate vcard child
                else// for in here
                {
                    if (packageItemExist.IsAdult == false)
                    {
                        throw new BadHttpRequestException(PackageItemMessage.NotAdult, HttpStatusCodes.BadRequest);
                    }
                    for (var i = 0; i < quantity; i++)
                    {
                        var newWallet = new Wallet()
                        {
                            Id = Guid.NewGuid(),
                            Balance = (int)package.Data.PackageDetails.SingleOrDefault().StartMoney,
                            BalanceHistory = (int)package.Data.PackageDetails.SingleOrDefault().StartMoney,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Deflag = false,
                            WalletTypeId = (Guid)package.Data.PackageDetails.SingleOrDefault().WalletTypeId
                        };
                        await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
                        var newPackageItemChild = new PackageItem()
                        {
                            Id = Guid.NewGuid(),
                            PackageId = packageItemExist.Package.Id,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Status = PackageItemStatus.Inactive.GetDescriptionFromEnum(),
                            Gender = GenderEnum.Other.GetDescriptionFromEnum(),
                            IsChanged = false,
                            WalletId = newWallet.Id,
                            StartDate = TimeUtils.GetCurrentSEATime(),
                            EndDate = packageItemExist.EndDate,
                            IsAdult = false,
                            Name = "Vcard for Child Created at: " + TimeUtils.GetCurrentSEATime(),
                            PhoneNumber = packageItemExist.PhoneNumber,
                            Cccdpassport = packageItemExist.Cccdpassport,
                            Email = packageItemExist.Email,
                            Rfid = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime())
                        };
                        packageItems.Add(_mapper.Map<GetListPackageItemResponse>(newPackageItemChild));
                        await _unitOfWork.GetRepository<PackageItem>().InsertAsync(newPackageItemChild);
                    }
                }
               // }
                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.CreatePackageItemSuccessfully,
                    StatusCode = HttpStatusCodes.Created,
                    ParentName = packageItemExist.Name,
                    Data = packageItems,
                };
            }
            else
            {
                var package = await SearchPackage(req.PackageId);
                #region check 
                // after done main flow, make utils service to shorten this code
                var userId = GetUserIdFromJwt();
                var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                    predicate: x => x.UserId == userId && x.ZoneId == package.Data.PackageType.ZoneId
                                   && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                                   && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
                )
                    ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                                HttpStatusCodes.BadRequest);
                #endregion
                List<GetListPackageItemResponse> packageItems = new List<GetListPackageItemResponse>();
                for (var i = 0; i < quantity; i++)
                {
                    var newWallet = new Wallet()
                    {
                        Id = Guid.NewGuid(),
                        Balance = (int)package.Data.PackageDetails.SingleOrDefault().StartMoney,
                        BalanceHistory = (int)package.Data.PackageDetails.SingleOrDefault().StartMoney,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Deflag = false,
                        WalletTypeId = (Guid)package.Data.PackageDetails.SingleOrDefault().WalletTypeId
                    };
                    await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
                    var newPackageItem = new PackageItem()
                    {
                        Id = Guid.NewGuid(),
                        PackageId = req.PackageId,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Status = PackageItemStatus.Inactive.GetDescriptionFromEnum(),
                        Gender = GenderEnum.Other.GetDescriptionFromEnum(),
                        IsChanged = false,
                        WalletId = newWallet.Id,
                        StartDate = TimeUtils.GetCurrentSEATime(),
                        EndDate = req.EndDate,
                        IsAdult = true,
                        Rfid = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                        Name = "User VegaCity",
                    };
                    packageItems.Add(_mapper.Map<GetListPackageItemResponse>(newPackageItem));
                    await _unitOfWork.GetRepository<PackageItem>().InsertAsync(newPackageItem);
                }

                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.CreatePackageItemSuccessfully,
                    StatusCode = HttpStatusCodes.Created,
                    Data = packageItems
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<GetListPackageItemResponse>>> SearchAllPackageItem(int size, int page)
        {
            try
            {
                IPaginate<GetListPackageItemResponse> data = await _unitOfWork.GetRepository<PackageItem>().GetPagingListAsync(

                selector: x => new GetListPackageItemResponse()
                {
                    Id = x.Id,
                    PackageId = x.PackageId,
                    Name = x.Name,
                    Cccdpassport = x.Cccdpassport,
                    Email = x.Email,
                    Status = x.Status,
                    Gender = GenderEnum.Male.ToString(),
                    IsAdult = x.IsAdult,
                    WalletId = x.WalletId,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    IsChanged = x.IsChanged,
                    PhoneNumber = x.PhoneNumber,
                    Rfid = x.Rfid,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    WalletTypeName = x.Wallet.WalletType.Name,
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Package.PackageType.Zone.MarketZoneId == GetMarketZoneIdFromJwt(),
                include: x => x.Include(a => a.Package).ThenInclude(b => b.PackageType).ThenInclude(c => c.Zone)
                               .Include(z => z.Wallet).ThenInclude(g => g.WalletType)
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
        public async Task<ResponseAPI<PackageItem>> SearchPackageItem(Guid? PackageItemId, string? rfid)
        {
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageItemId || x.Rfid == rfid
                //&& x.Status == PackageItemStatus.Active.ToString() && x.Status == PackageItemStatus.Inactive.ToString()
                ,
                include: packageItem => packageItem
                .Include(b => b.Reports)
                .Include(c => c.Orders).ThenInclude(y => y.Transactions)
                .Include(d => d.Deposits)
                .Include(z => z.CustomerMoneyTransfers)
                .Include(e => e.Wallet).ThenInclude(t => t.WalletType)
            );

            if (packageItem == null)
            {
                return new ResponseAPI<PackageItem>()
                {
                    MessageResponse = PackageItemMessage.NotFoundPackageItem,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            var packageItemParent = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(
                predicate: x => x.Cccdpassport == packageItem.Cccdpassport && x.IsAdult == true
            );
            var qrCodeString = EnCodeBase64.EncodeBase64Etag("http://localhost:3000/etagEdit/" + packageItem.Id);
            return new ResponseAPI<PackageItem>()
            {
                MessageResponse = PackageItemMessage.GetPackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                ParentName = packageItemParent.Name,
                QRCode = qrCodeString,
                Data = packageItem
            };
        }
        public async Task<ResponseAPI> UpdatePackageItem(Guid packageItemId, UpdatePackageItemRequest req)
        {
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync
                (predicate: x => x.Id == packageItemId && x.Status == PackageItemStatus.Active.ToString(),
                 include: packageItem => packageItem.Include(b => b.Package).ThenInclude(c => c.PackageType)
            );
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageItem.Package.PackageType.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            if (packageItem == null)
                throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            if(packageItem.IsChanged == true)
                throw new BadHttpRequestException("You only change info one time !!", HttpStatusCodes.BadRequest);

            //update
            packageItem.Name = req.Name ?? packageItem.Name;
            packageItem.Gender = req.Gender ?? packageItem.Gender;
            packageItem.ImageUrl = req.ImageUrl ?? packageItem.ImageUrl;
            packageItem.IsChanged = true;

            _unitOfWork.GetRepository<PackageItem>().UpdateAsync(packageItem);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { PackageItemId = packageItem.Id }
            } : new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> UpdateRfIdPackageItem(Guid Id, string rfId)
        {
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync
                (predicate: x => x.Id == Id && x.Status == PackageItemStatus.Active.ToString(),
                 include: packageItem => packageItem.Include(b => b.Package).ThenInclude(c => c.PackageType)
            );
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageItem.Package.PackageType.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            if (packageItem == null)
                throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            if(packageItem.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum() && packageItem.Rfid == rfId)
                throw new BadHttpRequestException(PackageItemMessage.RfIdExist, HttpStatusCodes.BadRequest);
            //update
            packageItem.Rfid = rfId;

            _unitOfWork.GetRepository<PackageItem>().UpdateAsync(packageItem);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { PackageItemId = packageItem.Id }
            } : new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.UpdatePackageItemFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        } 
        public async Task<ResponseAPI> ActivePackageItem(Guid packageItem, ActivatePackageItemRequest req)
        {
            var packageItemExist = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(
                predicate: x => x.Id == packageItem , 
                include: z => z.Include(a => a.Package).ThenInclude(z => z.PackageType))
                    ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageItemExist.Package.PackageType.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            //vcard child 
            if (packageItemExist.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum())
            {
                throw new BadHttpRequestException(PackageItemMessage.AlreadyActivated, HttpStatusCodes.BadRequest);
            }
            if (packageItemExist.IsAdult == false)
            {
                packageItemExist.Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum();
                packageItemExist.Name = req.Name.Trim();
                packageItemExist.PhoneNumber =packageItemExist.PhoneNumber.Trim(); //from parent
                packageItemExist.Cccdpassport = packageItemExist.Cccdpassport.Trim(); //from parent
                packageItemExist.Email = packageItemExist.Email.Trim();
                packageItemExist.Gender = req.Gender.Trim();
                packageItemExist.StartDate = TimeUtils.GetCurrentSEATime();
                packageItemExist.EndDate = TimeUtils.GetCurrentSEATime().AddDays((double)packageItemExist.Package.Duration);
                packageItemExist.IsAdult = false;
                packageItemExist.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<PackageItem>().UpdateAsync(packageItemExist);
                //update wallet
                var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync(predicate: x => x.Id == packageItemExist.WalletId);
                wallet.StartDate = TimeUtils.GetCurrentSEATime();
                wallet.EndDate = TimeUtils.GetCurrentSEATime().AddDays((double)packageItemExist.Package.Duration);
                wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                //end child vcard
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = EtagMessage.ActivateEtagSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new { packageItemId = packageItemExist.Id }
                } : new ResponseAPI()
                {
                    MessageResponse = EtagMessage.ActivateEtagFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            else {
                if (!ValidationUtils.IsCCCD(req.Cccdpassport))
                    throw new BadHttpRequestException(PackageItemMessage.CCCDInvalid, HttpStatusCodes.BadRequest);
                if (!ValidationUtils.IsEmail(req.Email))
                    throw new BadHttpRequestException(PackageItemMessage.EmailInvalid, HttpStatusCodes.BadRequest);
                if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                    throw new BadHttpRequestException(PackageItemMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);
                if (req.Cccdpassport == packageItemExist.Cccdpassport)
                    throw new BadHttpRequestException(PackageItemMessage.CCCDExist, HttpStatusCodes.BadRequest);
                if (req.Email == packageItemExist.Email)
                    throw new BadHttpRequestException(PackageItemMessage.EmailExist, HttpStatusCodes.BadRequest);
                packageItemExist.Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum();
                packageItemExist.Name = req.Name.Trim();
                packageItemExist.PhoneNumber = req.PhoneNumber.Trim();
                packageItemExist.Cccdpassport = req.Cccdpassport.Trim();
                packageItemExist.Email = req.Email.Trim();
                packageItemExist.Gender = req.Gender.Trim();
                packageItemExist.StartDate = TimeUtils.GetCurrentSEATime();
                packageItemExist.EndDate = TimeUtils.GetCurrentSEATime().AddDays((double)packageItemExist.Package.Duration);
                packageItemExist.IsAdult = req.IsAdult;
                packageItemExist.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<PackageItem>().UpdateAsync(packageItemExist);
                //update wallet
                var wallet = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync(predicate: x => x.Id == packageItemExist.WalletId);
                wallet.StartDate = TimeUtils.GetCurrentSEATime();
                wallet.EndDate = TimeUtils.GetCurrentSEATime().AddDays((double)packageItemExist.Package.Duration);
                wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);

                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = EtagMessage.ActivateEtagSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new { packageItemId = packageItemExist.Id }
                } : new ResponseAPI()
                {
                    MessageResponse = EtagMessage.ActivateEtagFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            
        }
        public async Task<ResponseAPI> PrepareChargeMoneyEtag(ChargeMoneyRequest req)
        {
            if (req.ChargeAmount <= 0) throw new BadHttpRequestException("The number must be more than 0", HttpStatusCodes.BadRequest);
            if (!ValidationUtils.IsCCCD(req.CccdPassport))
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.CCCDInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var promotionAutos = await _unitOfWork.GetRepository<Promotion>().GetListAsync(
                predicate: x => x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == (int) PromotionStatusEnum.Automation
            );
            foreach (var prAuto in promotionAutos)
            {
                if (req.ChargeAmount >= prAuto.RequireAmount)
                {
                    req.PromoCode = prAuto.PromotionCode;
                    break;
                }
            }
            var packageItemExsit = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageItemId
            && x.Cccdpassport == req.CccdPassport, include: w => w.Include(wallet => wallet.Wallet).ThenInclude(w => w.WalletType).Include(z => z.Package).ThenInclude(i => i.PackageType)) 
                ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            if(packageItemExsit.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.NotFound);
            }
            if (packageItemExsit.Wallet.WalletType.Name == "SpecificWallet")
            {
                throw new BadHttpRequestException(PackageItemMessage.InvalidType, HttpStatusCodes.BadRequest);
            }
            #region check session
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageItemExsit.Package.PackageType.ZoneId
                               && x.StartDate <= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            if (packageItemExsit.Wallet.EndDate < TimeUtils.GetCurrentSEATime())
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
                    PaymentType = req.PaymentType,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                    Name = "Charge Money for: " + packageItemExsit.Name + "with balance: " + req.ChargeAmount,
                    Status = OrderStatus.Pending,
                    PackageItemId = req.PackageItemId,
                    UserId = userId,
                    SaleType = SaleType.PackageItemCharge,
                    TotalAmount = req.ChargeAmount,
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                string key = req.PaymentType + "_" + newOrder.InvoiceId;
                var packageOrder = new PackageOrder()
                {
                    Id = Guid.NewGuid(),
                    OrderId = newOrder.Id,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    CusCccdpassport = req.CccdPassport,
                    CusEmail = packageItemExsit.Email,
                    CusName = packageItemExsit.Name,
                    PackageId = packageItemExsit.PackageId,
                    PhoneNumber = packageItemExsit.PhoneNumber,
                    Status = OrderStatus.Pending,
                };
                await _unitOfWork.GetRepository<PackageOrder>().InsertAsync(packageOrder);
                var transactionCharge = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Amount = req.ChargeAmount,
                    OrderId = newOrder.Id,
                    Status = TransactionStatus.Pending,
                    Type = TransactionType.ChargeMoney,
                    WalletId = packageItemExsit.WalletId,
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Description = "Charge Money for: " + packageItemExsit.Name + "with balance: " + req.ChargeAmount,
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
                        packageOrderId = packageOrder.Id,
                        packageItemId = packageItemExsit.Id,
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
                if(checkPromo.Status != (int)PromotionStatusEnum.Automation)
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
                    if(req.ChargeAmount >= checkPromo.RequireAmount)
                    {
                        amountPromo = (int) checkPromo.MaxDiscount;
                    }
                }
                
                
                var newOrder = new Order()
                {
                    Id = Guid.NewGuid(),
                    PaymentType = req.PaymentType,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    InvoiceId = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                    Name = "Charge Money for: " + packageItemExsit.Name + "with balance: " + req.ChargeAmount,
                    Status = OrderStatus.Pending,
                    PackageItemId = req.PackageItemId,
                    UserId = userId,
                    SaleType = SaleType.PackageItemCharge,
                    TotalAmount = req.ChargeAmount - amountPromo,
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                var packageOrder = new PackageOrder()
                {
                    Id = Guid.NewGuid(),
                    OrderId = newOrder.Id,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    CusCccdpassport = req.CccdPassport,
                    CusEmail = packageItemExsit.Email,
                    CusName = packageItemExsit.Name,
                    PackageId = packageItemExsit.PackageId,
                    Status = OrderStatus.Pending,
                    PhoneNumber = packageItemExsit.PhoneNumber
                };
                await _unitOfWork.GetRepository<PackageOrder>().InsertAsync(packageOrder);
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
                    WalletId = packageItemExsit.WalletId,
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Description = "Charge Money for: " + packageItemExsit.Name + "with balance: " + req.ChargeAmount,
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
                        packageOrderId = packageOrder.Id,
                        packageItemId = packageItemExsit.Id,
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
                    (predicate: x => x.PromotionCode == promoCode && x.Status == (int)PromotionStatusEnum.Active || x.Status == (int) PromotionStatusEnum.Automation)
                    ?? throw new BadHttpRequestException(PromotionMessage.NotFoundPromotion, HttpStatusCodes.NotFound);
            if (checkPromo.EndDate < TimeUtils.GetCurrentSEATime())
                throw new BadHttpRequestException(PromotionMessage.PromotionExpired, HttpStatusCodes.BadRequest);
            if (checkPromo.Quantity <= 0)
                throw new BadHttpRequestException(PromotionMessage.PromotionOutOfStock, HttpStatusCodes.BadRequest);
            if(checkPromo.RequireAmount > amount)
                throw new BadHttpRequestException(PromotionMessage.PromotionRequireAmount, HttpStatusCodes.BadRequest);
            return checkPromo;
        }
        //MONEY AND EXPIRE 
        public async Task CheckPackageItemExpire()
        {
            var currentDate = TimeUtils.GetCurrentSEATime();
            var packageItems = (List<PackageItem>)await _unitOfWork.GetRepository<PackageItem>().GetListAsync
                (predicate: x => x.EndDate < currentDate && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum());
            foreach (var item in packageItems)
            {
                item.Status = PackageItemStatusEnum.Expired.GetDescriptionFromEnum();
                item.UpsDate = currentDate;
            }
            _unitOfWork.GetRepository<PackageItem>().UpdateRange(packageItems);
            await _unitOfWork.CommitAsync();
        }

        public async Task<ResponseAPI> PackageItemPayment(Guid packageItemId, int totalPrice, Guid storeId, List<OrderProduct> products)
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
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(predicate: x => x.Id == packageItemId 
                && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(), include: etag => etag.Include(y => y.Wallet))
                ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            if (packageItem.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.NotFound);
            }
            if (packageItem.Wallet.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = PackageItemMessage.PackageItemExpired,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (packageItem.Wallet.Balance < totalPrice)
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
                Name = "Payment From Store of: " + packageItem.Name,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                PaymentType = PaymentTypeHelper.allowedPaymentTypes[5],
                SaleType = SaleType.PackageItemPayment,
                Status = OrderStatus.Completed,
                TotalAmount = totalPrice,
                UserId = (Guid)store.UserStoreMappings.SingleOrDefault().UserId,
                PackageItemId = packageItem.Id,
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

            var newDeposit = new Deposit
            {
                Id = Guid.NewGuid(),
                Name = "Payment From Store of: " + packageItem.Name,
                Amount = totalPrice,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                PackageItemId = packageItem.Id,
                IsIncrease = false,
                PaymentType = PaymentTypeHelper.allowedPaymentTypes[5],
                WalletId = packageItem.WalletId,
                OrderId = order.Id
            };
            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);

            packageItem.Wallet.Balance -= totalPrice;
            packageItem.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageItem.Wallet);
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = totalPrice,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                WalletId = packageItem.WalletId,
                DespositId = newDeposit.Id,
                IsIncrease = false,
                Type = TransactionType.Payment.GetDescriptionFromEnum(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Description = "Payment From Store of: " + packageItem.Name,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                StoreId = storeId,
                UserId = (Guid)store.UserStoreMappings.SingleOrDefault().UserId
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
                Description = "Payment From Store of: " + packageItem.Name,
                IsIncrease = true,
                Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                WalletId = store.Wallets.SingleOrDefault().Id,
                Type = TransactionType.Payment.GetDescriptionFromEnum(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                UserId = (Guid)store.UserStoreMappings.SingleOrDefault().UserId,
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
                Description = "Payment From Store of: " + packageItem.Name,
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
            var packageItems = await _unitOfWork.GetRepository<PackageItem>().GetListAsync
                (predicate: x => x.Status == PackageItemStatusEnum.Expired.GetDescriptionFromEnum(),
                 include: w => w.Include(z => z.Wallet));
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
                foreach(var item in packageItems)
                {
                    if(item.Wallet.Balance > 0)
                    {
                        var transaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = item.WalletId,
                            IsIncrease = false,
                            Amount = item.Wallet.Balance,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Description = "Refund for: " + item.Name,
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
                            Amount = item.Wallet.Balance,
                            MarketZoneId = marketZone.Id,
                            IsIncrease = true,
                            Status = TransactionStatus.Success,
                            PackageItemId = item.Id,
                            TransactionId = transaction.Id
                        };
                        await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(transactionTransfer);

                        item.Wallet.Balance = 0;
                        item.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Wallet>().UpdateAsync(item.Wallet);

                        adminWallet.BalanceHistory += item.Wallet.Balance;
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

        public async Task <ResponseAPI> GetLostPackageItem(GetLostPackageItemRequest req)
        {
            //need authorize cashierWeb
            var searchName = NormalizeString(req.FullName);

            var packageItems = await _unitOfWork.GetRepository<PackageItem>().GetListAsync(
               predicate: x => x.Cccdpassport == req.Cccdpassport && x.Email == req.Email 
               && x.Status == PackageItemStatus.Active.GetDescriptionFromEnum(),
               include: p => p.Include(w => w.Wallet).Include(a => a.Package).ThenInclude(t => t.PackageType)
               );

            var packageItemLost = packageItems.SingleOrDefault(
                x => NormalizeString(x.Name) == searchName
               );
            if (packageItemLost == null)
            {
                throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            }
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageItemLost.Package.PackageType.ZoneId
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
            if(packageItemLost.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                throw new BadHttpRequestException(PackageItemMessage.PackageItemExpired, HttpStatusCodes.BadRequest);
            }
            //var ID = packageItemLost.Id;
            //after run api to get by cccd and mark as Blocked, transfer money, check log transaction
            //case lost vcard
            //check cus transfer to create new card
            packageItemLost.Status = PackageItemStatus.Blocked.GetDescriptionFromEnum();
            packageItemLost.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<PackageItem>().UpdateAsync(packageItemLost);
          
            //Transaction
            var newTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = packageItemLost.WalletId,
                IsIncrease = false,
                Amount = 50000,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Description = "Charge Fee From Lost PackageItem: " + packageItemLost.Name,
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Status = TransactionStatus.Pending,
                Type = TransactionType.TransferMoney,
                UserId = userId,
                //need to update with status is success & add deposit id above
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.SuccessfullyReadyToCreate,
                StatusCode = HttpStatusCodes.OK,
                Data = new { packageItemId = packageItemLost.Id }
            } : new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.FailedToMark,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        //assign new 
    }
}

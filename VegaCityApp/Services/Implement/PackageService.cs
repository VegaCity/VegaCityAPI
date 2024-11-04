using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
            var result = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name);
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
            var result = await _unitOfWork.GetRepository<PackageType>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name);
            if (result != null)
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
                include: package => package.Include(a => a.PackageDetails)
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
            var package = await SearchPackage(req.PackageId);
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == package.Data.PackageType.ZoneId
                               && x.StartDate >= TimeUtils.GetCurrentSEATime() && x.EndDate <= TimeUtils.GetCurrentSEATime()
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
                    WalletId = newWallet.Id
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

        //delete
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
                    Rfid = x.Rfid
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Package.PackageType.Zone.MarketZoneId == GetMarketZoneIdFromJwt(),
                include: x => x.Include(a => a.Package).ThenInclude(b => b.PackageType).ThenInclude(c => c.Zone)
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

        public async Task<ResponseAPI<PackageItem>> SearchPackageItem(Guid PackageItemId)
        {
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageItemId && x.Status == PackageItemStatus.Active.ToString(),
                include: packageItem => packageItem.Include(b => b.Reports)
                .Include(c => c.Orders).Include(d => d.Deposits).Include(z => z.CustomerMoneyTransfers)
                .Include(e => e.Wallet).ThenInclude(y => y.Transactions)
            );

            if (packageItem == null)
            {
                return new ResponseAPI<PackageItem>()
                {
                    MessageResponse = PackageItemMessage.NotFoundPackageItem,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI<PackageItem>()
            {
                MessageResponse = PackageItemMessage.GetPackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
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
            packageItem.Name = req.Name;
            packageItem.Gender = req.Gender;
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

        //active Package Item
        public async Task<ResponseAPI> ActivePackageItem(Guid packageItem, ActivatePackageItemRequest req)
        {

            //check if cccd or passport 
            if(!ValidationUtils.IsCCCD(req.Cccdpassport))
                throw new BadHttpRequestException(PackageItemMessage.CCCDInvalid, HttpStatusCodes.BadRequest);
            if (!ValidationUtils.IsEmail(req.Email))
                throw new BadHttpRequestException(PackageItemMessage.EmailInvalid, HttpStatusCodes.BadRequest);
            if(!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                throw new BadHttpRequestException(PackageItemMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);
            var packageItemExist = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(
                predicate: x => x.Id == packageItem && x.Status == PackageItemStatusEnum.Inactive.GetDescriptionFromEnum(), 
                include: z => z.Include(a => a.Package).ThenInclude(z => z.PackageType))
                    ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
            #region check 
            // after done main flow, make utils service to shorten this code
            var userId = GetUserIdFromJwt();
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.ZoneId == packageItemExist.Package.PackageType.ZoneId
                               && x.StartDate >= TimeUtils.GetCurrentSEATime() && x.EndDate >= TimeUtils.GetCurrentSEATime()
                               && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
            )
                ?? throw new BadHttpRequestException("You don't have permission to create package item because you don't have session",
                                                            HttpStatusCodes.BadRequest);
            #endregion
            if (req.Cccdpassport == packageItemExist.Cccdpassport)
                throw new BadHttpRequestException(PackageItemMessage.CCCDExist, HttpStatusCodes.BadRequest);
            if(req.Email == packageItemExist.Email)
                throw new BadHttpRequestException(PackageItemMessage.EmailExist, HttpStatusCodes.BadRequest);
            packageItemExist.Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum();
            packageItemExist.Name = req.Name.Trim();
            packageItemExist.PhoneNumber = req.PhoneNumber.Trim();
            packageItemExist.Cccdpassport = req.Cccdpassport.Trim();
            packageItemExist.Email = req.Email.Trim();
            packageItemExist.Gender = req.Gender.Trim();
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

        //CREATE MONEY REQUEST
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
            var packageItemExsit = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageItemId
            && x.Cccdpassport == req.CccdPassport, include: w => w.Include(wallet => wallet.Wallet).Include(z => z.Package).ThenInclude(i => i.PackageType)) 
                ?? throw new BadHttpRequestException(PackageItemMessage.NotFoundPackageItem, HttpStatusCodes.NotFound);
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
                var checkPromo = await _unitOfWork.GetRepository<Promotion>().SingleOrDefaultAsync
                    (predicate: x => x.PromotionCode == req.PromoCode && x.Status == (int)PromotionStatusEnum.Active)
                    ?? throw new BadHttpRequestException(PromotionMessage.NotFoundPromotion, HttpStatusCodes.NotFound);
                if (checkPromo.EndDate < TimeUtils.GetCurrentSEATime())
                    throw new BadHttpRequestException(PromotionMessage.PromotionExpired, HttpStatusCodes.BadRequest);
                if (checkPromo.Quantity <= 0)
                    throw new BadHttpRequestException(PromotionMessage.PromotionOutOfStock, HttpStatusCodes.BadRequest);
                var newPromotionOrder = new PromotionOrder()
                {
                    Id = Guid.NewGuid(),
                    PromotionId = checkPromo.Id,
                    OrderId = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Deflag = false,
                    DiscountAmount = req.ChargeAmount * (int)checkPromo.DiscountPercent
                };
                await _unitOfWork.GetRepository<PromotionOrder>().InsertAsync(newPromotionOrder);
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
                    TotalAmount = req.ChargeAmount + newPromotionOrder.DiscountAmount,
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

        //MONEY AND EXPIRE 
        //public async Task CheckEtagExpire()
        //{
        //    var currentDate = TimeUtils.GetCurrentSEATime();
        //    var etags = (List<Etag>)await _unitOfWork.GetRepository<Etag>().GetListAsync
        //        (predicate: x => x.EndDate < currentDate && x.Status == (int)EtagStatusEnum.Active && !x.Deflag);
        //    foreach (var etag in etags)
        //    {
        //        etag.Status = (int)EtagStatusEnum.Expired;
        //        etag.UpsDate = currentDate;
        //    }
        //    _unitOfWork.GetRepository<Etag>().UpdateRange(etags);
        //    await _unitOfWork.CommitAsync();
        //}

        public async Task<ResponseAPI> PackageItemPayment(Guid packageItemId, int price, Guid storeId, List<OrderProduct> products)
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
            if (packageItem.Wallet.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = PackageItemMessage.PackageItemExpired,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (packageItem.Wallet.Balance < price)
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
                TotalAmount = price,
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
                Amount = price,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                PackageItemId = packageItem.Id,
                IsIncrease = false,
                PaymentType = PaymentTypeHelper.allowedPaymentTypes[5],
                WalletId = packageItem.WalletId,
                OrderId = order.Id
            };
            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);

            packageItem.Wallet.Balance -= price;
            packageItem.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageItem.Wallet);
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = price,
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
                Amount = (int)(price * marketZone.MarketZoneConfig.StoreStranferRate),
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

            admin.Wallets.SingleOrDefault().Balance += (int)(price * marketZone.MarketZoneConfig.StoreStranferRate);
            admin.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(admin.Wallets.SingleOrDefault());

            var newStoreTransfer = new StoreMoneyTransfer
            {
                Id = Guid.NewGuid(),
                Amount = (int)(price * marketZone.MarketZoneConfig.StoreStranferRate),
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
                Amount = (int)(price * (1 - marketZone.MarketZoneConfig.StoreStranferRate)),
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

            store.Wallets.SingleOrDefault().Balance += (int)(price * (1 - marketZone.MarketZoneConfig.StoreStranferRate));
            store.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(store.Wallets.SingleOrDefault());

            var newStoreTransaction = new StoreMoneyTransfer
            {
                Id = Guid.NewGuid(),
                Amount = (int)(price * (1 - marketZone.MarketZoneConfig.StoreStranferRate)),
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

    }
}

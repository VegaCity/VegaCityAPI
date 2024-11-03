using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Etag;
using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
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

        public async Task<ResponseAPI> CreatePackage(CreatePackageRequest req)
        {
            var result = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name);
            if (result != null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.ExistedPackageName, 
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var newPackage = new Package()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                //StartDate = req.StartDate,
                //EndDate = req.EndDate,
                ImageUrl = req.ImageUrl,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
            };
            await _unitOfWork.GetRepository<Package>().InsertAsync(newPackage);
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
            DateTime currentDate = TimeUtils.GetCurrentSEATime();
            if (req.EndDate < currentDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.EndateInThePast,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate == req.StartDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.SameStrAndEndDate,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }

            TimeSpan? duration = req.EndDate - req.StartDate;
            if (duration.HasValue)
            {
                double totalHours = duration.Value.TotalHours;
                if (totalHours < 48)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = PackageMessage.durationLimit,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.InvalidDuration,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            package.Name = req.Name;
            package.Description = req.Description;
            //package.Price = req.Price;
            //package.StartDate = req.StartDate;
            //package.EndDate = req.EndDate;
            package.ImageUrl = req.ImageUrl;
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
        public async Task<ResponseAPI> SearchPackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageId && x.Deflag==false,
                include: package => package.Include(a => a.PackageDetails).Include(b => b.PackageOrders).Include(c => c.PackageItems)
            );

            if (package == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.NotFoundPackage,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = PackageMessage.GetPackagesSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                   package
                }
            };
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
            //delete mapping
            //if(package.PackageETagTypeMappings.Count > 0)
            //{
            //    foreach (var item in package.PackageETagTypeMappings)
            //    {
            //        _unitOfWork.GetRepository<PackageETagTypeMapping>().DeleteAsync(item);
            //    }
            //}
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

        //Create PackageType
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

       // Update PackageType
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
            //package.Description = req.Description;
            //package.Price = req.Price;
            //package.StartDate = req.StartDate;
            //package.EndDate = req.EndDate;
            //packageType.MarketZoneId = req.MarketZoneId ?? packageType.MarketZoneId;
            packageType.ZoneId = req.ZoneId;
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

        public async Task<ResponseAPI> SearchPackageType(Guid PackageTypeId)
        {
            var packageType = await _unitOfWork.GetRepository<PackageType>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageTypeId && x.Deflag == false,
                include: package => package.Include(y => y.Packages)
            );

            if (packageType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageTypeMessage.NotFoundPackageType,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = PackageTypeMessage.GetPackageTypesSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    packageType
                }
            };
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
            //delete mapping
            //if(package.PackageETagTypeMappings.Count > 0)
            //{
            //    foreach (var item in package.PackageETagTypeMappings)
            //    {
            //        _unitOfWork.GetRepository<PackageETagTypeMapping>().DeleteAsync(item);
            //    }
            //}
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
        //create need fix
        //public async Task<ResponseAPI> CreatePackageItem(CreatePackageItemRequest req) //include EtagDetail too
        //{
        //    //check if cccd or passport 
        //    if (!string.IsNullOrEmpty(req.CCCDPassport))
        //    {
        //        if (req.CCCDPassport.Length == 12 && long.TryParse(req.CCCDPassport, out _))
        //        {
        //            if (!ValidationUtils.IsCCCD(req.CCCDPassport))
        //            {
        //                return new ResponseAPI()
        //                {
        //                    MessageResponse = EtagMessage.CCCDInvalid,
        //                    StatusCode = HttpStatusCodes.BadRequest
        //                };
        //            }
        //        }
        //        else if (req.CCCDPassport.Length == 8 || req.CCCDPassport.Length == 9)
        //        {
        //            req.CCCDPassport = req.CCCDPassport.Length == 8
        //                ? "0000" + req.CCCDPassport
        //                : "000" + req.CCCDPassport;
        //        }
        //        else
        //        {
        //            // Handle case if neither CCCD nor a valid passport format
        //            return new ResponseAPI
        //            {
        //                MessageResponse = EtagMessage.CCCDInvalid,
        //                StatusCode = HttpStatusCodes.BadRequest
        //            };
        //        }
        //    }
        //    //end check valid cccd or passport
        //    //if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
        //    //{
        //    //    return new ResponseAPI()
        //    //    {
        //    //        MessageResponse = EtagMessage.PhoneNumberInvalid,
        //    //        StatusCode = HttpStatusCodes.BadRequest
        //    //    };
        //    //}
        //    var packageAvailable = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageId
        //    , include: x => x.Include(a => a.Price).Include(y => y.PackageType).Include(b => b.PackageDetails).ThenInclude(z => z.WalletType));
        //    if (packageAvailable == null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = PackageMessage.NotFoundPackage,
        //            StatusCode = HttpStatusCodes.NotFound
        //        };
        //    }

        //    var newWallet = new Wallet
        //    {
        //        Id = Guid.NewGuid(),
        //        Balance = (int)(etagType.Amount + (etagType.Amount * etagType.BonusRate)),
        //        BalanceHistory = (int)(etagType.Amount + (etagType.Amount * etagType.BonusRate)),
        //        CrDate = TimeUtils.GetCurrentSEATime(),
        //        UpsDate = TimeUtils.GetCurrentSEATime(),
        //        Deflag = false,
        //        WalletTypeId = etagType.WalletType.Id,
        //        StartDate = req.StartDate,
        //        ExpireDate = req.EndDate
        //    };
        //    await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
        //    var newEtag = new Etag
        //    {
        //        Id = Guid.NewGuid(),
        //        EtagTypeId = etagType.Id,
        //        MarketZoneId = etagType.MarketZoneId,
        //        WalletId = newWallet.Id,
        //        Deflag = false,
        //        CrDate = TimeUtils.GetCurrentSEATime(),
        //        UpsDate = TimeUtils.GetCurrentSEATime(),
        //        //Cccd = req.Cccd,
        //        //FullName = req.FullName,
        //        //PhoneNumber = req.PhoneNumber,
        //        //Gender = req.Gender,
        //        EtagCode = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
        //        StartDate = req.StartDate,
        //        EndDate = req.EndDate,
        //        Status = (int)EtagStatusEnum.Active,
        //        // IsVerifyPhone = false,
        //        IsAdult = true,
        //    };
        //    newEtag.Qrcode = EnCodeBase64.EncodeBase64Etag(newEtag.EtagCode);
        //    await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
        //    //etag detail below here 
        //    var newEtagDetail = new EtagDetail
        //    {
        //        Id = Guid.NewGuid(),
        //        EtagId = newEtag.Id,
        //        FullName = req.FullName,
        //        PhoneNumber = req.PhoneNumber,
        //        CccdPassport = req.CccdPassport,
        //        Gender = req.Gender,
        //        IsVerifyPhone = false,
        //        CrDate = TimeUtils.GetCurrentSEATime(),
        //        UpsDate = TimeUtils.GetCurrentSEATime()
        //    };
        //    await _unitOfWork.GetRepository<EtagDetail>().InsertAsync(newEtagDetail);
        //    return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
        //    {
        //        MessageResponse = EtagMessage.CreateSuccessFully,
        //        StatusCode = HttpStatusCodes.Created,
        //        Data = new { etagId = newEtag.Id, walletId = newWallet.Id }
        //    } : new ResponseAPI()
        //    {
        //        MessageResponse = EtagTypeMessage.CreateFail,
        //        StatusCode = HttpStatusCodes.BadRequest
        //    };
        //}        //update
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
                    CCCDPassport = x.Cccdpassport,
                    Email = x.Email,
                    Status = x.Status,
                    Gender = GenderEnum.Male.ToString(),
                    IsAdult = x.IsAdult,
                    WalletId = x.WalletId,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,     
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Status == PackageItemStatus.Active.ToString()
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

        public async Task<ResponseAPI> SearchPackageItem(Guid PackageItemId)
        {
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageItemId && x.Status == PackageItemStatus.Active.ToString(),
                include: packageItem => packageItem.Include(b => b.Reports)
                .Include(c => c.Orders).Include(d => d.Deposits).Include(z => z.CustomerMoneyTransfers)
                .Include(e => e.Wallet).ThenInclude(y => y.Transactions)
            );

            if (packageItem == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.NotFoundPackageItem,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = PackageItemMessage.GetPackageItemSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    packageItem
                }
            };
        }

        public async Task<ResponseAPI> UpdatePackageItem(Guid packageItemId, UpdatePackageItemRequest req)
        {
            var packageItem = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync(predicate: x => x.Id == packageItemId && x.Status == PackageItemStatus.Active.ToString()
            //&& x.IsChange == false    
            );
            if (packageItem == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = PackageItemMessage.NotFoundPackageItem,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            //update 
            packageItem.Name = req.Name;
            packageItem.Gender = req.Gender;
            // packageItem.IsChange = true;

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
        //public async Task<ResponseAPI> ActivePackageItem(Guid etagId, ActivatePackageItemRequest req)
        //{
        //    //check if cccd or passport 
        //    if (!string.IsNullOrEmpty(req.CccdPassport))
        //    {
        //        if (req.CccdPassport.Length == 12 && long.TryParse(req.CccdPassport, out _))
        //        {
        //            if (!ValidationUtils.IsCCCD(req.CccdPassport))
        //            {
        //                return new ResponseAPI()
        //                {
        //                    MessageResponse = EtagMessage.CCCDInvalid,
        //                    StatusCode = HttpStatusCodes.BadRequest
        //                };
        //            }
        //        }
        //        else if (req.CccdPassport.Length == 8 || req.CccdPassport.Length == 9)
        //        {
        //            req.CccdPassport = req.CccdPassport.Length == 8
        //                ? "0000" + req.CccdPassport
        //                : "000" + req.CccdPassport;
        //        }
        //        else
        //        {
        //            // Handle case if neither CCCD nor a valid passport format
        //            return new ResponseAPI
        //            {
        //                MessageResponse = EtagMessage.CCCDInvalid,
        //                StatusCode = HttpStatusCodes.BadRequest
        //            };
        //        }
        //    }
        //    //end check valid cccd or passport
        //    if (!ValidationUtils.IsPhoneNumber(req.Phone))
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.PhoneNumberInvalid,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(
        //        predicate: x => x.Id == etagId && !x.Deflag && x.Status == (int)EtagStatusEnum.Inactive,
        //        include: etag => etag.Include(y => y.EtagDetail));
        //    if (etag == null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.NotFoundEtag,
        //            StatusCode = HttpStatusCodes.NotFound
        //        };
        //    }
        //    //BELOW CHECK FROM ETAG DETAIL (INCLUDE ETAG DETAIL FROM ETAG)
        //    var checkPhone = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagDetail.PhoneNumber == req.Phone && !x.Deflag);
        //    if (checkPhone != null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.PhoneNumberExist,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    var checkCCCD = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagDetail.CccdPassport == req.CccdPassport && !x.Deflag);
        //    if (checkCCCD != null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.CCCDExist,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    etag.Status = (int)EtagStatusEnum.Active;
        //    //etag.FullName = req.Name;
        //    //etag.PhoneNumber = req.Phone; 
        //    //etag.Cccd = req.CCCD; //ETAG DETAIL
        //    var etagDetail = etag.EtagDetail;
        //    etagDetail.FullName = req.Name;
        //    etagDetail.PhoneNumber = req.Phone;
        //    etagDetail.CccdPassport = req.CccdPassport;

        //    etag.UpsDate = TimeUtils.GetCurrentSEATime();

        //    //etag.Gender = req.Gender;
        //    //etag.Birthday = req.Birthday; ETAG DETAIL
        //    etagDetail.Gender = req.Gender;
        //    etagDetail.Birthday = req.Birthday;
        //    etag.StartDate = req.StartDate ?? etag.StartDate;
        //    etag.EndDate = req.EndDate ?? etag.EndDate;
        //    _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
        //    return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
        //    {
        //        MessageResponse = EtagMessage.ActivateEtagSuccess,
        //        StatusCode = HttpStatusCodes.OK,
        //        Data = new { etagId = etag.Id }
        //    } : new ResponseAPI()
        //    {
        //        MessageResponse = EtagMessage.ActivateEtagFail,
        //        StatusCode = HttpStatusCodes.BadRequest
        //    };
        //}

        //CREATE MONEY REQUEST
        //public async Task<ResponseAPI> PrepareChargeMoneyEtag(ChargeMoneyEtagRequest req)
        //{
        //    if (!ValidationUtils.CheckNumber(req.ChargeAmount))
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = OrderMessage.TotalAmountInvalid,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    if (!ValidationUtils.IsCCCD(req.CccdPassport))
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.CCCDInvalid,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    //get user id from token
        //    Guid userId = GetUserIdFromJwt();
        //    var etagExist = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagCode == req.EtagCode && !x.Deflag
        //    && x.EtagDetail.CccdPassport == req.CccdPassport // FROM ETAG DETAIL HERE TOO
        //         , include: etag => etag.Include(y => y.Wallet).Include(y => y.EtagDetail));
        //    if (etagExist == null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.NotFoundEtag,
        //            StatusCode = HttpStatusCodes.NotFound,
        //        };
        //    }
        //    if (etagExist.Wallet.ExpireDate < TimeUtils.GetCurrentSEATime() || etagExist.EndDate < TimeUtils.GetCurrentSEATime())
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = EtagMessage.EtagExpired,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    if (PaymentTypeHelper.allowedPaymentTypes.Contains(req.PaymentType) == false)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = OrderMessage.PaymentTypeInvalid,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    var newOrder = new Order()
        //    {
        //        Id = Guid.NewGuid(),
        //        PaymentType = req.PaymentType,
        //        Name = "Charge Money for Etag: " + etagExist.EtagDetail.FullName, //FULL NAME FROM ETAG DETAIL
        //        TotalAmount = req.ChargeAmount,
        //        CrDate = TimeUtils.GetCurrentSEATime(),
        //        UpsDate = TimeUtils.GetCurrentSEATime(),
        //        Status = OrderStatus.Pending,
        //        InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
        //        EtagId = etagExist.Id,
        //        SaleType = SaleType.EtagCharge,
        //        UserId = userId,
        //    };

        //    await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
        //    string key = req.PaymentType + "_" + newOrder.InvoiceId;
        //    return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
        //    {
        //        MessageResponse = EtagMessage.CreateOrderForCharge,
        //        StatusCode = HttpStatusCodes.OK,
        //        Data = new
        //        {
        //            invoiceId = newOrder.InvoiceId,
        //            balance = req.ChargeAmount,
        //            Key = key,
        //            UrlDirect = $"https://api.vegacity.id.vn/api/v1/payment/{req.PaymentType.ToLower()}/order/charge-money", //https://localhost:44395/api/v1/payment/momo/order, http://14.225.204.144:8000/api/v1/payment/momo/order
        //            UrlIpn = $"https://vegacity.id.vn/order-status?status=success&orderId={newOrder.Id}"
        //        }
        //    } : new ResponseAPI()
        //    {
        //        MessageResponse = EtagMessage.CreateOrderForChargeFail,
        //        StatusCode = HttpStatusCodes.BadRequest
        //    };
        //}

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

        //public async Task<ResponseAPI> EtagPayment(string etagCode, int price, Guid storeId)
        //{
        //    var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => !x.Deflag && x.Id == storeId && x.Status == (int)StoreStatusEnum.Opened);
        //    if (store == null)
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = StoreMessage.NotFoundStore,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagCode == etagCode && x.Status == (int)EtagStatusEnum.Active, include: etag => etag.Include(y => y.Wallet));
        //    if (etag.EndDate <= TimeUtils.GetCurrentSEATime())
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = EtagMessage.EtagExpired,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    if (etag.Wallet.ExpireDate <= TimeUtils.GetCurrentSEATime())
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = WalletTypeMessage.WalletExpired,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    if (etag.Wallet.Balance < price)
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = WalletTypeMessage.NotEnoughBalance,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    if (etag.Deflag)
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = EtagMessage.NotFoundEtag,
        //            StatusCode = HttpStatusCodes.BadRequest
        //        };
        //    }
        //    etag.Wallet.Balance -= price;
        //    etag.UpsDate = TimeUtils.GetCurrentSEATime();
        //    _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);

        //    var newDeposit = new Deposit
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = "Payment From Store",
        //        Amount = price,
        //        CrDate = TimeUtils.GetCurrentSEATime(),
        //        UpsDate = TimeUtils.GetCurrentSEATime(),
        //        EtagId = etag.Id,
        //        IsIncrease = false,
        //        PaymentType = PaymentTypeHelper.allowedPaymentTypes[5],
        //        WalletId = etag.WalletId,
        //        StoreId = store.Id
        //    };
        //    etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
        //    _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
        //    await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
        //    return await _unitOfWork.CommitAsync() > 0
        //       ? new ResponseAPI()
        //       {
        //           StatusCode = HttpStatusCodes.OK,
        //           MessageResponse = EtagMessage.PaymentQrCodeSuccess
        //       }
        //       : new ResponseAPI()
        //       {
        //           StatusCode = HttpStatusCodes.InternalServerError,
        //           MessageResponse = EtagMessage.FailedToPay
        //       };
        //}
    }
}

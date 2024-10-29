using AutoMapper;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Etag;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class EtagService : BaseService<EtagService>, IEtagService
    {
        private readonly IWalletTypeService _walletTypeService;
        private readonly IPackageService _packageService;

        public EtagService(IUnitOfWork<VegaCityAppContext> unitOfWork, 
                           ILogger<EtagService> logger, 
                           IHttpContextAccessor httpContextAccessor, 
                           IMapper mapper,
                           IWalletTypeService walletTypeService,
                           IPackageService packageService) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _walletTypeService = walletTypeService;
            _packageService = packageService;
        }

        public async Task<ResponseAPI> CreateEtagType(EtagTypeRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            //check wallet type exist
            var walletType = await _walletTypeService.GetWalletTypeById(req.WalletTypeId) ?? throw new BadHttpRequestException(WalletTypeMessage.NotFoundWalletType);
            var newEtagType = new EtagType
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                ImageUrl = req.ImageUrl,
                MarketZoneId = apiKey,
                BonusRate = req.BonusRate,
                Deflag = false,
                Amount = req.Amount,
                WalletTypeId = req.WalletTypeId
            };
            await _unitOfWork.GetRepository<EtagType>().InsertAsync(newEtagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.CreateSuccessFully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    EtagTypeId = newEtagType.Id,
                }
            } : throw new BadHttpRequestException(EtagMessage.CreateFail);
        }
        public async Task<ResponseAPI> UpdateEtagType(Guid etagTypeId, UpdateEtagTypeRequest req)
        {
            var etagType = await SearchEtagType(etagTypeId) ?? throw new BadHttpRequestException(EtagTypeMessage.NotFoundEtagType);
            etagType.Name = req.Name;
            etagType.ImageUrl = req.ImageUrl ?? etagType.ImageUrl;
            etagType.BonusRate = req.BonusRate ?? etagType.BonusRate;
            etagType.Amount = req.Amount ?? etagType.Amount;
            _unitOfWork.GetRepository<EtagType>().UpdateAsync(etagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.UpdateSuccessFully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etagTypeId = etagType.Id }
            } : throw new BadHttpRequestException(EtagTypeMessage.UpdateFail);
        }
        public async Task<ResponseAPI> DeleteEtagType(Guid etagTypeId)
        {
            var etagType = await SearchEtagType(etagTypeId) ?? throw new BadHttpRequestException(EtagTypeMessage.NotFoundEtagType);
            if (etagType.PackageETagTypeMappings.Count > 0)
            {
                foreach (var item in etagType.PackageETagTypeMappings)
                {
                    _unitOfWork.GetRepository<PackageETagTypeMapping>().DeleteAsync(item);
                }
            }
            etagType.Deflag = true;
            _unitOfWork.GetRepository<EtagType>().UpdateAsync(etagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.DeleteEtagTypeSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etagTypeId = etagType.Id }
            } : new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.DeleteEtagTypeFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<EtagType> SearchEtagType(Guid etagTypeId)
        {
            EtagType etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == etagTypeId && !x.Deflag,
                include: etag => etag.Include(y => y.Etags).Include(y => y.WalletType));
            return etagType;
        }
        public async Task<ResponseAPI<IEnumerable<EtagTypeResponse>>> SearchAllEtagType(int size, int page)
        {
            try
            {
                IPaginate<EtagTypeResponse> data = await _unitOfWork.GetRepository<EtagType>().GetPagingListAsync(

                selector: x => new EtagTypeResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    MarketZoneId = x.MarketZoneId,
                    ImageUrl = x.ImageUrl,
                    BonusRate = x.BonusRate,
                    Deflag = x.Deflag,
                    Amount = (int)x.Amount
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(y => y.Name),
                predicate: x => !x.Deflag && x.MarketZoneId == GetMarketZoneIdFromJwt()
                );

                return new ResponseAPI<IEnumerable<EtagTypeResponse>>()
                {
                    MessageResponse = EtagTypeMessage.SearchAllEtagTypeSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = data.Items,  // Danh sách EtagType trả về
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
                return new ResponseAPI<IEnumerable<EtagTypeResponse>>()
                {
                    MessageResponse = EtagTypeMessage.SearchAllEtagTypeFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null // Không có metadata khi lỗi xảy ra
                };
            }
        }
        public async Task<ResponseAPI> AddEtagTypeToPackage(Guid etagTypeId, Guid packageId, int quantityEtagType)
        {
            var etagType = await SearchEtagType(etagTypeId) ?? throw new BadHttpRequestException(EtagTypeMessage.NotFoundEtagType, HttpStatusCodes.NotFound);
            var package = await _packageService.SearchPackage(packageId) ?? throw new BadHttpRequestException(PackageMessage.NotFoundPackage, HttpStatusCodes.NotFound);
            
            var etagPackage = new PackageETagTypeMapping()
            {
                Id = Guid.NewGuid(),
                EtagTypeId = etagType.Id,
                PackageId = package.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                QuantityEtagType = quantityEtagType
            };
            await _unitOfWork.GetRepository<PackageETagTypeMapping>().InsertAsync(etagPackage);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.CreateSuccessFully,
                StatusCode = HttpStatusCodes.Created,
                Data = new { etagPackageId = etagPackage.Id }
            } : throw new BadHttpRequestException(EtagTypeMessage.CreateFail);
        }
        public async Task<ResponseAPI> RemoveEtagTypeFromPackage(Guid etagId, Guid packageId)
        {
            var packageEtagType = await _unitOfWork.GetRepository<PackageETagTypeMapping>().SingleOrDefaultAsync
                (predicate: x => x.EtagTypeId == etagId && x.PackageId == packageId)?? throw new BadHttpRequestException(EtagTypeMessage.NotFoundEtagType, HttpStatusCodes.NotFound);
            _unitOfWork.GetRepository<PackageETagTypeMapping>().DeleteAsync(packageEtagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.DeleteEtagTypeSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etagPackageId = packageEtagType.Id }
            } : throw new BadHttpRequestException("Delete EtagType from Package fail!!");
        }
        public async Task<ResponseAPI> CreateEtag(EtagRequest req) //include EtagDetail too
        {
            if (!ValidationUtils.IsCCCD(req.CccdPassport.Trim())) throw new BadHttpRequestException(EtagMessage.CCCDInvalid, HttpStatusCodes.BadRequest);
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber.Trim())) throw new BadHttpRequestException(EtagMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);
            var etagType = await SearchEtagType(req.EtagTypeId) ?? throw new BadHttpRequestException(EtagTypeMessage.NotFoundEtagType, HttpStatusCodes.NotFound);

            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Balance = (int)(etagType.Amount * (1 +  etagType.BonusRate)),
                BalanceHistory = (int)(etagType.Amount * (1 + etagType.BonusRate)),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false,
                WalletTypeId = etagType.WalletType.Id,
                StartDate = req.StartDate,
                ExpireDate = req.EndDate
            };
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
            var newEtag = new Etag
            {
                Id = Guid.NewGuid(),
                EtagTypeId = etagType.Id,
                MarketZoneId = etagType.MarketZoneId,
                WalletId = newWallet.Id,
                Deflag = false,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                //Cccd = req.Cccd,
                //FullName = req.FullName,
                //PhoneNumber = req.PhoneNumber,
                //Gender = req.Gender,
                EtagCode = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Status = (int)EtagStatusEnum.Active,
                // IsVerifyPhone = false,
                IsAdult = true,
            };
            newEtag.Qrcode = EnCodeBase64.EncodeBase64Etag(newEtag.EtagCode);
            await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
            var newEtagDetail = new EtagDetail
            {
                Id = Guid.NewGuid(),
                EtagId = newEtag.Id,
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                CccdPassport = req.CccdPassport,
                Gender = req.Gender,
                IsVerifyPhone = false, 
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<EtagDetail>().InsertAsync(newEtagDetail);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagMessage.CreateSuccessFully,
                StatusCode = HttpStatusCodes.Created,
                Data = new { etagId = newEtag.Id, walletId = newWallet.Id }
            } : new ResponseAPI()
            {
                MessageResponse = EtagTypeMessage.CreateFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI<List<Guid>>> GenerateEtag(int quantity, Guid etagTypeId, GenerateEtagRequest req)
        {
            List<Guid> listEtagCreated = new List<Guid>();
            var checkEtagType = await SearchEtagType(etagTypeId) ?? throw new BadHttpRequestException(EtagTypeMessage.NotFoundEtagType, HttpStatusCodes.NotFound);
            //generate etag
            for (int i = 0; i < quantity; i++)
            {
                // create wallet for etag
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    WalletTypeId = checkEtagType.WalletType.Id,
                    Balance = (int)(checkEtagType.Amount *(1 + checkEtagType.BonusRate)),
                    BalanceHistory = (int)(checkEtagType.Amount * (1 + checkEtagType.BonusRate)),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Deflag = false
                };
                await _unitOfWork.GetRepository<Wallet>().InsertAsync(wallet);
                // create etag
                if(req.EtagId != null) //check 
                {
                    var etagParent = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagId && x.Status==(int)EtagStatusEnum.Active && x.IsAdult == true,
                                                                                                   include: etag => etag.Include(z => z.EtagDetail));           
                        /// generate etag child based on amount and fields from etag details of parent
                        var newEtagChild = new Etag
                        {
                            Id = Guid.NewGuid(),
                            //FullName = "User VegaCity",
                            //PhoneNumber = "",
                            //Cccd = "",
                            //ImageUrl = "",
                            //Gender = (int)GenderEnum.Other, //THOSE IN ETAG DETAIL
                            EtagCode = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Deflag = false,
                            EtagTypeId = checkEtagType.Id,
                            MarketZoneId = checkEtagType.MarketZoneId,
                            WalletId = wallet.Id,
                            StartDate = etagParent.StartDate,
                            EndDate = req.EndDate,
                            Status = (int)EtagStatusEnum.Inactive,
                            //IsVerifyPhone = false,
                            IsAdult = false,
                        };
                        newEtagChild.Qrcode = EnCodeBase64.EncodeBase64Etag(newEtagChild.EtagCode);
                        await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtagChild);
                        var newEtagChildDetail = new EtagDetail
                        {
                            Id = Guid.NewGuid(),
                            EtagId = newEtagChild.Id,
                            FullName = "User VegaCity", // Default or provided value
                            PhoneNumber = etagParent.EtagDetail.PhoneNumber, // Default or provided value
                            CccdPassport = etagParent.EtagDetail.CccdPassport, // Default or provided value
                            Gender = (int)GenderEnum.Other, // Default or provided value
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            IsVerifyPhone = etagParent.EtagDetail.IsVerifyPhone // Assuming the phone isn't verified initially
                        };
                    await _unitOfWork.GetRepository<EtagDetail>().InsertAsync(newEtagChildDetail);
                   // await _unitOfWork.CommitAsync();
                    listEtagCreated.Add(newEtagChild.Id);
                    //end parent here
                }
                else
                {
                    var newEtag = new Etag
                    {
                        Id = Guid.NewGuid(),
                        //FullName = "User VegaCity",
                        //PhoneNumber = "",
                        //Cccd = "",
                        //ImageUrl = "",
                        //Gender = (int)GenderEnum.Other, //THOSE IN ETAG DETAIL
                        EtagCode = "VGC" + TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Deflag = false,
                        EtagTypeId = checkEtagType.Id,
                        MarketZoneId = checkEtagType.MarketZoneId,
                        WalletId = wallet.Id,
                        StartDate = req.StartDate,
                        EndDate = req.EndDate,
                        Status = (int)EtagStatusEnum.Inactive,
                        //IsVerifyPhone = false,
                        IsAdult = true,
                    };
                    newEtag.Qrcode = EnCodeBase64.EncodeBase64Etag(newEtag.EtagCode);
                    await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
                    var newEtagDetail = new EtagDetail
                    {
                        Id = Guid.NewGuid(),
                        EtagId = newEtag.Id,
                        FullName = "User VegaCity", // Default or provided value
                        PhoneNumber = "", // Default or provided value
                        CccdPassport = "", // Default or provided value
                        Gender = (int)GenderEnum.Other, // Default or provided value
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        IsVerifyPhone = false // Assuming the phone isn't verified initially
                    };
                    //end create etag here
                    await _unitOfWork.GetRepository<EtagDetail>().InsertAsync(newEtagDetail);
                    listEtagCreated.Add(newEtag.Id);
                } 
            }
            await _unitOfWork.CommitAsync();


            return new ResponseAPI<List<Guid>>()
            {
                MessageResponse = EtagTypeMessage.CreateSuccessFully,
                StatusCode = HttpStatusCodes.Created,
                Data = listEtagCreated
            };
        }
        public async Task<ResponseAPI> UpdateEtag(Guid etagId, UpdateEtagRequest req)
        {
            //var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == etagId && !x.Deflag);
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == etagId && !x.Deflag,
                include: etag => etag.Include(y => y.EtagDetail)
            );
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            //if(!ValidationUtils.IsCCCD(req.CCCD))
            //{
            //    return new ResponseAPI()
            //    {
            //        MessageResponse = EtagMessage.CCCDInvalid,
            //        StatusCode = HttpStatusCodes.BadRequest
            //    };
            //}
            if(!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.PhoneNumberInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            //BELOW UPDATE ETAG DETAIL instead of etag ban dau
            //etag.FullName = req.Fullname ?? etag.FullName;
            //etag.PhoneNumber = req.PhoneNumber ?? etag.PhoneNumber;
            //etag.Cccd = req.CCCD ?? etag.Cccd;
            // etag.ImageUrl = req.ImageUrl ?? etag.ImageUrl;
            //etag.Birthday = req.DateOfBirth ?? etag.Birthday;
            //etag.Gender = req.Gender ?? etag.Gender;
            //  etag.UpsDate = TimeUtils.GetCurrentSEATime();
            // _unitOfWork.GetRepository<Etag>().UpdateAsync(etag
            var etagDetail = etag.EtagDetail;
            if (etagDetail != null)
            {
                etagDetail.FullName = req.Fullname ?? etagDetail.FullName;
                etagDetail.PhoneNumber = req.PhoneNumber ?? etagDetail.PhoneNumber;
                //etagDetail.CccdPassport = req.CCCD ?? etagDetail.CccdPassport;
                etagDetail.Gender = req.Gender ?? etagDetail.Gender;
                etagDetail.Birthday = req.DateOfBirth ?? etagDetail.Birthday;
                etagDetail.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<EtagDetail>().UpdateAsync(etagDetail);
            }
            //update etag
            etag.ImageUrl = req.ImageUrl ?? etag.ImageUrl;
            etag.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagMessage.UpdateSuccessFully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etagId = etag.Id }
            } : new ResponseAPI()
            {
                MessageResponse = EtagMessage.UpdateFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> DeleteEtag(Guid etagId)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == etagId && !x.Deflag,
                                                                                    include: wallet => wallet.Include(z => z.Wallet));
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            etag.Deflag = true;
            etag.Status =(int) EtagStatusEnum.Inactive;
            etag.Wallet.Deflag = true;
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
            etag.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagMessage.DeleteEtagSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etagId = etag.Id }
            } : new ResponseAPI()
            {
                MessageResponse = EtagMessage.DeleteEtagFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> SearchEtag(Guid? etagId, string? etagCode)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => (x.Id == etagId || x.EtagCode == etagCode) && !x.Deflag,
                include: etag => etag.Include(y => y.EtagType)
                        .Include(y => y.Wallet)
                        .Include(y => y.EtagDetail)
                        .Include(y => y.Orders)
                        .Include(y => y.MarketZone));
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            if (etag.Wallet == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = "Etag Wallet is deleted !!",
                    Data = new { etag }
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = EtagMessage.SearchEtagSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etag }
            };
        }

        public async Task<ResponseAPI<IEnumerable<EtagResponse>>> SearchAllEtag(int size, int page)
        {
            try
            {
                IPaginate<EtagResponse> data = await _unitOfWork.GetRepository<Etag>().GetPagingListAsync(
                              selector: x => new EtagResponse()
                              {
                                  Id = x.Id,
                                  FullName = x.EtagDetail.FullName,
                                  PhoneNumber = x.EtagDetail.PhoneNumber,
                                  CccdPassport = x.EtagDetail.CccdPassport,
                                  ImageUrl = x.ImageUrl,
                                  EtagCode = x.EtagCode,
                                  QRCode = x.Qrcode,
                                  Birthday = x.EtagDetail.Birthday,
                                  Gender = x.EtagDetail.Gender, 
                                  Deflag = x.Deflag,
                                  EndDate = x.EndDate,
                                  StartDate = x.StartDate,
                                  Status = x.Status,
                                  IsAdult = x.IsAdult
                              },
                               page: page,
                               size: size,
                               orderBy: x => x.OrderByDescending(z => z.StartDate),
                               predicate: x => !x.Deflag,
                               include: etag => etag.Include(y => y.EtagDetail));
                return new ResponseAPI<IEnumerable<EtagResponse>>
                {
                    MessageResponse = EtagMessage.SearchAllEtagsSuccess,
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
                return new ResponseAPI<IEnumerable<EtagResponse>>()
                {
                    MessageResponse = EtagMessage.SearchAllEtagsFailed + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> ActivateEtag(Guid etagId, ActivateEtagRequest req)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(
                predicate: x => x.Id == etagId && !x.Deflag && x.Status ==(int) EtagStatusEnum.Inactive,
                include: etag => etag.Include(y => y.EtagDetail));
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            if(!ValidationUtils.IsCCCD(req.CccdPassport))
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.CCCDInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (!ValidationUtils.IsPhoneNumber(req.Phone))
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.PhoneNumberInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            //BELOW CHECK FROM ETAG DETAIL (INCLUDE ETAG DETAIL FROM ETAG)
            var checkPhone = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagDetail.PhoneNumber == req.Phone && !x.Deflag);
            if (checkPhone != null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.PhoneNumberExist,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var checkCCCD = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagDetail.CccdPassport == req.CccdPassport && !x.Deflag);
            if (checkCCCD != null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.CCCDExist,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            etag.Status = (int)EtagStatusEnum.Active;
            //etag.FullName = req.Name;
            //etag.PhoneNumber = req.Phone; 
            //etag.Cccd = req.CCCD; //ETAG DETAIL
            var etagDetail = etag.EtagDetail;
            etagDetail.FullName = req.Name;
            etagDetail.PhoneNumber= req.Phone;
            etagDetail.CccdPassport = req.CccdPassport;

            etag.UpsDate = TimeUtils.GetCurrentSEATime();

            //etag.Gender = req.Gender;
            //etag.Birthday = req.Birthday; ETAG DETAIL
            etagDetail.Gender = req.Gender;
            etagDetail.Birthday = req.Birthday;
            etag.StartDate = req.StartDate ?? etag.StartDate;
            etag.EndDate = req.EndDate ?? etag.EndDate;
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagMessage.ActivateEtagSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new { etagId = etag.Id }
            } : new ResponseAPI()
            {
                MessageResponse = EtagMessage.ActivateEtagFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }

        public async Task<ResponseAPI> PrepareChargeMoneyEtag(ChargeMoneyEtagRequest req)
        {
            if (!ValidationUtils.CheckNumber(req.ChargeAmount))
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.TotalAmountInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (!ValidationUtils.IsCCCD(req.CccdPassport))
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.CCCDInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            //get user id from token
            Guid userId = GetUserIdFromJwt(); 
            var etagExist = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagCode == req.EtagCode && !x.Deflag
            && x.EtagDetail.CccdPassport == req.CccdPassport // FROM ETAG DETAIL HERE TOO
                 , include: etag => etag.Include(y => y.Wallet).Include(y => y.EtagDetail));
            if (etagExist == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = HttpStatusCodes.NotFound,
                };
            }
            if(etagExist.Wallet.ExpireDate < TimeUtils.GetCurrentSEATime() || etagExist.EndDate < TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.EtagExpired,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if(PaymentTypeHelper.allowedPaymentTypes.Contains(req.PaymentType) == false)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.PaymentTypeInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = req.PaymentType,
                Name = "Charge Money for Etag: " + etagExist.EtagDetail.FullName, //FULL NAME FROM ETAG DETAIL
                TotalAmount = req.ChargeAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                EtagId = etagExist.Id,
                SaleType = SaleType.EtagCharge,
                UserId = userId,
            };

            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            string key = req.PaymentType + "_" + newOrder.InvoiceId;
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagMessage.CreateOrderForCharge,
                StatusCode = HttpStatusCodes.OK,
                 Data = new
                 {
                     invoiceId = newOrder.InvoiceId,
                     balance = req.ChargeAmount,
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

        public async Task CheckEtagExpire()
        {
            var currentDate = TimeUtils.GetCurrentSEATime();
            var etags = (List<Etag>) await _unitOfWork.GetRepository<Etag>().GetListAsync
                (predicate: x => x.EndDate < currentDate && x.Status == (int)EtagStatusEnum.Active && !x.Deflag);
            foreach (var etag in etags)
            {
                etag.Status = (int)EtagStatusEnum.Expired;
                etag.UpsDate = currentDate;
            }
            _unitOfWork.GetRepository<Etag>().UpdateRange(etags);
            await _unitOfWork.CommitAsync();
        }

        public async Task<ResponseAPI> EtagPayment(string etagCode, int amount, Guid storeId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => !x.Deflag && x.Id == storeId && x.Status == (int)StoreStatusEnum.Opened);
            if (store == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagCode == etagCode && x.Status == (int)EtagStatusEnum.Active , include: etag => etag.Include(y => y.Wallet));
            if(etag.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = EtagMessage.EtagExpired,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if(etag.Wallet.ExpireDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = WalletTypeMessage.WalletExpired,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if(etag.Wallet.Balance < amount)
            {
                return new ResponseAPI
                {
                    MessageResponse = WalletTypeMessage.NotEnoughBalance,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if(etag.Deflag)
            {
                return new ResponseAPI
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            etag.Wallet.Balance -= amount;
            etag.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);

            var newDeposit = new Deposit
            {
                Id = Guid.NewGuid(),
                Name = "Payment From Store",
                Amount = amount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                EtagId = etag.Id,
                IsIncrease = false,
                PaymentType = PaymentTypeHelper.allowedPaymentTypes[5],
                WalletId = etag.WalletId,
                StoreId = store.Id
            };
            etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
             _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
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
            
        

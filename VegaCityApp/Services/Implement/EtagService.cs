using AutoMapper;
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
        public EtagService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<EtagService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateEtagType(EtagTypeRequest req)
        {
            var newEtagType = new EtagType
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                ImageUrl = req.ImageUrl,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.MarketZoneId),
                BonusRate = req.BonusRate,
                Deflag = false,
                Amount = req.Amount
            };
            await _unitOfWork.GetRepository<EtagType>().InsertAsync(newEtagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.Created,
                Data = new
                {
                    EtagTypeId = newEtagType.Id,
                }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> UpdateEtagType(Guid etagTypeId, UpdateEtagTypeRequest req)
        {
            var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == etagTypeId && !x.Deflag);
            if (etagType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            etagType.Name = req.Name;
            etagType.ImageUrl = req.ImageUrl ?? etagType.ImageUrl;
            etagType.BonusRate = req.BonusRate ?? etagType.BonusRate;
            etagType.Amount = req.Amount ?? etagType.Amount;
            _unitOfWork.GetRepository<EtagType>().UpdateAsync(etagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.UpdateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagTypeId = etagType.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.UpdateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> DeleteEtagType(Guid etagTypeId)
        {
            var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == etagTypeId && !x.Deflag);
            if (etagType == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            etagType.Deflag = true;
            _unitOfWork.GetRepository<EtagType>().UpdateAsync(etagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.DeleteEtagTypeSuccessfully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagTypeId = etagType.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.DeleteEtagTypeFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> SearchEtagType(Guid etagTypeId)
        {
            var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == etagTypeId && !x.Deflag,
                include: etag => etag.Include(y => y.Etags), selector: z => new { z.Id, z.BonusRate, z.Name, z.Etags, z.Amount, z.ImageUrl });
            if (etagType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagType }
            };
        }
        public async Task<IPaginate<EtagTypeResponse>> SearchAllEtagType(int size, int page)
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
                predicate: x => !x.Deflag
                );
            return data;

        }
        public async Task<ResponseAPI> CreateEtag(EtagRequest req)
        {
            if(!ValidationUtils.IsCCCD(req.Cccd))
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.CCCDInvalid,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.PhoneNumberInvalid,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagTypeId);
            if (etagType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.EtagTypeNotFound,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                WalletType = (int)WalletTypeEnum.EtagWallet,
                Balance = req.MoneyStart??0,
                BalanceHistory = req.MoneyStart??0,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false
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
                Cccd = req.Cccd,
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                Gender = req.Gender,
                EtagCode = "VGC" + TimeUtils.GetCurrentSEATime().ToString("yyyyMMddHHmmss"),
                StartDate = req.StartDate,
                EndDate = req.Day == null ? req.EndDate : req.StartDate.AddDays((double)req.Day),
                Status = (int)EtagStatusEnum.Active
            };
            newEtag.Qrcode = EnCodeBase64.EncodeBase64Etag(newEtag.EtagCode);
            await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.Created,
                Data = new { etagId = newEtag.Id, walletId = newWallet.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> AddEtagTypeToPackage(Guid etagTypeId, Guid packageId)
        {
            var etag = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == etagTypeId && !x.Deflag);
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == packageId && !x.Deflag);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.NotFoundPackage,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            var etagPackage = new PackageETagTypeMapping()
            {
                Id = Guid.NewGuid(),
                EtagTypeId = etag.Id,
                PackageId = package.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<PackageETagTypeMapping>().InsertAsync(etagPackage);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.Created,
                Data = new { etagPackageId = etagPackage.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> RemoveEtagTypeFromPackage(Guid etagId, Guid packageId)
        {
            var packageEtagType = await _unitOfWork.GetRepository<PackageETagTypeMapping>().SingleOrDefaultAsync
                (predicate: x => x.EtagTypeId == etagId && x.PackageId == packageId);
            if (packageEtagType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            _unitOfWork.GetRepository<PackageETagTypeMapping>().DeleteAsync(packageEtagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.DeleteEtagTypeSuccessfully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagPackageId = packageEtagType.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.DeleteEtagTypeFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> GenerateEtag(int quantity, Guid etagTypeId, GenerateEtagRequest req)
        {
            List<Guid> listEtagCreated = new List<Guid>();
            var checkEtagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == etagTypeId && !x.Deflag);
            if (checkEtagType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            //generate etag
            for (int i = 0; i < quantity; i++)
            {
                // create wallet for etag
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    WalletType = (int)WalletTypeEnum.EtagWallet,
                    Balance = req.MoneyStart?? 0,
                    BalanceHistory = req.MoneyStart??0,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Deflag = false
                };
                await _unitOfWork.GetRepository<Wallet>().InsertAsync(wallet);
                // create etag
                var newEtag = new Etag
                {
                    Id = Guid.NewGuid(),
                    FullName = "User VegaCity",
                    PhoneNumber = "",
                    Cccd = "",
                    ImageUrl = "",
                    Gender = (int)GenderEnum.Other,
                    EtagCode = "VGC" + TimeUtils.GetCurrentSEATime().ToString("yyyyMMddHHmmss"),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Deflag = false,
                    EtagTypeId = checkEtagType.Id,
                    MarketZoneId = checkEtagType.MarketZoneId,
                    WalletId = wallet.Id,
                    StartDate = req.StartDate,
                    EndDate = req.Day == null? req.EndDate: req.StartDate.AddDays((double)req.Day),
                    Status = (int)EtagStatusEnum.Inactive
                };
                newEtag.Qrcode = EnCodeBase64.EncodeBase64Etag(newEtag.EtagCode);
                await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
                await _unitOfWork.CommitAsync();
                listEtagCreated.Add(newEtag.Id);
            }
            return new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.Created,
                Data = new { 
                    Quantity = quantity,
                    ListIdEtag = listEtagCreated 
                }
            };
        }
        public async Task<ResponseAPI> UpdateEtag(Guid etagId, UpdateEtagRequest req)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == etagId && !x.Deflag);
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.NotFoundEtag,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            if(!ValidationUtils.IsCCCD(req.CCCD))
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.CCCDInvalid,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            if(!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.PhoneNumberInvalid,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            etag.FullName = req.Fullname ?? etag.FullName;
            etag.PhoneNumber = req.PhoneNumber ?? etag.PhoneNumber;
            etag.Cccd = req.CCCD ?? etag.Cccd;
            etag.ImageUrl = req.ImageUrl ?? etag.ImageUrl;
            etag.Birthday = req.DateOfBirth ?? etag.Birthday;
            etag.Gender = req.Gender ?? etag.Gender;
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.UpdateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagId = etag.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.UpdateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> DeleteEtag(Guid etagId)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == etagId && !x.Deflag);
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.NotFoundEtag,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            etag.Deflag = true;
            etag.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.DeleteEtagSuccessfully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagId = etag.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.DeleteEtagFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> SearchEtag(Guid etagId)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == etagId && !x.Deflag,
                include: etag => etag.Include(y => y.EtagType)
                        .Include(y => y.Wallet)
                        .Include(y => y.MarketZone));
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.NotFoundEtag,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.SearchEtagSuccess,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etag }
            };
        }
        public async Task<IPaginate<EtagResponse>> SearchAllEtag(int size, int page)
        {
            var data = await _unitOfWork.GetRepository<Etag>().GetPagingListAsync(
                               selector: x => new EtagResponse()
                               {
                                   Id = x.Id,
                                   Fullname = x.FullName,
                                   PhoneNumber = x.PhoneNumber,
                                   CCCD = x.Cccd,
                                   ImageUrl = x.ImageUrl,
                                   EtagCode = x.EtagCode,
                                   QRCode = x.Qrcode,
                                   Birthday = x.Birthday,
                                   Gender = x.Gender,
                                   Deflag = x.Deflag,
                                   EndDate = x.EndDate,
                                   StartDate = x.StartDate,
                                   Status = x.Status
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.FullName),
                                predicate: x => !x.Deflag);
            return data;
        }
        public async Task<ResponseAPI> ActivateEtag(Guid etagId, ActivateEtagRequest req)
        {
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(
                predicate: x => x.Id == etagId && !x.Deflag && x.Status ==(int) EtagStatusEnum.Inactive);
            if (etag == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagMessage.NotFoundEtag,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            etag.Status = (int)EtagStatusEnum.Active;
            etag.FullName = req.Name;
            etag.PhoneNumber = req.Phone;
            etag.Cccd = req.CCCD;
            etag.UpsDate = TimeUtils.GetCurrentSEATime();
            etag.Gender = req.Gender;
            etag.StartDate = req.StartDate ?? etag.StartDate;
            etag.EndDate = req.EndDate ?? etag.EndDate;
            _unitOfWork.GetRepository<Etag>().UpdateAsync(etag);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.ActivateEtagSuccess,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new { etagId = etag.Id }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.ActivateEtagFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
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
            var etagExist = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.EtagCode == req.EtagCode && !x.Deflag && x.Cccd == req.CCCD,
                 include: etag => etag.Include(y => y.Wallet) );
            if (etagExist == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = EtagMessage.NotFoundEtag,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound,
                };
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == req.UserId && x.Status == (int)UserStatusEnum.Active);
               
            if (user == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = UserMessage.NotFoundUser,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound,
                };
            }
           

            etagExist.Wallet.Balance += req.ChargeAmount;
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = req.PaymentType,
                Name = "Charge Money for Etag: " + etagExist.FullName,
                TotalAmount = req.ChargeAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = TimeUtils.GetCurrentSEATime().ToString("yyyymmdd"),
                EtagId = etagExist.Id,
                SaleType = "Etag Charge",
                UserId = req.UserId,
            };

            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            string key = req.PaymentType + "_" + newOrder.InvoiceId;
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = EtagMessage.CreateOrderForCharge,
                StatusCode = HttpStatusCodes.OK,
                 Data = new
                 {
                     InvoiceId = newOrder.InvoiceId,
                     Balance = etagExist.Wallet.Balance,
                     Key = key,
                     UrlDirect = "https://localhost:44395/api/v1/payment/momo/order/charge-money", //https://localhost:44395/api/v1/payment/momo/order, https://vega.vinhuser.one/api/v1/payment/momo/order
                     UrlIpn = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b"
                 }
            } : new ResponseAPI()
            {
                MessageResponse = EtagMessage.CreateOrderForChargeFail,
                StatusCode = HttpStatusCodes.BadRequest
            };



        }
    }
}
            
        

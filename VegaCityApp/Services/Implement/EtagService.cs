using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;

namespace VegaCityApp.API.Services.Implement
{
    public class EtagService: BaseService<EtagService>, IEtagService
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
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
                BonusRate = req.BonusRate,
                Deflag = false,
                Amount = req.Amount
            };
            await _unitOfWork.GetRepository<EtagType>().InsertAsync(newEtagType);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.Created,
                Data = new {
                    EtagTypeId = newEtagType.Id,
                }
            } : new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagTypeMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> UpdateEtagType(UpdateEtagTypeRequest req)
        {
            var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagTypeId && !x.Deflag);
            if(etagType == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.NotFoundEtagType,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }
            etagType.Name = req.Name;
            etagType.ImageUrl = req.ImageUrl?? etagType.ImageUrl;
            etagType.BonusRate = req.BonusRate?? etagType.BonusRate;
            etagType.Amount = req.Amount?? etagType.Amount;
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
            if(etagType == null)
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
                include: etag => etag.Include(y => y.Etags), selector: z => new { z.Id, z.BonusRate, z.Name, z.Etags, z.Amount });
            if(etagType == null)
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
            if(req.UserId != null)
            {
                var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == req.UserId);
                if(user == null)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = MessageConstant.EtagMessage.UserNotFound,
                        StatusCode = MessageConstant.HttpStatusCodes.NotFound
                    };
                }
                var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagTypeId);
                if(etagType == null)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = MessageConstant.EtagMessage.EtagTypeNotFound,
                        StatusCode = MessageConstant.HttpStatusCodes.NotFound
                    };
                }
                var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == etagType.MarketZoneId);
                if(marketZone == null)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = MessageConstant.EtagMessage.MarketZoneNotFound,
                        StatusCode = MessageConstant.HttpStatusCodes.NotFound
                    };
                }

                var newEtag = new Etag
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EtagTypeId = etagType.Id,
                    Balance = req.Balance ?? 0,
                    Birthday = req.Birthday,
                    Cccd = req.Cccd,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    Deflag = false,
                    FullName = req.FullName,
                    Gender = req.Gender,
                    ImageUrl = req.ImageUrl,
                    MarketZoneId = marketZone.Id,
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    EtagCode = PasswordUtil.GenerateCharacter(6),
                    Qrcode = PasswordUtil.GenerateCharacter(8),
                    PhoneNumber = req.PhoneNumber
                };

                await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                    StatusCode = MessageConstant.HttpStatusCodes.Created,
                    Data = new { etagId = newEtag.Id }
                } : new ResponseAPI()
                {
                    MessageResponse = MessageConstant.EtagTypeMessage.CreateFail,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = MessageConstant.EtagMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
    }
}

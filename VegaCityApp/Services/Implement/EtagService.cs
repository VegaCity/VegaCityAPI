using Pos_System.API.Constants;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;

namespace VegaCityApp.API.Services.Implement
{
    public class EtagService: BaseService<EtagService>, IEtagService
    {
        public EtagService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<EtagService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        public async Task<CreateEtagTypeResponse> CreateEtagType(EtagTypeRequest req)
        {
            var newEtagType = new EtagType
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                ImageUrl = req.ImageUrl,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId)
            };
            await _unitOfWork.GetRepository<EtagType>().InsertAsync(newEtagType);
            return await _unitOfWork.CommitAsync() > 0 ? new CreateEtagTypeResponse()
            {
                EtagTypeId = newEtagType.Id,
                Message = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                StatusCode = MessageConstant.HttpStatusCodes.Created
            } : new CreateEtagTypeResponse()
            {
                Message = MessageConstant.EtagTypeMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        public async Task<CreateEtagResponse> CreateEtag(EtagRequest req)
        {
            if(req.UserId != null)
            {
                var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == req.UserId);
                if(user == null)
                {
                    return new CreateEtagResponse()
                    {
                        Message = MessageConstant.EtagMessage.UserNotFound,
                        StatusCode = MessageConstant.HttpStatusCodes.NotFound
                    };
                }
                var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagTypeId);
                if(etagType == null)
                {
                    return new CreateEtagResponse()
                    {
                        Message = MessageConstant.EtagMessage.EtagTypeNotFound,
                        StatusCode = MessageConstant.HttpStatusCodes.NotFound
                    };
                }
                var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == etagType.MarketZoneId);
                if(marketZone == null)
                {
                    return new CreateEtagResponse()
                    {
                        Message = MessageConstant.EtagMessage.MarketZoneNotFound,
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
                    EtagCode = GenerateETag(6),
                    Qrcode = GenerateETag(8),
                    PhoneNumber = req.PhoneNumber
                };

                await _unitOfWork.GetRepository<Etag>().InsertAsync(newEtag);
                return await _unitOfWork.CommitAsync() > 0 ? new CreateEtagResponse()
                {
                    Message = MessageConstant.EtagTypeMessage.CreateSuccessFully,
                    StatusCode = MessageConstant.HttpStatusCodes.Created,
                    etagId = newEtag.Id
                } : new CreateEtagResponse()
                {
                    Message = MessageConstant.EtagTypeMessage.CreateFail,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            return new CreateEtagResponse()
            {
                Message = MessageConstant.EtagMessage.CreateFail,
                StatusCode = MessageConstant.HttpStatusCodes.BadRequest
            };
        }
        private static string GenerateETag(int length)
        {
            // Chuỗi chứa các ký tự có thể có trong ETag
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            // Tạo chuỗi ngẫu nhiên từ các ký tự đã định nghĩa
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

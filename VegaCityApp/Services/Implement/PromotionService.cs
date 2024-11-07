using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Promotion;
using VegaCityApp.API.Payload.Request.Zone;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.GetZoneResponse;
using VegaCityApp.API.Payload.Response.PromotionResponse;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class PromotionService : BaseService<PromotionService>, IPromotionService
    {
        public PromotionService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PromotionService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreatePromotion(PromotionRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            var promotion = await _unitOfWork.GetRepository<Promotion>().SingleOrDefaultAsync
                (predicate: x => x.PromotionCode == req.PromotionCode && x.Status == (int) PromotionStatusEnum.Active);
            if(promotion != null)
            {
                return new ResponseAPI
                {
                    MessageResponse = PromotionMessage.PromotionExists,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if(req.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = PromotionMessage.InvalidEndDate,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate <= req.StartDate)
            {
                return new ResponseAPI
                {
                    MessageResponse = PromotionMessage.InvalidDuration,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.Quantity <= 0)
            {
                return new ResponseAPI
                {
                    MessageResponse = "Invalid Quantity, Must greater than 0",
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var newPromotion = new Promotion()
            {
                Id = Guid.NewGuid(),
                MarketZoneId = apiKey,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                PromotionCode = req.PromotionCode,
                Description = req.Description,
                DiscountPercent = req.DiscountPercent,
                MaxDiscount = req.MaxDiscount,
                RequireAmount = req.RequireAmount,
                Quantity = req.Quantity,
                Status = (int)PromotionStatusEnum.Active,
                Name = req.Name,
            };
            await _unitOfWork.GetRepository<Promotion>().InsertAsync(newPromotion);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PromotionMessage.CreatePromotionSuccessfully,
                StatusCode = HttpStatusCodes.OK
            }
            : new ResponseAPI()
            {
                MessageResponse = PromotionMessage.CreatePromotionFail,
                StatusCode = HttpStatusCodes.BadRequest
            };

        }
        public async Task <ResponseAPI> UpdatePromotion(Guid PromotionId ,UpdatePromotionRequest req)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().SingleOrDefaultAsync(predicate: x => x.Id == PromotionId && x.Status == (int)PromotionStatusEnum.Inactive); // Only Inactive Promotion can be updated
            if(promotion == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = PromotionMessage.NotFoundPromotion,
                    StatusCode= HttpStatusCodes.NotFound
                };
            }
            if (req.EndDate <= TimeUtils.GetCurrentSEATime())
            {
                return new ResponseAPI
                {
                    MessageResponse = PromotionMessage.InvalidEndDate,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate <= req.StartDate)
            {
                return new ResponseAPI
                {
                    MessageResponse = PromotionMessage.InvalidDuration,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.Quantity <= 0)
            {
                return new ResponseAPI
                {
                    MessageResponse = "Invalid Quantity, Must greater than 0",
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            promotion.Name = req.Name;
            promotion.Description = req.Description;
            promotion.MaxDiscount = req.MaxDiscount;
            promotion.RequireAmount = req.RequireAmount;
            promotion.DiscountPercent = req.DiscountPercent;
            promotion.StartDate = req.StartDate;
            promotion.EndDate = req.EndDate;    
            promotion.Quantity = req.Quantity;
            _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotion);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = PromotionMessage.UpdatePromotionSuccessfully,
                StatusCode = HttpStatusCodes.OK
            }
            : new ResponseAPI()
            {
                MessageResponse = PromotionMessage.UpdatePromotionFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI<IEnumerable<GetListPromotionResponse>>> SearchPromotions(int size, int page)
        {
            try
            {
                IPaginate<GetListPromotionResponse> data = await _unitOfWork.GetRepository<Promotion>().GetPagingListAsync(
                selector: x => new GetListPromotionResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    MarketZoneId = x.MarketZoneId,
                    Description = x.Description,
                    DiscountPercent = x.DiscountPercent,
                    MaxDiscount = x.MaxDiscount,
                    RequireAmount = x.RequireAmount,
                    PromotionCode = x.PromotionCode,
                    Quantity = x.Quantity,
                    Status = x.Status,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                    predicate: x => 
                     // x.Status == (int)PromotionStatusEnum.Active &&
                     x.EndDate >= TimeUtils.GetCurrentSEATime()
                );
                return new ResponseAPI<IEnumerable<GetListPromotionResponse>>
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = PromotionMessage.GetPromotionsSuccessfully,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items,
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<GetListPromotionResponse>>
                {
                    MessageResponse = PromotionMessage.GetPromotionsFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }

        }
        public async Task<ResponseAPI> SearchPromotion(Guid promotionId)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().SingleOrDefaultAsync(
                predicate: x => x.Id == promotionId && x.Status == (int)PromotionStatusEnum.Active && x.EndDate >= TimeUtils.GetCurrentSEATime(),
                include: zone => zone.Include(y => y.PromotionOrders));
            if (promotion == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PromotionMessage.GetPromotionFail,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = PromotionMessage.GetPromotionSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    promotion
                }
            };
        }

        public async Task<ResponseAPI> DeletePromotion(Guid promotionId)
        {
            var promotion = await _unitOfWork.GetRepository<Promotion>().SingleOrDefaultAsync(predicate: x => x.Id == promotionId && x.Status == (int)PromotionStatusEnum.Active);
            if (promotion == null)
               {
                return new ResponseAPI()
                {
                    MessageResponse = PromotionMessage.GetPromotionFail,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
                _unitOfWork.GetRepository<Promotion>().UpdateAsync(promotion);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = PromotionMessage.DeletePromotionSuccessfully,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = PromotionMessage.DeletePromotionFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
    }
}

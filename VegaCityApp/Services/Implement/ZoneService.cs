using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request.Zone;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.GetZoneResponse;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class ZoneService: BaseService<ZoneService>, IZoneService
    {
        public ZoneService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<ZoneService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateZone(CreateZoneRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            var zoneExisted = await _unitOfWork.GetRepository<Zone>()
                .SingleOrDefaultAsync(predicate: x => x.Name == req.Name && x.Location == req.Location && !x.Deflag);
            if (zoneExisted != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.Conflict,
                    MessageResponse = ZoneMessage.ZoneExisted
                };
            }
            var newZone = new Zone()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Location = req.Location,
                MarketZoneId = apiKey,
                Deflag = false,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
            };
            await _unitOfWork.GetRepository<Zone>().InsertAsync(newZone);
            var response = new ResponseAPI()
            {
                MessageResponse = ZoneMessage.CreateZoneSuccess,
                StatusCode = HttpStatusCodes.Created,
                Data = newZone.Id

            };
            int check = await _unitOfWork.CommitAsync();

            return check > 0 ? response : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = ZoneMessage.CreateZoneFail
            };
        }

        public async Task<ResponseAPI> UpdateZone(Guid Id, UpdateZoneRequest req)
        {
          
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Id == Id && !x.Deflag,
                include: z => z.Include(zone => zone.Store));
            if (zone == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = ZoneMessage.SearchZoneFail
                };
            }
            zone.Name = req.ZoneName != null ? req.ZoneName.Trim() : zone.Name;
            zone.Location = req.ZoneLocation != null ? req.ZoneLocation.Trim() : zone.Location;
            zone.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Zone>().UpdateAsync(zone);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = ZoneMessage.UpdateZoneSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        zone
                    }
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = ZoneMessage.UpdateZoneFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<GetZoneResponse>>> SearchZones(int size, int page)
        {
            try
            {
                IPaginate<GetZoneResponse> data = await _unitOfWork.GetRepository<Zone>().GetPagingListAsync(
                selector: x => new GetZoneResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Location = x.Location,
                    MarketZoneId = x.MarketZoneId,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag && x.MarketZoneId == GetMarketZoneIdFromJwt()
                );
                return new ResponseAPI<IEnumerable<GetZoneResponse>>
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = ZoneMessage.SearchZonesSuccess,
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
                return new ResponseAPI<IEnumerable<GetZoneResponse>>
                {
                    MessageResponse = ZoneMessage.SearchZonesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }

        }
        public async Task<ResponseAPI<Zone>> SearchZone(Guid ZoneId)
        {
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(
                predicate: x => x.Id == ZoneId && !x.Deflag,
                include: zone => zone.Include(y => y.Store))
                ?? throw new BadHttpRequestException(ZoneMessage.SearchZoneFail, HttpStatusCodes.NotFound);

            return new ResponseAPI<Zone>
            {
                MessageResponse = ZoneMessage.SearchZoneSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = zone
            };
        }

        public async Task<ResponseAPI> DeleteZone(Guid ZoneId)
        {
            var zone = await SearchZone(ZoneId);
            zone.Data.Deflag = true;
            _unitOfWork.GetRepository<Zone>().UpdateAsync(zone.Data);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = ZoneMessage.DeleteZoneSuccess,
                StatusCode = HttpStatusCodes.OK
            }
            : new ResponseAPI()
            {
                MessageResponse = ZoneMessage.DeleteZoneFailed,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
    }
}

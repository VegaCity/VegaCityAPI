using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pos_System.API.Constants;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
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
    public class ZoneService: BaseService<ZoneService>, IZoneService
    {
        public ZoneService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<ZoneService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateZone(CreateZoneRequest req)
        {
           
           
            var newZone = new Zone()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Location = req.Location,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
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
          
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Id == Id, include: z=>z.Include(zone=>zone.Houses) );
            if (zone == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = ZoneMessage.SearchZoneFail
                };
            }
            zone.Name = req.ZoneName;
            if (zone.Houses.Any())
            {
                return new ResponseAPI()
                {
                    MessageResponse = ZoneMessage.HouseStillExist,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            zone.Location = req.ZoneLocation;
            zone.CrDate = zone.CrDate;
            zone.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Zone>().UpdateAsync(zone);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = ZoneMessage.UpdateZoneSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    
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

        public async Task<IPaginate<Zone>> SearchZones(int size, int page)
        {
            IPaginate<Zone> data = await _unitOfWork.GetRepository<Zone>().GetPagingListAsync(

                selector: x => new Zone()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Location = x.Location,
                    MarketZoneId = x.MarketZoneId,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name)
            );
            return data;
        }
        public async Task<ResponseAPI> SearchZone(Guid ZoneId)
        {
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(
                predicate: x => x.Id == ZoneId,
                include: zone => zone
                    .Include(y => y.Houses)
            );

            if (zone == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = ZoneMessage.SearchZoneFail,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = ZoneMessage.SearchZoneSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                   zone
                   
                }
            };
        }

        public async Task<ResponseAPI> DeletePackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == PackageId);
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
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = PackageMessage.DeleteFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
    }
}

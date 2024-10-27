using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Payload.Request.House;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class HouseService : BaseService<HouseService>, IHouseService
    {
        public HouseService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<HouseService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateHouse(CreateHouseRequest req)
        {
            var house = await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync
                (predicate: x => x.HouseName == req.HouseName 
                              && !x.Deflag
                              && x.Location == req.Location
                              && x.Address == req.Address
                              && x.ZoneId == req.ZoneId);
            if (house != null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = HouseMessage.ExistedHouseName,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var newHouse = new House
            {
                Id = Guid.NewGuid(),
                HouseName = req.HouseName,
                Location = req.Location,
                Address = req.Address,
                ZoneId = req.ZoneId,
                Deflag = false,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                IsRent = false,
            };
            await _unitOfWork.GetRepository<House>().InsertAsync(newHouse);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = HouseMessage.CreateHouseSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    HouseId = newHouse.Id
                }
            } : new ResponseAPI()
            {
                MessageResponse = HouseMessage.CreateHouseFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> DeleteHouse(Guid HouseId)
        {
            var house = await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync(predicate: x => x.Id == HouseId && !x.Deflag,
                include: store => store.Include(z => z.Stores));
            if (house == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = HouseMessage.NotFoundHouse,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            // delete all store in house
            if(house.Stores.Count > 0)
            {
                foreach (var store in house.Stores)
                {
                    store.Deflag = true;
                    store.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Store>().UpdateAsync(store);
                }
            }
            house.Deflag = true;
            _unitOfWork.GetRepository<House>().UpdateAsync(house);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = HouseMessage.DeleteSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = HouseMessage.DeleteFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        public async Task<ResponseAPI<IEnumerable<GetHouseResponse>>> SearchAllHouse(int size, int page)
        {
            try
            {
                IPaginate<GetHouseResponse> data = await _unitOfWork.GetRepository<House>().GetPagingListAsync(
                               selector: x => new GetHouseResponse()
                               {
                                   Id = x.Id,
                                   HouseName = x.HouseName,
                                   Location = x.Location,
                                   Address = x.Address,
                                   ZoneId = x.ZoneId,
                                   CrDate = x.CrDate,
                                   UpsDate = x.UpsDate,
                                   Deflag = x.Deflag,
                                   IsRent = x.IsRent,
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.HouseName),
                                predicate: x => !x.Deflag);
                return new ResponseAPI<IEnumerable<GetHouseResponse>>
                {
                    MessageResponse = HouseMessage.GetHousesSuccessfully,
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
                return new ResponseAPI<IEnumerable<GetHouseResponse>>
                {
                    MessageResponse = HouseMessage.GetHousesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData=null
                };
            }
        }
        public async Task<ResponseAPI> SearchHouse(Guid HouseId)
        {
            var house = await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync(
                predicate: x => x.Id == HouseId && !x.Deflag,
                include: house => house.Include(x => x.Stores)
            );
            if (house == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = HouseMessage.NotFoundHouse,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = HouseMessage.FoundHouse,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    house
                }
            };
        }
        public async Task<ResponseAPI> UpdateHouse(Guid houseId, UpdateHouseRequest req)
        {
            var house = await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync(predicate: x => x.Id == houseId && !x.Deflag);
            if (house == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = HouseMessage.NotFoundHouse,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            house.HouseName = req.HouseName != null? req.HouseName.Trim() : house.HouseName;
            house.Location = req.Location != null ? req.Location.Trim() : house.Location;
            house.Address = req.Address != null ? req.Address.Trim() : house.Address;
            house.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<House>().UpdateAsync(house);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = HouseMessage.UpdateHouseSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    HouseId = house.Id
                }
            } : new ResponseAPI()
            {
                MessageResponse = HouseMessage.UpdateHouseFailed,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
    }
}

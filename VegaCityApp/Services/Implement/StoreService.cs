using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class StoreService: BaseService<StoreService>, IStoreService
    {
        public StoreService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<StoreService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> UpdateStore(Guid storeId,UpdateStoreRequest req)
        {
          
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == storeId && !x.Deflag);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            //check enum
            if (!Enum.IsDefined(typeof(StoreTypeEnum), req.StoreType))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = StoreMessage.InvalidStoreType
                };
            }
            store.Id = store.Id;
            store.Name = req.Name;
            store.Status = req.StoreStatus;
            store.StoreType = req.StoreType;
            store.Address = req.Address;
            store.CrDate = TimeUtils.GetCurrentSEATime();
            store.PhoneNumber = req.PhoneNumber;
            store.ShortName = req.ShortName;
            store.Email = req.Email;
            store.MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId);
            store.Description = req.Description;
            _unitOfWork.GetRepository<Store>().UpdateAsync(store);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateStoreSuccesss,
                    StatusCode = HttpStatusCodes.OK,
                    
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateStoreSuccesss,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }

        public async Task<IPaginate<GetStoreResponse>> SearchAllStore(int size, int page)
        {
            IPaginate<GetStoreResponse> data = await _unitOfWork.GetRepository<Store>().GetPagingListAsync(

                selector: x => new GetStoreResponse()
                {
                    Id = x.Id,
                    StoreType = x.StoreType,
                    Name = x.Name,
                    Address = x.Address,
                    Description = x.Description,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    PhoneNumber = x.PhoneNumber,
                    MarketZoneId = x.MarketZoneId,
                   ShortName = x.ShortName,
                   Email = x.Email,
                   HouseId = x.HouseId,
                   Status = x.Status
                   
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag 
            );
            return data;
        }

        public async Task<ResponseAPI> SearchStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId && !x.Deflag,
                include: store => store
                    .Include(y => y.DisputeReports)
                    .Include(y => y.Menus)
                    .Include(y => y.Orders)
                    .Include(y => y.ProductCategories)
                    .Include(y => y.Users)
            );
            store.Users = store.Users.Select(x => new User{ 
                Id = x.Id,
                FullName = x.FullName,
                IsChange = x.IsChange,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                Email = x.Email,
                Gender = x.Gender,
                Birthday = x.Birthday,
                Cccd = x.Cccd,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Status = x.Status
            }).ToList();
            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.GetStoreSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    store,
                }
            };
        }

        public async Task<ResponseAPI> DeleteStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == StoreId && !x.Deflag);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }

            store.Deflag = true;
            _unitOfWork.GetRepository<Store>().UpdateAsync(store);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeletedSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeleteFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
    }
}

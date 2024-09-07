using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pos_System.API.Constants;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
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

        public async Task<ResponseAPI> UpdateStore(UpdateStoreRequest req)
        {
          
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == req.StoreId);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.StoreMessage.NotFoundStore
                };
            }

            store.Id = store.Id;
            store.Name = req.Name;
            store.Status = req.StoreStatus;
            store.StoreType = int.Parse(EnvironmentVariableConstant.StoreSellerType);
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
                    MessageResponse = MessageConstant.StoreMessage.UpdateStoreSuccesss,
                    StatusCode = HttpStatusCodes.OK,
                    
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.StoreMessage.UpdateStoreSuccesss,
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
                predicate: x => x.Deflag == false
            );
            return data;
        }

        public async Task<ResponseAPI> SearchStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId,
                include: store => store
                    .Include(y => y.DisputeReports)
                    .Include(y => y.Menus)
                    .Include(y => y.Orders)
                    .Include(y => y.ProductCategories)
                    .Include(y => y.Users)
            );

            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.StoreMessage.NotFoundStore,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.GetStoreSuccess,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new
                {
                    Store = new
                    {
                      store.Id,
                      store.StoreType,
                      store.Name,
                      store.Address,
                      store.CrDate,
                      store.UpsDate,
                      store.Deflag,
                      store.PhoneNumber,
                      store.ShortName,
                      store.Email,
                      store.HouseId,
                      store.MarketZoneId,
                      store.Description,
                      store.Status,
                      store.House,
                 },
                   
                    DisputeReport = store.DisputeReports.Select(w => new
                    {
                        w.Id,
                        w.Status,
                        w.Description,
                        w.CrDate,
                        w.IssueType,
                        w.Store,
                        w.StoreId,
                        w.User,
                        w.UserId,
                        w.Transaction,
                        w.TransactionId,
                        w.ResolvedBy,
                        w.Resolution,
                        w.ResolvedDate,
                       
                    }),
                    Menus = store.Menus.Select(w=>new
                    {
                        w.Id,
                        w.Store,
                        w.StoreId,
                        w.Address,
                        w.ImageUrl,
                        w.MenuJson,
                        w.Name,
                        w.PhoneNumber,
                        w.Deflag,
                        w.CrDate
                    }),
                    Orders = store.Orders.Select(w=> new
                    {
                        w.Id,
                        w.Name,
                        w.Status,
                        w.Etag,
                        w.EtagId,
                        w.CrDate,
                        w.Store,
                        w.StoreId,
                        w.InvoiceId,
                        w.Transactions
                    }),
                    ProductCategories = store.ProductCategories.Select(w=>
                        new
                        {
                            w.Id,
                            w.Name,
                            w.StoreId,
                            w.Store,
                            w.CrDate,
                            w.UpsDate
                        }),
                    Users = store.Users.Select(w=>
                        new
                        {
                            w.Email,
                            w.Address,
                            w.PhoneNumber,
                            w.Status,
                            w.RoleId,
                            w.Role,
                            w.Etags

                        }
                    )

                }
            };
        }

        public async Task<ResponseAPI> DeleteStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == StoreId);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.StoreMessage.NotFoundStore
                };
            }

            store.Deflag = true;
            _unitOfWork.GetRepository<Store>().UpdateAsync(store);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = MessageConstant.StoreMessage.DeletedSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = MessageConstant.StoreMessage.DeleteFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
    }
}

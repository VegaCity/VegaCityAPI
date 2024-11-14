using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Request.WalletType;
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
    public class ServiceStore : BaseService<StoreService>, IServiceStore
    {
        public ServiceStore(
            IUnitOfWork<VegaCityAppContext> unitOfWork,
            ILogger<StoreService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateServiceStore(ServiceStoreRequest serviceStoreRequest)
        {
            serviceStoreRequest.Name = serviceStoreRequest.Name.Trim();
            if(serviceStoreRequest.Price <= 0)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = StoreMessage.PriceMustBeGreaterThanZero
                };
            }
            //check store exist
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == serviceStoreRequest.StoreId && !x.Deflag);
            var newServiceStore = _mapper.Map<Domain.Models.StoreService>(serviceStoreRequest);
            newServiceStore.Id = Guid.NewGuid();
            newServiceStore.CrDate = TimeUtils.GetCurrentSEATime();
            newServiceStore.UpsDate = TimeUtils.GetCurrentSEATime();
            newServiceStore.Deflag = false;
            newServiceStore.StoreId = store.Id;
            await _unitOfWork.GetRepository<Domain.Models.StoreService>().InsertAsync(newServiceStore);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = StoreMessage.CreateStoreServiceSuccessfully,
                Data = newServiceStore
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = StoreMessage.CreateStoreServiceFail
            };
        }

        public async Task<ResponseAPI> DeleteServiceStore(Guid id)
        {
            var serviceStore = await _unitOfWork.GetRepository<Domain.Models.StoreService>().SingleOrDefaultAsync
                (predicate: x => x.Id == id && !x.Deflag);
            if (serviceStore == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            //if(serviceStore.WalletTypeStoreServiceMappings.Count > 0)
            //{
            //    foreach(var item in serviceStore.WalletTypeStoreServiceMappings)
            //    {
            //        _unitOfWork.GetRepository<WalletTypeStoreServiceMapping>().DeleteAsync(item);
            //    }
            //}
            serviceStore.Deflag = true;
            serviceStore.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Domain.Models.StoreService>().UpdateAsync(serviceStore);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = StoreMessage.DeleteStoreServiceSuccessfully,
                Data = new
                {
                    id = serviceStore.Id
                }
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = StoreMessage.DeleteStoreServiceFail
            };
        }
        public async Task<ResponseAPI<IEnumerable<ServiceStoreResponse>>> GetAllServiceStore(int size, int page)
        {
            try
            {
                IPaginate<ServiceStoreResponse> data = await _unitOfWork.GetRepository<Domain.Models.StoreService>().GetPagingListAsync(
                predicate: x => !x.Deflag,
                selector: z => new ServiceStoreResponse
                {
                    Id = z.Id,
                    Name = z.Name,
                    StoreId = z.StoreId,
                    CrDate = z.CrDate,
                    UpsDate = z.UpsDate,
                    Deflag = z.Deflag
                },
                size: size,
                page: page,
                orderBy: c => c.OrderByDescending(z => z.Name)
            );
                return new ResponseAPI<IEnumerable<ServiceStoreResponse>>
                {
                    MessageResponse = StoreMessage.GetListStoreServicesSuccess,
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
                return new ResponseAPI<IEnumerable<ServiceStoreResponse>>
                {
                    MessageResponse = StoreMessage.GetListStoreServicesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

        public async Task<ResponseAPI> GetServiceStoreById(Guid id)
        {
            var serviceStore = await _unitOfWork.GetRepository<Domain.Models.StoreService>().SingleOrDefaultAsync(
                predicate: x => x.Id == id && !x.Deflag,
                include: x => x.Include(z => z.Store));
            if (serviceStore == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                Data = serviceStore
            };
        }

        public async Task<ResponseAPI> UpdateServiceStore(Guid Id, UpDateServiceStoreRequest serviceStoreRequest)
        {
            var ServiceStore = await _unitOfWork.GetRepository<Domain.Models.StoreService>().SingleOrDefaultAsync(
                predicate: x => x.Id == Id && !x.Deflag);
            if (ServiceStore == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStoreService
                };
            }
            ServiceStore.Name = serviceStoreRequest.Name.Trim();
            ServiceStore.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Domain.Models.StoreService>().UpdateAsync(ServiceStore);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = StoreMessage.UpdateStoreServiceSuccessfully,
                Data = ServiceStore
            } : new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = StoreMessage.UpdateStoreServiceFail
            };
        }
    }
}

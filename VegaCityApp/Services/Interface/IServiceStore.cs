using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IServiceStore
    {
        Task<ResponseAPI> CreateServiceStore(ServiceStoreRequest serviceStoreRequest);
        Task<ResponseAPI> UpdateServiceStore(Guid Id, UpDateServiceStoreRequest serviceStoreRequest);
        Task<ResponseAPI> DeleteServiceStore(Guid id);
        Task<ResponseAPI> GetServiceStoreById(Guid id);
        Task<IPaginate<ServiceStoreResponse>> GetAllServiceStore(int size, int page);
    }
}

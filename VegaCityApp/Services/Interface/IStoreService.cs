using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IStoreService
    {
        Task<ResponseAPI> UpdateStore(Guid storeId,UpdateStoreRequest req);
        Task<IPaginate<GetStoreResponse>> SearchAllStore(int size, int page);
        Task<ResponseAPI> DeleteStore(Guid StoreId);
        Task<ResponseAPI> SearchStore(Guid StoreId);
    }
}

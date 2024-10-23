using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.Domain.Paginate;

// test

namespace VegaCityApp.API.Services.Interface
{
    public interface IStoreService
    {
        Task<ResponseAPI> UpdateStore(Guid storeId,UpdateStoreRequest req);
        Task<ResponseAPI<IEnumerable<GetStoreResponse>>> SearchAllStore(Guid apiKey, int size, int page);
        Task<ResponseAPI> DeleteStore(Guid StoreId);
        Task<ResponseAPI> SearchStore(Guid StoreId);
        Task<ResponseAPI> GetMenuFromPos(Guid id);
    }
}

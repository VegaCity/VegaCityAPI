using VegaCityApp.API.Payload.Request.House;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IHouseService
    {
        Task<ResponseAPI> CreateHouse(CreateHouseRequest req);
        Task<ResponseAPI> UpdateHouse(Guid houseId, UpdateHouseRequest req);
        Task<ResponseAPI<IEnumerable<GetHouseResponse>>> SearchAllHouse(int size, int page);
        Task<ResponseAPI> SearchHouse(Guid HouseId);
        Task<ResponseAPI> DeleteHouse(Guid HouseId);
    }
}

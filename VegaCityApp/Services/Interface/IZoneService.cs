using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IZoneService
    {
       // Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreateZone(CreateZoneRequest req);
        Task<ResponseAPI> UpdateZone(Guid ZoneId, UpdateZoneRequest req);

        Task<IPaginate<Zone>> SearchZones(int size, int page);
        Task<ResponseAPI> SearchZone(Guid ZoneId);
        //Task<ResponseAPI> DeletePackage(Guid PackageId);
    }
}

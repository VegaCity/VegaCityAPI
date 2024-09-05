using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPackageService
    {
       // Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreatePackage(CreatePackageRequest req, Guid UserId);
        Task<ResponseAPI> UpdatePackage(UpddatePackageRequest req, Guid UserId);
        Task<GetListPackageResponse> GetListPackage(GetListParameterRequest req);
        Task<GetPackageResponse> GetPackageDetail (Guid PackageId);
    }
}

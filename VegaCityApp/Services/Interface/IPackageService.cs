using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPackageService
    {
       // Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreatePackage(CreatePackageRequest req);
        Task<ResponseAPI> UpdatePackage(UpdatePackageRequest req);

        Task<IPaginate<GetPackageResponse>> SearchAllPackage(int size, int page);

        Task<ResponseAPI> SearchPackage(Guid PackageId);
        Task<ResponseAPI> DeletePackage(Guid PackageId);
    }
}

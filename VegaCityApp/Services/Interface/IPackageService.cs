using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPackageService
    {
       // Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreatePackage(CreatePackageRequest req);
        Task<ResponseAPI> UpdatePackage(Guid packageId, UpdatePackageRequest req);
        Task<ResponseAPI<IEnumerable<GetPackageResponse>>> SearchAllPackage(int size, int page);
        Task<ResponseAPI> SearchPackage(Guid PackageId);
        Task<ResponseAPI> DeletePackage(Guid PackageId);

        Task<ResponseAPI> CreatePackageType(CreatePackageTypeRequest req);
        Task<ResponseAPI> UpdatePackageType(Guid packageId, UpdatePackageTypeRequest req);
        Task<ResponseAPI<IEnumerable<GetPackageTypeResponse>>> SearchAllPackageType(int size, int page);
        Task<ResponseAPI> SearchPackageType(Guid PackageTypeId);
        Task<ResponseAPI> DeletePackageType(Guid PackageTypeId);

        //Task<ResponseAPI> CreatePackageType(CreatePackageTypeRequest req);
        Task<ResponseAPI> UpdatePackageItem(Guid packageItemId, UpdatePackageItemRequest req);
        Task<ResponseAPI<IEnumerable<GetListPackageItemResponse>>> SearchAllPackageItem(int size, int page);
        Task<ResponseAPI> SearchPackageItem(Guid PackageItemId);
        //Task<ResponseAPI> DeletePackageType(Guid PackageTypeId);

    }
}

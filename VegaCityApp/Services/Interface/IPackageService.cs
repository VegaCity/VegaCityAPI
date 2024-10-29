﻿using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPackageService
    {
       // Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreatePackage(CreatePackageRequest req);
        Task<ResponseAPI> UpdatePackage(Guid packageId, UpdatePackageRequest req);

        Task<ResponseAPI<IEnumerable<GetPackageResponse>>> SearchAllPackage(int size, int page);

        Task<Package> SearchPackage(Guid PackageId);
        Task<ResponseAPI> DeletePackage(Guid PackageId);
    }
}

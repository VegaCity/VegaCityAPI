﻿using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPackageService
    {
        Task<ResponseAPI> CreatePackage(CreatePackageRequest req);
        Task<ResponseAPI> UpdatePackage(Guid packageId, UpdatePackageRequest req);
        Task<ResponseAPI<IEnumerable<GetPackageResponse>>> SearchAllPackage(int size, int page);
        Task<ResponseAPI<Package>> SearchPackage(Guid PackageId);
        Task<ResponseAPI> DeletePackage(Guid PackageId);
        Task<ResponseAPI> CreatePackageType(CreatePackageTypeRequest req);
        Task<ResponseAPI> UpdatePackageType(Guid packageId, UpdatePackageTypeRequest req);
        Task<ResponseAPI<IEnumerable<GetPackageTypeResponse>>> SearchAllPackageType(int size, int page);
        Task<ResponseAPI<PackageType>> SearchPackageType(Guid PackageTypeId);
        Task<ResponseAPI> DeletePackageType(Guid PackageTypeId);
        Task<ResponseAPI> CreatePackageItem(int quantity, CreatePackageItemRequest req);
        Task<ResponseAPI> ActivePackageItem(Guid packageItem, ActivatePackageItemRequest req);
        Task<ResponseAPI> UpdatePackageItem(Guid packageItemId, UpdatePackageItemRequest req);
        Task<ResponseAPI<IEnumerable<GetListPackageItemResponse>>> SearchAllPackageItem(int size, int page);
        Task<ResponseAPI> UpdateRfIdPackageItem(Guid Id, string rfId);
        Task<ResponseAPI<PackageItem>> SearchPackageItem(Guid? PackageItemId, string? rfId);
        Task<ResponseAPI> PrepareChargeMoneyEtag(ChargeMoneyRequest req);
        Task<ResponseAPI> PackageItemPayment(Guid packageItemId, int price, Guid storeId, List<OrderProduct> products);
        Task CheckPackageItemExpire();
        Task SolveWalletPackageItem(Guid apiKey);
    }
}

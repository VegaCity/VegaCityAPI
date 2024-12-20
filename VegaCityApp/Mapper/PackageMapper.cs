﻿using AutoMapper;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Mapper
{
    public class PackageMapper : Profile
    {
        public PackageMapper()
        {
            CreateMap<PackageOrder, GetListPackageItemResponse>();
            CreateMap<GetListPackageItemResponse, PackageOrder>();
        }
    }
}

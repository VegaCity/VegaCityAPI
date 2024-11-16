using AutoMapper;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Mapper
{
    public class ServiceStoreMapper : Profile
    {
        public ServiceStoreMapper()
        {
            CreateMap<Store, CreateStoreRequest>();
            CreateMap<CreateStoreRequest, Store>();
            CreateMap<CreateMenuRequest, Menu>();
            CreateMap<Menu, CreateMenuRequest>();
            CreateMap<CreateProductRequest, Product>();
            CreateMap<Product, CreateProductRequest>();
            CreateMap<CreateProductCategoryRequest, ProductCategory>();
            CreateMap<ProductCategory, CreateProductCategoryRequest>();
        }
    }
}

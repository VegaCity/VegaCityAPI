using AutoMapper;
using VegaCityApp.API.Payload.Request.Store;

namespace VegaCityApp.API.Mapper
{
    public class ServiceStoreMapper : Profile
    {
        public ServiceStoreMapper()
        {
            CreateMap<ServiceStoreRequest, Domain.Models.StoreService>();
            CreateMap<UpDateServiceStoreRequest, Domain.Models.StoreService>();
            CreateMap<Domain.Models.StoreService, UpDateServiceStoreRequest>();
            CreateMap<Domain.Models.StoreService, ServiceStoreRequest>();
        }
    }
}

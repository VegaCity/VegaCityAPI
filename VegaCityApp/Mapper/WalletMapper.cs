using AutoMapper;
using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Mapper
{
    public class WalletMapper : Profile
    {
        public WalletMapper()
        {
            CreateMap<WalletTypeRequest, WalletType>();
            CreateMap<WalletType, WalletTypeRequest>();
            CreateMap<UpDateWalletTypeRequest, WalletType>();
            CreateMap<WalletType, UpDateWalletTypeRequest>();
        }
    }
}

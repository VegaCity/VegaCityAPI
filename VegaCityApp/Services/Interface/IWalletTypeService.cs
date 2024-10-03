using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IWalletTypeService
    {
        Task<ResponseAPI> CreateWalletType(WalletTypeRequest walletTypeRequest);
        Task<ResponseAPI> UpdateWalletType(Guid Id, UpDateWalletTypeRequest walletTypeRequest);
        Task<ResponseAPI> DeleteWalletType(Guid id);
        Task<ResponseAPI> GetWalletTypeById(Guid id);
        Task<IPaginate<WalletTypeResponse>> GetAllWalletType(int size, int page);

    }
}

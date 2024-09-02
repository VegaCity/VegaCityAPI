
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Request;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task<CreateAccountResponse> CreateAccount(CreateAccountRequest req);
    }
}

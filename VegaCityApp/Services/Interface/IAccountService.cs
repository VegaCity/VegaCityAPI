
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.Payload.Request;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task<ResponseAPI> Login(LoginRequest req);

        Task<ResponseAPI> Register (RegisterRequest req);

        Task<ResponseAPI> ApproveUser (ApproveRequest req);

        Task<ResponseAPI> ChangePassword (ChangePasswordRequest req);

    }
}

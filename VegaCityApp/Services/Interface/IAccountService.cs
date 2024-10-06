
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task<LoginResponse> Login(LoginRequest req);

        Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req);

        Task<ResponseAPI> Register (RegisterRequest req);

        Task<ResponseAPI> AdminCreateUser(RegisterRequest req);

        Task<ResponseAPI> ApproveUser (Guid userId, ApproveRequest req);

        Task<ResponseAPI> ChangePassword (ChangePasswordRequest req);

        Task<ResponseAPI> SearchAllUser(int size, int page);

        Task<ResponseAPI> SearchUser(Guid UserId);

        Task<ResponseAPI> UpdateUser(Guid userId, UpdateUserAccountRequest req);

        Task<ResponseAPI> DeleteUser(Guid UserId);
    }
}

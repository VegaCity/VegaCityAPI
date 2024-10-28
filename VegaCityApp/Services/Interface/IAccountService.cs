
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task<LoginResponse> Login(LoginRequest req);
        Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req);
        Task<ResponseAPI> GetRefreshTokenByEmail(string email, GetApiKey req);
        Task<ResponseAPI> Register (RegisterRequest req);
        Task<ResponseAPI> AdminCreateUser(RegisterRequest req);
        Task<ResponseAPI> ApproveUser (Guid userId, ApproveRequest req);
        Task<ResponseAPI> ChangePassword (ChangePasswordRequest req);
        Task<ResponseAPI<IEnumerable<GetUserResponse>>> SearchAllUser(int size, int page);
        Task<User> SearchUser(Guid UserId);
        Task<ResponseAPI<User>> UpdateUser(Guid userId, UpdateUserAccountRequest req);
        Task<ResponseAPI> DeleteUser(Guid UserId);
        Task<ResponseAPI<Wallet>> GetAdminWallet();
        Task<ResponseAPI> GetChartByDuration(AdminChartDurationRequest req);

    }
}

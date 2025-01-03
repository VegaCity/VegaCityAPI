
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;
using VegaCityApp.Domain.Models;
using VegaCityApp.API.Payload.Response.UserResponse;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Payload.Request.Store;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task AddRole();
        Task<LoginResponse> Login(LoginRequest req);
        Task<ResponseAPI<UserSession>> CreateUserSession(Guid userId, SessionRequest req);
        Task<ResponseAPI<UserSession>> GetUserSessionById(Guid sessionId);
        Task<ResponseAPI<IEnumerable<GetUserSessions>>> GetAllUserSessions(int page, int size);
        Task<ResponseAPI> DeleteSession(Guid sessionId);
        Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req);
        Task<ResponseAPI> GetRefreshTokenByEmail(string email, GetApiKey req);
        Task<ResponseAPI> Register(RegisterRequest req);
        Task<ResponseAPI> AdminCreateUser(RegisterRequest req);
        Task<ResponseAPI> ApproveUser(Guid userId, ApproveRequest req);
        Task<ResponseAPI> ChangePassword(ChangePasswordRequest req);
        Task<ResponseAPI<IEnumerable<GetUserResponse>>> SearchAllUser(int size, int page);
        Task<ResponseAPI<IEnumerable<GetUserResponse>>> SearchAllUserNoSession(int size, int page);
        Task<ResponseAPI<User>> SearchUser(Guid UserId);
        Task<ResponseAPI> UpdateUser(Guid userId, UpdateUserAccountRequest req);
        Task<ResponseAPI> DeleteUser(Guid UserId);
        Task<ResponseAPI> GetAdminWallet();
        Task<ResponseAPI> GetChartByDuration(AdminChartDurationRequest req);
        Task<ResponseAPI> GetTopSaleStore(TopSaleStore req);

        Task<string> ReAssignEmail(Guid userId, ReAssignEmail email);
        Task<ResponseAPI<IEnumerable<GetStoreResponse>>> GetAllClosingRequest([FromQuery] Guid apiKey, [FromQuery] int size = 10, [FromQuery] int page = 1);
        Task<ResponseAPI> SearchStoreClosing(Guid StoreId);
        Task<ResponseAPI> ResolveClosingStore(GetWalletStoreRequest req);
        Task CheckSession();
        Task<ResponseAPI<IEnumerable<GetDepositApprovalResponse>>> GetDepositApproval(int size, int page);
        Task<ResponseAPI> DepositApproval(Guid transactionId, DepositApproval status);
    }
}

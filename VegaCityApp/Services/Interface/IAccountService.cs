
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Payload.Request;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task<ResponseAPI> Login(LoginRequest req);

        Task<ResponseAPI> Register (RegisterRequest req);

        Task<ResponseAPI> ApproveUser (Guid userId, ApproveRequest req);

        Task<ResponseAPI> ChangePassword (ChangePasswordRequest req);

        Task<IPaginate<GetUserResponse>> SearchAllUser(int size, int page);

        //Task<GetListUserResponse> GetListUserByUserRoleId(Guid RoleId);

        Task<ResponseAPI> SearchUser(Guid UserId);

        Task<ResponseAPI> UpdateUser(Guid userId, UpdateUserAccountRequest req);

        Task<ResponseAPI> DeleteUser(Guid UserId);
    }
}

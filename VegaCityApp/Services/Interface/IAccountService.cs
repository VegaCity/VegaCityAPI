
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response.UserResponse;
using VegaCityApp.Payload.Request;

namespace VegaCityApp.Service.Interface
{
    public interface IAccountService
    {
        Task<ResponseAPI> Login(LoginRequest req);

        Task<ResponseAPI> Register (RegisterRequest req);

        Task<ResponseAPI> ApproveUser (ApproveRequest req);

        Task<ResponseAPI> ChangePassword (ChangePasswordRequest req);

        Task<GetListUserResponse> GetUserList(GetListParameterRequest req);

        Task<GetListUserResponse> GetListUserByUserRoleId(Guid RoleId);

        Task<GetUserResponse> GetUserDetail(Guid UserId);

        Task<GetUserResponse> UpdateUserById(UpdateUserAccountRequest req, Guid UserId);

       // Task<ResponseAPI> DeleteUserById(Guid UserId);

    }
}

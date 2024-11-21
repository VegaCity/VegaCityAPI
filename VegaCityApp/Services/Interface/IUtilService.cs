using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Services.Interface
{
    public interface IUtilService
    {
        Task<UserSession> CheckUserSession(Guid userId);
    }
}

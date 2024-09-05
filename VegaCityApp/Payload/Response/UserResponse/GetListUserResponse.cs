using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response.UserResponse
{
    public class GetListUserResponse : ResponseAPI
    {
        public List<User> Users { get; set; }
    }
}

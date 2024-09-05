using VegaCityApp.API.Payload.Response.ETagResponse;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.API.Payload.Response.RoleResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Payload.Response.WalletResponse;

namespace VegaCityApp.API.Payload.Response.UserResponse
{
    public class GetUserResponse : ResponseAPI
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime?  Birthday { get; set; }
        public string Description { get; set; }
        public string Cccd { get; set; }
        public int? Gender { get; set; }
        public string ImageUrl { get; set; }

        public string PinCode { get; set; }

        public Guid? MarketZoneId { get; set; }

        public Guid? StoreId { get; set; }

        public Guid? RoleId { get; set; }

        public GetRoleResponse Role { get; set; }
        public GetStoreResponse Store { get; set; }

        public List<GetETagResponse> Etags { get; set; } = new List<GetETagResponse>();

        //order
        public List<GetOrderResponse> orders { get; set; } = new List<GetOrderResponse>();
        public List<GetWalletResponse> userWallets { get; set; } = new List<GetWalletResponse>();

    }

}
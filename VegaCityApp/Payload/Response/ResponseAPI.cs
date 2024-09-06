using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response
{
    public class ResponseAPI
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public Object Data { get; set; }
    }
    public class EtagTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid MarketZoneId { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? BonusRate { get; set; }
        public bool Deflag { get; set; }
        public int Amount { get; set; }
    }
    public class GetUserResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public int? Gender { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
        public string? PinCode { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public Guid? RoleId { get; set; }
        public string? Description { get; set; }
        public bool? IsChange { get; set; }
        public string? Address { get; set; }
        public int? Status { get; set; }
    }
}


using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response
{
    public class ResponseAPI
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public Object Data { get; set; }
        public string? ParentName { get; set; }
    }
    //responseAPI for get list paginate 
    public class ResponseAPI<T>
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public string? ParentName { get; set; }
        public MetaData MetaData { get; set; }
        public T Data { get; set; }
        public string? QRCode { get; set; }

    }
    public class MetaData
    {
        public int Size { get; set; }
        public int Page { get; set; }
        public int Total { get; set; }
        public int TotalPage { get; set; }
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
        public string? CccdPassport { get; set; }
        public string? ImageUrl { get; set; }
        public string? Email { get; set; }
        public Guid? RoleId { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? Status { get; set; }
        public string? RegisterStoreType { get; set; }
        public string RoleName { get; set; }
    }
}


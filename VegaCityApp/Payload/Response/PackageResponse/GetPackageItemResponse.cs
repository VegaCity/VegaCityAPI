using VegaCityApp.Domain.Models;
using Newtonsoft.Json;


namespace VegaCityApp.API.Payload.Response.PackageResponse
{
    public class GetPackageItemResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public string? Name { get; set; }
        public string? CCCDPassport { get; set; }  
        public string? Email { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool? IsAdult { get; set; }
        public Guid? WalletId { get; set; }   
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
    }

    public class GetListPackageItemResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public string? Name { get; set; }
        public string? CCCDPassport { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool? IsAdult { get; set; }
        public Guid? WalletId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        [JsonIgnore]
        public virtual MarketZone? MarketZone { get; set; }
    }
}

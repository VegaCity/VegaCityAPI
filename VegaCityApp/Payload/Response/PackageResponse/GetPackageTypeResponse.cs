using VegaCityApp.Domain.Models;
using Newtonsoft.Json;
namespace VegaCityApp.API.Payload.Response.PackageResponse
{
    public class GetPackageTypeResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid? ZoneId { get; set; }
        public string? Name { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }
        //public string? ImageUrl { get; set; }
        //public virtual MarketZone? MarketZone { get; set; }

    }

    public class GetListPackageTypeResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid? ZoneId { get; set; }
        public string? Name { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }

        [JsonIgnore]
        public virtual MarketZone? MarketZone { get; set; }

    }
}

using VegaCityApp.Domain.Models;
using Newtonsoft.Json;
namespace VegaCityApp.API.Payload.Response.PackageResponse
{
    public class GetPackageResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? Duration { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag {get; set; }
        public string? ImageUrl { get; set; }
        public virtual MarketZone? MarketZone { get; set; }

    }

    public class GetListPackageResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }

        [JsonIgnore]
        public virtual MarketZone? MarketZone { get; set; }  

    }
}

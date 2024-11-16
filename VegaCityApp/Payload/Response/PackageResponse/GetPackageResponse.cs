using VegaCityApp.Domain.Models;
using Newtonsoft.Json;
namespace VegaCityApp.API.Payload.Response.PackageResponse
{
    public class GetPackageResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public Guid ZoneId { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public int Duration { get; set; }

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

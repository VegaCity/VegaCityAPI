using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response.StoreResponse
{
    public class GetStoreResponse
    {
        public Guid? Id { get; set; }
        public int? StoreType { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShortName { get; set; }
        public string? Email { get; set; }
        public Guid? HouseId { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }

        public virtual House? House { get; set; }
        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<DisputeReport> DisputeReports { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}

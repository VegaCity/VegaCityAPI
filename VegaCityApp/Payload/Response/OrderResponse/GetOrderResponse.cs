using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response.OrderResponse
{
    public class GetOrderResponse
    {
        public Guid Id { get; set; }
        public string? PaymentType { get; set; }
        public string? Name { get; set; }
        public int? TotalAmount { get; set; }
        public DateTime? CrDate { get; set; }
        public string? Status { get; set; }
        public string? InvoiceId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? EtagId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? EtagTypeId { get; set; }
        public Guid? PackageId { get; set; }
        public ICollection<OrderDetail> details { get; set; } 
    }
}

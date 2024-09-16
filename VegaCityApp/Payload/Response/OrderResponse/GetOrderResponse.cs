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
        public double? Vatrate { get; set; }
        public string? ProductJson { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? EtagId { get; set; }
    }
}

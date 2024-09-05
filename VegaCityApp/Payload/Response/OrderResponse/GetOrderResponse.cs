namespace VegaCityApp.API.Payload.Response.OrderResponse
{
    public class GetOrderResponse : ResponseAPI
    {
        public Guid? OrderId { get; set; }
        public string? Name { get; set; }
        public int? TotalAmount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? EtagId { get; set; }
        public string? PaymentType { get; set; }
        public string? InvoiceId { get; set; }
    }
}

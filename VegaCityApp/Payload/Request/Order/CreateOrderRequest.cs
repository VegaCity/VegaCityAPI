namespace VegaCityApp.API.Payload.Request.Order
{
    public class CreateOrderRequest
    {
        //  public string? PaymentType { get; set; }
        public string? Name { get; set; }
        public string? InvoiceId { get; set; }
        public double? VATRate { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? EtagId { get; set; }
    }
}

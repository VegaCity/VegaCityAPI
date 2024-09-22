namespace VegaCityApp.API.Payload.Request.Order
{
    public class CreateOrderRequest
    {
        public string OrderName { get; set; }
        public string PaymentType { get; set; }
        public Guid? StoreId { get; set; }
        public int TotalAmount { get; set; }
        public Guid? MenuId { get; set; }
        public List<OrderProductFromPosRequest> ProductData { get; set; }
        public string InvoiceId { get; set; }
        public Guid? EtagId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? PackageId { get; set; }
        public Guid? EtagTypeId { get; set; }
    }
}

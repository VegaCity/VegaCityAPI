namespace VegaCityApp.API.Payload.Request.Order
{
    public class CreateOrderRequest
    {
        public string SaleType { get; set; }
        public string paymentType { get; set; }
        public Guid StoreId { get; set; }
        public int TotalAmount { get; set; }
        public Guid? PackageOrderId { get; set; }
        //public Guid PackageId { get; set; }
        public List<OrderProductFromPosRequest> ProductData { get; set; }
    }
    public class CreateOrderForCashierRequest
    {
        public string SaleType { get; set; }
        public string PaymentType { get; set; }
        public int TotalAmount { get; set; }
        public List<OrderProductFromCashierRequest> ProductData { get; set; }
        public CustomerInfo CustomerInfo { get; set; }

    }
    public class ConfirmOrderForCashierRequest
    {
        public string InvoiceId { get; set; }
        public string? TransactionChargeId { get; set; }
        public string? TransactionId { get; set; }
    }
    public class CustomerInfo
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CccdPassport { get; set; }
    }
}

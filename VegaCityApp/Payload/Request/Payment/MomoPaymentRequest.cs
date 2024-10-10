namespace VegaCityApp.API.Payload.Request.Payment
{
    public class MomoPaymentRequest
    {
        public string orderInfo { get; set; }
        public string partnerCode { get; set; }
        public string redirectUrl { get; set; }
        public string ipnUrl { get; set; }
        public long amount { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public string extraData { get; set; }
        public string partnerName { get; set; }
        public string storeId { get; set; }
        public string requestType { get; set; }
        public string orderGroupId { get; set; }
        public bool autoCapture { get; set; }
        public string lang { get; set; }
        public string signature { get; set; }
        public int orderExpireTime { get; set; }
    }
    public class PaymentRequest
    {
        public string InvoiceId { get; set; }
        public string? Key { get; set; }
        public string? UrlDirect { get; set; }
        public string? UrlIpn { get; set; }
    }
    //payOS
    public class PayOSItems
    {
        public string name { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }

    public class CustomerInfo
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; } // payOs must contain email
    }
}

namespace VegaCityApp.API.Payload.Response.PaymentResponse
{
    public class MomoPaymentResponse
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public int amount { get; set; }
        public string responseTime { get; set; }
        public string message { get; set; }
        public int resultCode { get; set; }
        public string payUrl { get; set; }
        public string shortLink { get; set; }
    }
}

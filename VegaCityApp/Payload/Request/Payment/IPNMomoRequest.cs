namespace VegaCityApp.API.Payload.Request.Payment
{
    public class IPNMomoRequest
    {
        public string orderType { get; set; }
        public long amount { get; set; }
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
        public string transId { get; set; }
        public string responseTime { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; }
        public string payType { get; set; }
        public string requestId { get; set; }
        public string orderInfo { get; set; }
    }
}

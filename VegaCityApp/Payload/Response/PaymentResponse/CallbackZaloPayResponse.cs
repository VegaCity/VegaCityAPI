namespace VegaCityApp.API.Payload.Response.PaymentResponse
{
    public class CallbackZaloPayResponse
    {
        public int app_id { get; set; }
        public string app_trans_id { get; set; }
        public long app_time { get; set; }
        public string app_user { get; set; }
        public long amount { get; set; }
        public string embed_data { get; set; }
        public string item { get; set; }
        public long zp_trans_id { get; set; }
        public long server_time { get; set; }
        public int channel { get; set; }
        public string merchant_user_id { get; set; }
        public long user_fee_amount { get; set; }
        public long discount_amount { get; set; }
    }
}

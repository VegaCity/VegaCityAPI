using VegaCityApp.API.Utils;

namespace VegaCityApp.API.Payload.Request.Payment
{
    public class ZaloPayRequest
    {
        public int app_id { get; set; }
        public string app_user { get; set; }
        public string app_trans_id { get; set; }
        public long app_time { get; set; }
        public long? expire_duration_seconds { get; set; }
        public long amount { get; set; }
        public string item { get; set; }
        public string description { get; set; }
        public string? embed_data { get; set; }
        public string bank_code { get; set; }
        public string mac { get; set; }
        public string? callback_url { get; set; }
        public string? sub_app_id { get; set; }
        public string? title { get; set; }
        public string? currency { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
    }
}

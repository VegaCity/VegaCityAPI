namespace VegaCityApp.API.Payload.Request.Payment
{
    public class IPNZaloPayRequest
    {
        public long amount { get; set; }
        public int appid { get; set; }
        public string apptransid { get; set; }
        public string? bankcode { get; set; } 
        public string checksum { get; set; }
        public long discountamount { get; set; }
        public int pmcid { get; set; }
        public int status { get; set; }
    }
}

using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Request.Admin
{
    public class ApproveRequest
    {
        public string LocationZone { get; set; }
        public int StoreType { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string StoreEmail { get; set; }
        public string ApprovalStatus { get; set; }
        public double StoreTransferRate { get; set; }
    }
}

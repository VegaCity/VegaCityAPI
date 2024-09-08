using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Request.Admin
{
    public class ApproveRequest
    {
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string StoreEmail { get; set; }
        public string ApprovalStatus { get; set; }
    }
}

using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Request
{
    public class ApproveRequest
    {
        public string UserId { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string StoreEmail { get; set; }
        public ApproveStatus ApprovalStatus { get; set; }
    }
}

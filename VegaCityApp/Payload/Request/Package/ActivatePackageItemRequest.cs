namespace VegaCityApp.API.Payload.Request.Package
{
    public class ActivatePackageItemRequest
    {
        public string Name { get; set; }
        public string Cccdpassport { get; set; }
        public string PhoneNumber { get; set; } 
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAdult { get; set; }
    }
}

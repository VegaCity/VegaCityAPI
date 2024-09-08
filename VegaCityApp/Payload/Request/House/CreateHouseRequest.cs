namespace VegaCityApp.API.Payload.Request.House
{
    public class CreateHouseRequest
    {
        public string HouseName { get; set; }
        public string? Location { get; set; }
        public string? Address { get; set; }
        public Guid ZoneId { get; set; }
    }
}

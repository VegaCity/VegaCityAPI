namespace VegaCityApp.API.Payload.Request.House
{
    public class UpdateHouseRequest
    {
        public string HouseName { get; set; } = null!;
        public string? Location { get; set; }
        public string? Address { get; set; }
    }
}

namespace VegaCityApp.API.Payload.Response.HouseResponse
{
    public class GetHouseResponse
    {
        public Guid Id { get; set; }
        public string HouseName { get; set; }
        public string? Location { get; set; }
        public string? Address { get; set; }
        public Guid ZoneId { get; set; }
        public bool Deflag { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
    }
}

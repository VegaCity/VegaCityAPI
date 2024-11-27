namespace VegaCityApp.API.Payload.Response.UserResponse
{
    public class GetUserSessions
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TotalCashReceive { get; set; }
        public int? TotalFinalAmountOrder { get; set; }
        public int? TotalQuantityOrder { get; set; }
        public int? TotalWithrawCash { get; set; }
        public Guid ZoneId { get; set; }
        public string? Status { get; set; }
    }
}

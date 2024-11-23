namespace VegaCityApp.API.Payload.Request.Report
{
    public class ReportRequest
    {
        public Guid? CreatorPackageOrderId { get; set; }
        public Guid? CreatorStoreId { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; } = null!;
    }
    public class ReportResponse
    {
        public Guid Id { get; set; }
        public Guid? CreatorPackageOrderId { get; set; }
        public Guid? CreatorStoreId { get; set; }
        public Guid? SolveUserId { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string? Solution { get; set; }
        public string? Creator { get; set; }
        public string? SolveBy { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Status { get; set; }
    }
}

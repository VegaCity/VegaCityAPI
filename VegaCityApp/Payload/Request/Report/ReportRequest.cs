namespace VegaCityApp.API.Payload.Request.Report
{
    public class ReportRequest
    {
        public Guid? PackageItemId { get; set; }
        public Guid? UserId { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string? Solution { get; set; }
        public string? SolveBy { get; set; }
        public Guid CrDate { get; set; }
        public Guid UpsDate { get; set; }
        public int Status { get; set; }
    }
    public class ReportResponse 
    {
        public Guid Id { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; }
        public Guid? StoreId { get; set; }
        public int Status { get; set; }
    }
}

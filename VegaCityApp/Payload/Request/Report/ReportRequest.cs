namespace VegaCityApp.API.Payload.Request.Report
{
    public class ReportRequest
    {
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; }
        public Guid? StoreId { get; set; }
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

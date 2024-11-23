using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Report;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.ReportResponse;

namespace VegaCityApp.API.Services.Interface
{
    public interface IReportService
    {
        Task<ResponseAPI> CreateIssueType(CreateIssueTypeRequest req);
        Task<ResponseAPI<IEnumerable<IssueTypeResponse>>> GetAllIssueType(int size, int page);
        Task<ResponseAPI<IEnumerable<ReportResponse>>> GetAllReports(int size, int page);
        Task<ResponseAPI> DeleteIssueType(Guid id);
        Task<ResponseAPI> CreateReport(ReportRequest req);
        Task<ResponseAPI> UpdateReport(Guid id, SolveRequest req);
    }
}

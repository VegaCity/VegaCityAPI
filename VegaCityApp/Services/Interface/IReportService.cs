using VegaCityApp.API.Payload.Request.Report;
using VegaCityApp.API.Payload.Response;

namespace VegaCityApp.API.Services.Interface
{
    public interface IReportService
    {
        Task<ResponseAPI> CreateIssueType(CreateIssueTypeRequest req);
        Task<ResponseAPI> DeleteIssueType(Guid id);
        Task<ResponseAPI> CreateReport(Guid creatorId, ReportRequest req);
        Task<ResponseAPI> UpdateReport(Guid id, SolveRequest req);
    }
}

using AutoMapper;
using VegaCityApp.API.Payload.Request.Report;

namespace VegaCityApp.API.Mapper
{
    public class ReportMapper : Profile
    {
        public ReportMapper()
        {
            CreateMap<CreateIssueTypeRequest, Domain.Models.IssueType>();
            CreateMap<Domain.Models.IssueType, CreateIssueTypeRequest>();

            CreateMap<ReportRequest, Domain.Models.DisputeReport>();
            CreateMap<Domain.Models.DisputeReport, ReportRequest>();
        }
    }
}

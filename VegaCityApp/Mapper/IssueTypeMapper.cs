using AutoMapper;
using VegaCityApp.API.Payload.Request.Report;

namespace VegaCityApp.API.Mapper
{
    public class IssueTypeMapper : Profile
    {
        public IssueTypeMapper()
        {
            CreateMap<CreateIssueTypeRequest, Domain.Models.IssueType>();
            CreateMap<Domain.Models.IssueType, CreateIssueTypeRequest>();
        }
    }
}

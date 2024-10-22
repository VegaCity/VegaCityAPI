using AutoMapper;
using VegaCityApp.API.Payload.Request.Report;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class ReportService : BaseService<ReportService>, IReportService
    {
        public ReportService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<ReportService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreateIssueType(CreateIssueTypeRequest req)
        {
            var issueType = _mapper.Map<IssueType>(req);
            issueType.CrDate = TimeUtils.GetCurrentSEATime();
            issueType.Id = Guid.NewGuid();
            await _unitOfWork.GetRepository<IssueType>().InsertAsync(issueType);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Create Issue Type Success",
                Data = issueType
            };
        }

        public async Task<ResponseAPI> DeleteIssueType(Guid id)
        {
            var issueType = await _unitOfWork.GetRepository<IssueType>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (issueType == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Issue Type Not Found"
                };
            }
            _unitOfWork.GetRepository<IssueType>().DeleteAsync(issueType);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Delete Issue Type Success",
                Data = new
                {
                    issueTypeId = issueType.Id
                }
            };
        }
    }
}

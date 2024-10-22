using AutoMapper;
using VegaCityApp.API.Enums;
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

        public async Task<ResponseAPI> CreateReport(Guid creatorId, ReportRequest req)
        {
            var report = _mapper.Map<DisputeReport>(req);
            report.Id = Guid.NewGuid();
            report.CreatorId = creatorId;
            report.CrDate = TimeUtils.GetCurrentSEATime();
            report.Status = (int) ReportStatus.Pending;
            report.StoreId = req.StoreId;
            await _unitOfWork.GetRepository<DisputeReport>().InsertAsync(report);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Create Report Success",
                Data = report
            };
        }
        public async Task<ResponseAPI> UpdateReport(Guid id, SolveRequest req)
        {
            string email = GetEmailFromJwt();
            var report = await _unitOfWork.GetRepository<DisputeReport>().SingleOrDefaultAsync
                (predicate: x => x.Id == id
                        && (x.Status != (int)ReportStatus.Done
                        || x.Status != (int)ReportStatus.Cancel));
            if (report == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Report Not Found"
                };
            }
            report.Status = req.Status;
            report.SolveDate = TimeUtils.GetCurrentSEATime();
            report.Solution = req.Solution != null? req.Solution.Trim() : report.Solution;
            report.SolveBy = email;
            _unitOfWork.GetRepository<DisputeReport>().UpdateAsync(report);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Update Report Success",
                Data = report
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

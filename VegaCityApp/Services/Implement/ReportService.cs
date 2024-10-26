using AutoMapper;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Report;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.API.Payload.Response.ReportResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
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

        public async Task<ResponseAPI> CreateReport(string creator, ReportRequest req)
        {
            var report = _mapper.Map<DisputeReport>(req);
            report.Id = Guid.NewGuid();
            report.Creator = creator;
            report.CrDate = TimeUtils.GetCurrentSEATime();
            report.Status = (int) ReportStatus.Pending;
            report.StoreId = req.StoreId;
            await _unitOfWork.GetRepository<DisputeReport>().InsertAsync(report);
           
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Create Report Success",
                Data = report
            }: new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = "Create Report Fail"
            };
        }
        public async Task<ResponseAPI> UpdateReport(Guid id, SolveRequest req)
        {
            Guid userSolve = GetUserIdFromJwt();
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == userSolve);
            var report = await _unitOfWork.GetRepository<DisputeReport>().SingleOrDefaultAsync
                (predicate: x => x.Id == id
                        && x.Status != (int)ReportStatus.Done
                       );
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
            report.SolveBy = user.Email;
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

        public async Task<ResponseAPI<IEnumerable<IssueTypeResponse>>> GetAllIssueType(int size, int page)
        {
            try
            {
                IPaginate<IssueTypeResponse> data = await _unitOfWork.GetRepository<IssueType>().GetPagingListAsync(
                               selector: x => new IssueTypeResponse()
                               {
                                   Id = x.Id,
                                   CrDate = x.CrDate,
                                   Name = x.Name
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.Name));
                return new ResponseAPI<IEnumerable<IssueTypeResponse>>
                {
                    MessageResponse = "Get All Issue Type Success",
                    StatusCode = HttpStatusCodes.OK,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<IssueTypeResponse>>
                {
                    MessageResponse = "Get Issue Type Fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

        public async Task<ResponseAPI<IEnumerable<ReportResponse>>> GetAllReports(int size, int page)
        {
            try
            {
                IPaginate<ReportResponse> data = await _unitOfWork.GetRepository<DisputeReport>().GetPagingListAsync(
                               selector: x => new ReportResponse()
                               {
                                 Id = x.Id,
                                 Description = x.Description,
                                 IssueTypeId = x.IssueTypeId,
                                 StoreId = x.StoreId,
                                 Status = x.Status
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate));
                return new ResponseAPI<IEnumerable<ReportResponse>>
                {
                    MessageResponse = "Get All Reports Successfully!",
                    StatusCode = HttpStatusCodes.OK,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<ReportResponse>>
                {
                    MessageResponse = "Failed To Get  All Reports" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

    }
}

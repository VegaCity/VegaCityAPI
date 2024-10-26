using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Report;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.ReportResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class ReportController : BaseController<ReportController>
    {
        private readonly IReportService _reportService;
        public ReportController(ILogger<ReportController> logger, IReportService service) : base(logger)
        {
            _reportService = service;
        }
        [HttpPost(ReportEndpoint.CreateIssueType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreateIssueType([FromBody] CreateIssueTypeRequest request)
        {
            var result = await _reportService.CreateIssueType(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(ReportEndpoint.DeleteIssueType)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteIssueType(Guid id)
        {
            var result = await _reportService.DeleteIssueType(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(ReportEndpoint.CreateReport)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreateReport([FromQuery] string PhoneNumberCreator, [FromBody] ReportRequest request)
        {
            var result = await _reportService.CreateReport(PhoneNumberCreator, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(ReportEndpoint.UpdateReport)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.Store)]
        public async Task<IActionResult> UpdateReport(Guid id, [FromBody] SolveRequest request)
        {
            var result = await _reportService.UpdateReport(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(ReportEndpoint.GetListIssueType)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<IssueTypeResponse>>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllIssueType([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _reportService.GetAllIssueType(size, page);
            return Ok(result);
        }
        [HttpGet(ReportEndpoint.GetListReports)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReportResponse>>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllReports([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _reportService.GetAllReports(size, page);
            return Ok(result);
        }
    }
}

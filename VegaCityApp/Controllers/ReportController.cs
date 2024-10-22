using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Report;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
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
    }
}

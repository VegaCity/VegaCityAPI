using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services.Interface;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using VegaCityApp.API.Payload.Response.TransactionResponse;
using static VegaCityApp.API.Constants.MessageConstant;
using VegaCityApp.API.Validators;
using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class TransactionController : BaseController<TransactionController>
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService) : base(logger)
        {
            _transactionService = transactionService;
        }
        [HttpGet(TransactionEndpoint.GetListTransaction)]
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<TransactionResponse>>), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp, RoleEnum.Store)]
        public async Task<IActionResult> GetAllTransaction(int size, int page)
        {
            var response = await _transactionService.GetAllTransaction(size, page);
            return Ok(response);
        }
        [HttpGet(TransactionEndpoint.GetTransaction)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.CashierWeb, RoleEnum.CashierApp, RoleEnum.CashierApp)]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            var response = await _transactionService.GetTransactionById(id);
            return Ok(response);
        }
        [HttpDelete(TransactionEndpoint.DeleteTransaction)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            var response = await _transactionService.DeleteTransaction(id);
            return Ok(response);
        }
    }
}

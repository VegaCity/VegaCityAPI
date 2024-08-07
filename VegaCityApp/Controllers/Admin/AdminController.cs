using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Service.Interface;
using static Pos_System.API.Constants.MessageConstant;
using static VegaCityApp.API.Constants.ApiEndPointConstant;

namespace VegaCityApp.API.Controllers.Admin
{
    [ApiController]
    public class AdminController : BaseController<AdminController>
    {
        private readonly IAccountService _service;

        public AdminController(ILogger<AdminController> logger, IAccountService accountService) : base(logger)
        {
            _service = accountService;
        }

        [HttpPost("account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var result = await _service.CreateAccount(request);
            return Ok(result);
        }
        [HttpPost(WalletTypeEndpoint.CreateWalletType)]
        [ProducesResponseType(typeof(CreateWalletTypeResponse), HttpStatusCodes.Created)]
        [ProducesResponseType(typeof(CreateWalletTypeResponse), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateWalletType([FromBody] WalletTypeRequest request)
        {
            var result = await _service.CreateWalletType(request);
            return Ok(result);
        }
        [HttpDelete(WalletTypeEndpoint.DeleteWalletType)]
        [ProducesResponseType(typeof(CreateWalletTypeResponse), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(CreateWalletTypeResponse), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> DeleteWalletType(Guid Id)
        {
            var result = await _service.DeleteWalletType(Id);
            return Ok(result);
        }

    }
}

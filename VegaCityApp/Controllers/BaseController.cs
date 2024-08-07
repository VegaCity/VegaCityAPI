using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Constants;

namespace VegaCityApp.API.Controllers
{
	[Route(ApiEndPointConstant.ApiEndpoint)]
	[ApiController]
	public class BaseController<T> : ControllerBase where T : BaseController<T>
	{
		protected ILogger<T> _logger;

		public BaseController(ILogger<T> logger)
		{
			_logger = logger;
		}
	}
}

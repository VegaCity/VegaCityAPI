using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;

namespace VegaCityApp.API.Services
{
	public abstract class BaseService<T> where T : class
	{
		protected IUnitOfWork<VegaCityAppContext> _unitOfWork;
		protected ILogger<T> _logger;
		protected IMapper _mapper;
		protected IHttpContextAccessor _httpContextAccessor;
		public BaseService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<T> logger, IHttpContextAccessor httpContextAccessor,IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
			_mapper = mapper;
		}

		protected string GetRoleFromJwt()
		{
			string role = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
			return role;
		}

		protected string GetMarketZoneIdFromJwt()
		{
			return _httpContextAccessor?.HttpContext?.User?.FindFirstValue("MarketZoneId");
		}

		protected string GetEmailFromJwt()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email);
        }
    }
}

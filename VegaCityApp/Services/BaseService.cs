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
		public BaseService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<T> logger, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
		}

		protected string GetUsernameFromJwt()
		{
			string username = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			return username;
		}

		protected string GetRoleFromJwt()
		{
			string role = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
			return role;
		}

		//Use for employee and store manager
		//protected async Task<bool> CheckIsUserInStore(Account account, Store store)
		//{
		//	ICollection<StoreAccount> storeAccount = await _unitOfWork.GetRepository<StoreAccount>()
		//		.GetListAsync(predicate: s => s.StoreId.Equals(store.Id));
		//	return storeAccount.Select(x => x.AccountId).Contains(account.Id);
		//}

		//protected string GetBrandIdFromJwt()
		//{
		//	return _httpContextAccessor?.HttpContext?.User?.FindFirstValue("brandId");
		//}
		//protected string GetStoreIdFromJwt()
		//{
		//	return _httpContextAccessor?.HttpContext?.User?.FindFirstValue("storeId");
		//}
	}
}

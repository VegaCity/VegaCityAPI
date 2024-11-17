using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Validators;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class StoreController : BaseController<StoreController>
    {
        private readonly IStoreService _storeService;

        public StoreController(ILogger<StoreController> logger, IStoreService storeService) : base(logger)
        {
            _storeService = storeService;
        }
        #region CRUD Store
        //[HttpPost(StoreEndpoint.CreateStore)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //[CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        //public async Task<IActionResult> CreateStore([FromBody] CreateStoreRequest req)
        //{
        //    var result = await _storeService.CreateStore(req);
        //    return StatusCode(result.StatusCode, result);
        //}
        [HttpGet(StoreEndpoint.GetListStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [SwaggerOperation(summary: "Search All Stores",
            description: "<b>Store Type: 0,1,2 (Food, Clothing, Service)<br/>" +
                         "Store Status: 0,1,2,3 (Opened, Closed, InActive, Blocked)</b>")]
        public async Task<IActionResult> SearchAllStore([FromQuery] Guid apiKey, [FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var result = await _storeService.SearchAllStore(apiKey, size, page);
            return Ok(result);
        }
        [HttpGet(StoreEndpoint.GetStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchStore(Guid id)
        {
            var result = await _storeService.SearchStore(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch(StoreEndpoint.UpdateStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        [SwaggerOperation(summary: "Update store",
            description: "<b>Store Type: 0,1,2 (Food, Clothing, Service)<br/>" +
                         "Store Status: 0,1,2,3 (Opened, Closed, InActive, Blocked)</b><br/>")]
        public async Task<IActionResult> UpdateStore(Guid id, UpdateStoreRequest req)
        {
            var result = await _storeService.UpdateStore(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete(StoreEndpoint.DeleteStore)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin)]
        [SwaggerOperation(Summary = "If delete Store, Everything in Store will be deleted")]
        public async Task<IActionResult> DeleteStore(Guid id)
        {
            var result = await _storeService.DeleteStore(id);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
        //[HttpGet(StoreEndpoint.GetMenu)]
        //[ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        //public async Task<IActionResult> GetMenu(string phone)
        //{
        //    var result = await _storeService.GetMenuFromPos(phone);
        //    return StatusCode(result.StatusCode, result);
        //}
        [HttpPost(StoreEndpoint.GetWalletStore)]
        [CustomAuthorize(RoleEnum.CashierWeb)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchWalletStore(GetWalletStoreRequest req)
        {
            var result = await _storeService.SearchWalletStore(req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost(StoreEndpoint.RequestCloseForStore)]
        //[CustomAuthorize(RoleEnum.Store)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> RequestCloseStore(Guid id)
        {
            var result = await _storeService.RequestCloseStore(id);
            return StatusCode(result.StatusCode, result);
        }
        #region CRUD Menu
        [HttpPost(StoreEndpoint.CreateMenu)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> CreateMenu(Guid storeId, CreateMenuRequest req)
        {
            var result = await _storeService.CreateMenu(storeId, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(StoreEndpoint.UpdateMenu)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> UpdateMenu(Guid id, UpdateMenuRequest req)
        {
            var result = await _storeService.UpdateMenu(id, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(StoreEndpoint.DeleteMenu)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        public async Task<IActionResult> DeleteMenu(Guid menuid)
        {
            var result = await _storeService.DeleteMenu(menuid);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetMenu)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchMenu(Guid id)
        {
            var result = await _storeService.SearchMenu(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetListMenu)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllMenu(Guid storeId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _storeService.SearchAllMenu(storeId, page, size);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
        #region CRUD Product
        [HttpPost(StoreEndpoint.CreateProduct)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> CreateProduct(Guid menuId, CreateProductRequest req)
        {
            var result = await _storeService.CreateProduct(menuId, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(StoreEndpoint.UpdateProduct)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductRequest req)
        {
            var result = await _storeService.UpdateProduct(id, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(StoreEndpoint.DeleteProduct)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _storeService.DeleteProduct(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetProduct)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchProduct(Guid id)
        {
            var result = await _storeService.SearchProduct(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetListProduct)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllProduct(Guid menuId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _storeService.SearchAllProduct(menuId, page, size);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
        #region CRUD ProductCategory
        [HttpPost(StoreEndpoint.CreateProductCategory)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> CreateProductCategory(CreateProductCategoryRequest req)
        {
            var result = await _storeService.CreateProductCategory(req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(StoreEndpoint.UpdateProductCategory)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Store)]
        public async Task<IActionResult> UpdateProductCategory(Guid id, UpdateProductCategoryRequest req)
        {
            var result = await _storeService.UpdateProductCategory(id, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(StoreEndpoint.DeleteProductCategory)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        [CustomAuthorize(RoleEnum.Admin, RoleEnum.Store)]
        public async Task<IActionResult> DeleteProductCategory(Guid id)
        {
            var result = await _storeService.DeleteProductCategory(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetProductCategory)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchProductCategory(Guid id)
        {
            var result = await _storeService.SearchProductCategory(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(StoreEndpoint.GetListProductCategory)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllProductCategory([FromQuery] Guid storeId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _storeService.SearchAllProductCategory(storeId, page, size);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
    }
}

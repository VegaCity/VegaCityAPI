using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;

// test

namespace VegaCityApp.API.Services.Interface
{
    public interface IStoreService
    {
        #region CRUD Store
        Task<ResponseAPI<Store>> CreateStore(Guid userStoreId, CreateStoreRequest req);
        Task<ResponseAPI> UpdateStore(Guid storeId, UpdateStoreRequest req);
        Task<ResponseAPI<IEnumerable<GetStoreResponse>>> SearchAllStore(Guid apiKey, int size, int page);
        Task<ResponseAPI> DeleteStore(Guid StoreId);
        Task<ResponseAPI> SearchStore(Guid StoreId);
        #endregion
        //Task<ResponseAPI> GetMenuFromPos(string phone);
        Task<ResponseAPI> SearchWalletStore(GetWalletStoreRequest req);
        Task<ResponseAPI> RequestCloseStore(Guid StoreId);
        Task<ResponseAPI> FinalSettlement(Guid StoreId, DateTime DateFinalSettlemnet);
        #region CRUD Menu
        Task<ResponseAPI> CreateMenu(Guid StoreId, CreateMenuRequest req);
        Task<ResponseAPI> UpdateMenu(Guid MenuId, UpdateMenuRequest req);
        Task<ResponseAPI> DeleteMenu(Guid MenuId);
        Task<ResponseAPI<Menu>> SearchMenu(Guid MenuId);
        Task<ResponseAPI<IEnumerable<GetMenuResponse>>> SearchAllMenu(Guid StoreId, int page, int size);
        #endregion
        #region CRUD Product
        Task<ResponseAPI> CreateProduct(Guid MenuId, CreateProductRequest req);
        Task<ResponseAPI> UpdateProduct(Guid ProductId, UpdateProductRequest req);
        Task<ResponseAPI> DeleteProduct(Guid ProductId);
        Task<ResponseAPI<Product>> SearchProduct(Guid ProductId);
        Task<ResponseAPI<IEnumerable<GetProductResponse>>> SearchAllProduct(Guid MenuId, int page, int size);
        #endregion
        #region CRUD ProductCategory
        Task<ResponseAPI> CreateProductCategory(CreateProductCategoryRequest req);
        Task<ResponseAPI> UpdateProductCategory(Guid ProductCategoryId, UpdateProductCategoryRequest req);
        Task<ResponseAPI> DeleteProductCategory(Guid ProductCategoryId);
        Task<ResponseAPI<ProductCategory>> SearchProductCategory(Guid ProductCategoryId);
        Task<ResponseAPI<IEnumerable<GetProductCategoryResponse>>> SearchAllProductCategory(Guid StoreId, int page, int size);
        #endregion
    }
}

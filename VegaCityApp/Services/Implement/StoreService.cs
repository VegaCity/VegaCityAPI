using System.Text.Json.Nodes;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Store;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class StoreService: BaseService<StoreService>, IStoreService
    {
        public StoreService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<StoreService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> UpdateStore(Guid storeId,UpdateStoreRequest req)
        {
          
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == storeId && !x.Deflag);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            //check enum
            if (!Enum.IsDefined(typeof(StoreTypeEnum), req.StoreType))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = StoreMessage.InvalidStoreType
                };
            }
            store.Id = store.Id;
            store.Name = req.Name;
            store.Status = req.StoreStatus;
            store.StoreType = req.StoreType;
            store.Address = req.Address;
            store.CrDate = TimeUtils.GetCurrentSEATime();
            store.PhoneNumber = req.PhoneNumber;
            store.ShortName = req.ShortName;
            store.Email = req.Email;
            store.MarketZoneId = Guid.Parse(EnvironmentVariableConstant.MarketZoneId);
            store.Description = req.Description;
            _unitOfWork.GetRepository<Store>().UpdateAsync(store);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateStoreSuccesss,
                    StatusCode = HttpStatusCodes.OK,
                    
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateStoreSuccesss,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }

        public async Task<IPaginate<GetStoreResponse>> SearchAllStore(int size, int page)
        {
            IPaginate<GetStoreResponse> data = await _unitOfWork.GetRepository<Store>().GetPagingListAsync(

                selector: x => new GetStoreResponse()
                {
                    Id = x.Id,
                    StoreType = x.StoreType,
                    Name = x.Name,
                    Address = x.Address,
                    Description = x.Description,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    PhoneNumber = x.PhoneNumber,
                    MarketZoneId = x.MarketZoneId,
                   ShortName = x.ShortName,
                   Email = x.Email,
                   HouseId = x.HouseId,
                   Status = x.Status
                   
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag 
            );
            return data;
        }

        public async Task<ResponseAPI> SearchStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId && !x.Deflag,
                include: store => store
                    .Include(y => y.Menus).ThenInclude(y => y.ProductCategories)
                    .Include(y => y.Orders)
                    .Include(y => y.Users)
            );
            store.Users = store.Users.Select(x => new User{ 
                Id = x.Id,
                FullName = x.FullName,
                IsChange = x.IsChange,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                Email = x.Email,
                Gender = x.Gender,
                Birthday = x.Birthday,
                Cccd = x.Cccd,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Status = x.Status
            }).ToList();
            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.GetStoreSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    store,
                }
            };
        }

        public async Task<ResponseAPI> DeleteStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == StoreId && !x.Deflag);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }

            store.Deflag = true;
            _unitOfWork.GetRepository<Store>().UpdateAsync(store);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeletedSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeleteFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }

        public async Task<ResponseAPI> GetMenuFromPos(Guid id)
        {
             //call api pos - take n parse into Object Menu
             var data = await CallApiUtils.CallApiGetEndpoint(
                 "https://6504066dc8869921ae2466d4.mockapi.io/api/Product");
             var productsPosResponse = await CallApiUtils.GenerateObjectFromResponse<List<ProductsPosResponse>>(data);
             //lưu chuỗi json này
             //parse object list sang json
             string json = JsonConvert.SerializeObject(productsPosResponse);
             //check menu
             var checkMenu = await _unitOfWork.GetRepository<Menu>()
                 .SingleOrDefaultAsync(predicate: x => x.StoreId == id && !x.Deflag);
             var store = await _unitOfWork.GetRepository<Store>()
                 .SingleOrDefaultAsync(predicate: x => x.Id == id && !x.Deflag);
            if (checkMenu == null)
            {
                var newMenu = new Menu()
                {
                    Id = Guid.NewGuid(),
                    StoreId = id,
                    Deflag = false,
                    Address = store.Address,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    ImageUrl = "string",
                    MenuJson = json,
                    Name = store.ShortName + "Menu",
                    PhoneNumber = store.PhoneNumber
                };
                await _unitOfWork.GetRepository<Menu>().InsertAsync(newMenu);
                await _unitOfWork.CommitAsync();
                // tim productcategory, insert vao
                bool check = await InsertProductCategory(productsPosResponse, id);
                return check?new ResponseAPI()
                {
                    MessageResponse = "Get Successfully!!",
                    StatusCode = HttpStatusCodes.OK,
                    Data = productsPosResponse
                }: new ResponseAPI()
                {
                    MessageResponse = "Fail add product category",
                    StatusCode = HttpStatusCodes.BadRequest,
                };
            }
            else
            {
                checkMenu.MenuJson = json;
                _unitOfWork.GetRepository<Menu>().UpdateAsync(checkMenu);
                await _unitOfWork.CommitAsync();
                bool check = await InsertProductCategory(productsPosResponse, id);
                return check ? new ResponseAPI()
                {
                    MessageResponse = "Get Successfully!!",
                    StatusCode = HttpStatusCodes.OK,
                    Data = productsPosResponse
                } : new ResponseAPI()
                {
                    MessageResponse = "Fail add product category",
                    StatusCode = HttpStatusCodes.BadRequest,
                };
            }
        }

        private async Task<bool> InsertProductCategory(List<ProductsPosResponse> listProduct, Guid storeId)
        {
            List<ProductFromPos> products = new List<ProductFromPos>();
            //chay ham for cho productCate name
            var selectField = listProduct.Select(p => new
            {
                p.ProductCategory
            }).Distinct().ToList();
            foreach (var Category in selectField)
            {
                var productCategory = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync(
                    predicate: x=> x.Name == Category.ProductCategory);
                if (productCategory == null)
                {
                    foreach (var product in listProduct)
                    {
                        if (product.ProductCategory == Category.ProductCategory)
                        {
                            products.Add(new ProductFromPos()
                            {
                                Name = product.Name,
                                Id = product.Id,
                                ImgUrl = product.ImgUrl,
                                Price = product.Price
                            });
                        }
                    }

                    var json = JsonConvert.SerializeObject(products);
                    var newProductCateGory = new ProductCategory()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Name = Category.ProductCategory,
                        ProductJson = json,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<ProductCategory>().InsertAsync(newProductCateGory);
                    await _unitOfWork.CommitAsync();
                    //xoa product
                    products.Clear();
                }
                else
                {
                    foreach (var product in listProduct)
                    {
                        if (product.ProductCategory == Category.ProductCategory)
                        {
                            products.Add(new ProductFromPos()
                            {
                                Name = product.Name,
                                Id = product.Id,
                                ImgUrl = product.ImgUrl,
                                Price = product.Price
                            });
                        }
                    }

                    var json = JsonConvert.SerializeObject(products);
                    productCategory.UpsDate = TimeUtils.GetCurrentSEATime();
                    productCategory.ProductJson = json;
                    _unitOfWork.GetRepository<ProductCategory>().UpdateAsync(productCategory);
                    await _unitOfWork.CommitAsync();
                    //xoa product
                    products.Clear();
                }
            }
            return true;
        }
    }
}

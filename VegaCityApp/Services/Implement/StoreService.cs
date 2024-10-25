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
            Guid apiKey = GetMarketZoneIdFromJwt();
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync
                (predicate: x => x.Id == storeId && !x.Deflag &&
                                 x.MarketZoneId == apiKey);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            store.Name = req.Name != null? req.Name.Trim() : store.Name;
            if (!Enum.IsDefined(typeof(StoreStatusEnum), req.StoreStatus))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = StoreMessage.InvalidStoreStatus
                };
            }
            store.Status = req.StoreStatus;
            if (!Enum.IsDefined(typeof(StoreTypeEnum), req.StoreType))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = StoreMessage.InvalidStoreType
                };
            }
            store.StoreType = req.StoreType;
            store.Address = req.Address != null ? req.Address.Trim() : store.Address;
            store.PhoneNumber = req.PhoneNumber != null ? req.PhoneNumber.Trim() : store.PhoneNumber;
            store.ShortName = req.ShortName != null ? req.ShortName.Trim() : store.ShortName;
            store.Email = req.Email != null ? req.Email.Trim() : store.Email;
            store.Description = req.Description != null ? req.Description.Trim() : store.Description;
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
        public async Task<ResponseAPI<IEnumerable<GetStoreResponse>>> SearchAllStore(Guid apiKey, int size, int page)
        {
            try
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
                    Status = x.Status,
                    ZoneName = x.House.Zone.Name

                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag && x.MarketZoneId == apiKey,
                include: store => store.Include(y => y.House).ThenInclude(y => y.Zone)
                );
                return new ResponseAPI<IEnumerable<GetStoreResponse>>
                {
                    MessageResponse = StoreMessage.GetListStoreSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = data.Items,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<GetStoreResponse>>
                {
                    MessageResponse = StoreMessage.GetListStoreFailed + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
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
                CccdPassport = x.CccdPassport,
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
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync
                (predicate: x => x.Id == StoreId && !x.Deflag,
                 include: inStore => inStore.Include(z => z.Menus)
                                            .Include(z => z.DisputeReports)
                                            .Include(z => z.StoreServices).ThenInclude(a => a.WalletTypeStoreServiceMappings));
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            //delete every thing related to store
            if(store.Menus.Count > 0)
            {
                foreach (var menu in store.Menus)
                {
                    menu.Deflag = true;
                    _unitOfWork.GetRepository<Menu>().UpdateAsync(menu);
                }
            }
            if(store.DisputeReports.Count > 0)
            {
                foreach (var dispute in store.DisputeReports)
                {
                    _unitOfWork.GetRepository<DisputeReport>().DeleteAsync(dispute);
                }
            }
            if (store.StoreServices.Count > 0)
            {
                if(store.StoreServices.Where(x => x.WalletTypeStoreServiceMappings.Count > 0).Count() > 0)
                {
                    foreach (var storeService in store.StoreServices)
                    {
                        if (storeService.WalletTypeStoreServiceMappings.Count > 0)
                        {
                            foreach (var walletTypeStoreServiceMapping in storeService.WalletTypeStoreServiceMappings)
                            {
                                _unitOfWork.GetRepository<WalletTypeStoreServiceMapping>().DeleteAsync(walletTypeStoreServiceMapping);
                            }
                        }
                        storeService.Deflag = true;
                        storeService.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Domain.Models.StoreService>().UpdateAsync(storeService);
                    }
                }
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
              // "https://6504066dc8869921ae2466d4.mockapi.io/api/Product"
              $"https://localhost:7131/api/v1/menus/{id}/menus"
                 );
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
                bool check = await InsertProductCategory(productsPosResponse, newMenu.Id);
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
                bool check = await InsertProductCategory(productsPosResponse, checkMenu.Id);
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

        private async Task<bool> InsertProductCategory(List<ProductsPosResponse> listProduct, Guid MenuId)
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
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        MenuId = MenuId
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

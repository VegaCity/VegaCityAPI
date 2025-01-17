﻿using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
    public class StoreService : BaseService<StoreService>, IStoreService
    {
        private readonly IUtilService _util;

        public StoreService(IUnitOfWork<VegaCityAppContext> unitOfWork,
                            ILogger<StoreService> logger,
                            IHttpContextAccessor httpContextAccessor,
                            IMapper mapper,
                            IUtilService util) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _util = util;
        }
        #region CRUD Store
        public async Task<ResponseAPI<Store>> CreateStore(Guid ownerStoreId, CreateStoreRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            req.Name = req.Name.Trim();
            req.Address = req.Address.Trim();
            req.ShortName = req.ShortName?.Trim();
            req.Description = req.Description?.Trim();
            req.PhoneNumber = req.PhoneNumber.Trim();
            req.Email = req.Email.Trim();
            if (!Enum.IsDefined(typeof(StoreTypeEnum), req.StoreType))
                throw new BadHttpRequestException(StoreMessage.InvalidStoreType, HttpStatusCodes.BadRequest);
            if (!Enum.IsDefined(typeof(StoreStatusEnum), req.Status))
                throw new BadHttpRequestException(StoreMessage.InvalidStoreStatus, HttpStatusCodes.BadRequest);
            var apiKey = GetMarketZoneIdFromJwt();
            var newStore = _mapper.Map<Store>(req);
            newStore.Id = Guid.NewGuid();
            newStore.MarketZoneId = apiKey;
            newStore.CrDate = TimeUtils.GetCurrentSEATime();
            newStore.UpsDate = TimeUtils.GetCurrentSEATime();
            newStore.Deflag = false;
            await _unitOfWork.GetRepository<Store>().InsertAsync(newStore);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI<Store>
            {
                MessageResponse = StoreMessage.CreateStoreSuccess,
                StatusCode = HttpStatusCodes.Created,
                Data = newStore
            };
        }
        public async Task<ResponseAPI> UpdateStore(Guid storeId, UpdateStoreRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            string roleJwt = GetRoleFromJwt();
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync
                (predicate: x => x.Id == storeId && !x.Deflag);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            if (roleJwt == RoleEnum.Admin.GetDescriptionFromEnum())
            {
                if (!Enum.IsDefined(typeof(StoreTypeEnum), req.StoreType))
                {
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = StoreMessage.InvalidStoreStatus
                    };
                }
                store.StoreTransferRate = req.StoreTransferRate != null ? (double)req.StoreTransferRate : store.StoreTransferRate;
                store.StoreType = req.StoreType != null ? (int)req.StoreType : store.StoreType;
            }
            store.Name = req.Name != null ? req.Name.Trim() : store.Name;
            if (req.Status != null)
            {
                if (!Enum.IsDefined(typeof(StoreStatusEnum), req.Status))
                {
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = StoreMessage.InvalidStoreStatus
                    };
                }
                store.Status = (int)req.Status;
            }

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
                    Status = x.Status,
                    ZoneName = x.Zone.Name,
                    ZoneId = x.ZoneId
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag && x.MarketZoneId == apiKey,
                include: h => h.Include(z => z.Zone)
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
                include: z => z.Include(s => s.Wallets).ThenInclude(t => t.WalletType)
                               .Include(a => a.Menus)
            );
            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            string storeType = null;
            //check storetype enum and parse to string
            if (!StoreTypeHelper.allowedStoreTypes.Contains((int)store.StoreType))
            {
                throw new BadHttpRequestException(StoreMessage.InvalidStoreType, HttpStatusCodes.BadRequest);
            }
            else
            {
                if (store.StoreType == (int)StoreTypeEnum.Service)
                {
                    storeType = StoreTypeEnum.Service.GetDescriptionFromEnum();
                }
                else if (store.StoreType == (int)StoreTypeEnum.Food)
                {
                    storeType = StoreTypeEnum.Food.GetDescriptionFromEnum();
                }
                else if (store.StoreType == (int)StoreTypeEnum.Other)
                {
                    storeType = StoreTypeEnum.Other.GetDescriptionFromEnum();
                }
                else if (store.StoreType == (int)StoreTypeEnum.Product)
                {
                    storeType = StoreTypeEnum.Product.GetDescriptionFromEnum();
                }
            }
            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.GetStoreSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    storeType,
                    store
                }
            };
        }
        public async Task<ResponseAPI> DeleteStore(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync
                (predicate: x => x.Id == StoreId && !x.Deflag,
                 include: z => z.Include(a => a.Menus).ThenInclude(a => a.MenuProductMappings));
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            //delete every thing related to store
            if (store.Menus.Count > 0)
            {
                foreach (var menu in store.Menus)
                {
                    menu.Deflag = true;
                    _unitOfWork.GetRepository<Menu>().UpdateAsync(menu);
                    foreach (var item in menu.MenuProductMappings)
                    {
                        var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(predicate: x => x.Id == item.ProductId);
                        product.Status = "InActive";
                        product.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Product>().UpdateAsync(product);
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
        #endregion
        #region CRUD Menu
        public async Task<ResponseAPI> CreateMenu(Guid StoreId, CreateMenuRequest req)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId && !x.Deflag);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundStore
                };
            }
            var menus = await _unitOfWork.GetRepository<Menu>().GetListAsync(
                predicate: x => x.StoreId == StoreId && !x.Deflag);
            if (menus.Count > 0)
            {
                foreach (var menu in menus)
                {
                    if (menu.DateFilter ==(int) req.DateFilter)
                    {
                        return new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = StoreMessage.MenuExist
                        };
                    }
                }
            }
            if (!Enum.IsDefined(typeof(DateFilterEnum), req.DateFilter))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = StoreMessage.InvalidDateFilter
                };
            }
            req.ImageUrl = req.ImageUrl?.Trim();
            req.Name = req.Name.Trim();
            var newMenu = _mapper.Map<Menu>(req);
            newMenu.Id = Guid.NewGuid();
            newMenu.CrDate = TimeUtils.GetCurrentSEATime();
            newMenu.UpsDate = TimeUtils.GetCurrentSEATime();
            newMenu.Deflag = false;
            newMenu.StoreId = store.Id;
            await _unitOfWork.GetRepository<Menu>().InsertAsync(newMenu);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.CreateMenuSuccess,
                StatusCode = HttpStatusCodes.Created
            };
        }
        public async Task<ResponseAPI> UpdateMenu(Guid MenuId, UpdateMenuRequest req)
        {
            var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync
                (predicate: x => x.Id == MenuId && !x.Deflag);
            if (menu == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundMenu
                };
            }
            var menus = await _unitOfWork.GetRepository<Menu>().GetListAsync(
                predicate: x => x.StoreId == menu.StoreId && !x.Deflag);
            if (menus.Count > 0)
            {
                foreach (var item in menus)
                {
                    if (item.DateFilter == req.DateFilter)
                    {
                        return new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = StoreMessage.MenuExist
                        };
                    }
                }
            }
            if (req.DateFilter != null)
            {
                if (!Enum.IsDefined(typeof(DateFilterEnum), req.DateFilter))
                {
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = StoreMessage.InvalidDateFilter
                    };
                }
                req.DateFilter = req.DateFilter;
            }
            menu.Name = req.Name.Trim();
            menu.ImageUrl = req.ImageUrl?.Trim();
            menu.DateFilter = req.DateFilter;
            menu.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Menu>().UpdateAsync(menu);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateMenuSuccess,
                    StatusCode = HttpStatusCodes.OK
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateMenuFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }
        public async Task<ResponseAPI> DeleteMenu(Guid MenuId)
        {
            var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync
                (predicate: x => x.Id == MenuId && !x.Deflag,
                 include: z => z.Include(a => a.MenuProductMappings));
            if (menu == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundMenu
                };
            }
            menu.Deflag = true;
            _unitOfWork.GetRepository<Menu>().UpdateAsync(menu);
            foreach (var item in menu.MenuProductMappings)
            {
                var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(predicate: x => x.Id == item.ProductId);
                product.Status = "InActive";
                product.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Product>().UpdateAsync(product);
            }
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeletedMenuSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeletedMenuFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        public async Task<ResponseAPI<IEnumerable<GetMenuResponse>>> SearchAllMenu(Guid StoreId, int page, int size)
        {
            try
            {
                IPaginate<GetMenuResponse> data = await _unitOfWork.GetRepository<Menu>().GetPagingListAsync(
                    selector: x => new GetMenuResponse()
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
                        ImageUrl = x.ImageUrl,
                        Name = x.Name,
                        CrDate = x.CrDate,
                        UpsDate = x.UpsDate,
                        Deflag = x.Deflag,
                        DateFilter = x.DateFilter
                    },
                    page: page,
                    size: size,
                    orderBy: x => x.OrderByDescending(z => z.Name),
                    predicate: x => x.StoreId == StoreId && !x.Deflag);
                return new ResponseAPI<IEnumerable<GetMenuResponse>>
                {
                    MessageResponse = StoreMessage.GetListMenuSuccess,
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
                return new ResponseAPI<IEnumerable<GetMenuResponse>>
                {
                    MessageResponse = StoreMessage.GetListMenuFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<Menu>> SearchMenu(Guid MenuId)
        {
            var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
                predicate: x => x.Id == MenuId && !x.Deflag,
                include: z => z.Include(a => a.MenuProductMappings).ThenInclude(a => a.Product).ThenInclude(a => a.ProductCategory));
            if (menu == null)
            {
                return new ResponseAPI<Menu>()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundMenu
                };
            }
            return new ResponseAPI<Menu>()
            {
                MessageResponse = StoreMessage.GetMenuSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = menu
            };
        }
        #endregion
        public async Task<ResponseAPI> SearchWalletStore(GetWalletStoreRequest req)
        {
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                throw new BadHttpRequestException(PackageItemMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);

            //var searchName = NormalizeString(req.StoreName);
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.PhoneNumber == req.PhoneNumber
                                                                                        && (x.Status == (int)UserStatusEnum.Active || x.Status == (int)UserStatusEnum.Disable)
                                                                                       ,include: w => w.Include(w => w.Wallets)
                                                                                                       .Include(u => u.UserStoreMappings)
                                                                                                       .ThenInclude(z => z.Store))
            ?? throw new BadHttpRequestException(UserMessage.NotFoundUser, HttpStatusCodes.NotFound);

            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == user.StoreId && (x.Status == (int)StoreStatusEnum.Opened || x.Status == (int)StoreStatusEnum.Closed || x.Status == (int)StoreStatusEnum.Blocked)
                                                                                       ,include: w => w.Include(u => u.Wallets)
                                                                                       .Include(z => z.Zone)
                                                                                       .ThenInclude(z => z.MarketZone)
                                                                                       .ThenInclude(a => a.MarketZoneConfig));
            //var storeTrack = stores.SingleOrDefault(x => NormalizeString(x.Name) == searchName || NormalizeString(x.ShortName) == searchName);
            if (user.StoreId == null)
            {
                throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);
            }
            if(user.Wallets.SingleOrDefault().StoreId == null)
            {
                throw new BadHttpRequestException(WalletTypeMessage.NotFoundWallet, HttpStatusCodes.NotFound);
            }
            if (store.Wallets.SingleOrDefault().Deflag == true)
            {
                throw new BadHttpRequestException(StoreMessage.StoreWalletIsPendingClose, HttpStatusCodes.BadRequest);
            }
            var storeTrack = store;
            //    predicate: x => x.Id == storeTrack.Id,
            //    include: z => z)
            //find order list store
            var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(
                predicate: x => x.StoreId == store.Id && (x.Status == OrderStatus.Completed || x.Status == OrderStatus.Renting) &&
                x.UpsDate <= TimeUtils.GetCurrentSEATime(), include: z => z.Include(z => z.Payments));
            if (orders.Count <= 0) throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            List<Payment> payments = new List<Payment>(); //list payment QRCode
            foreach (var order in orders)
            {
                foreach (var payment in order.Payments)
                {
                    if (payment.Name == PaymentTypeEnum.QRCode.GetDescriptionFromEnum())
                    {
                        payments.Add(payment);
                    }
                }
            }
            int AmountPayment = 0;
            foreach (var payment in payments)
            {
                AmountPayment += payment.FinalAmount;
            }
            int AmountTransfered = (int)(AmountPayment - AmountPayment * store.StoreTransferRate); // amount transfered to store
            //find list orders store
            var storeMoneyTransfers = await _unitOfWork.GetRepository<StoreMoneyTransfer>().GetListAsync(
                predicate: x => x.StoreId == store.Id && x.Status == OrderStatus.Completed &&
                x.UpsDate <= TimeUtils.GetCurrentSEATime());
            if (storeMoneyTransfers.Count <= 0) throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            var transactionStoreWithdraws = (List<Transaction>)await _unitOfWork.GetRepository<Transaction>().GetListAsync(
                predicate: x => x.StoreId == store.Id && x.Status == TransactionStatus.Success &&
                x.Type == TransactionType.WithdrawMoney);
            List<StoreMoneyTransfer> storeMoneyTransfersListToStore = new List<StoreMoneyTransfer>();

            foreach (var storeTransfer in storeMoneyTransfers)
            {
                if (storeTransfer.Description.Split(" to ")[1].Trim() == "store")
                {
                    storeMoneyTransfersListToStore.Add(storeTransfer);
                }
            }
            int amoutWithdrawed = 0;
            foreach (var transaction in transactionStoreWithdraws)
            {
                amoutWithdrawed += transaction.Amount; //amount withdrawed from store
            }
            int amount = 0;
            foreach (var storeTransfer in storeMoneyTransfersListToStore)
            {
                amount += storeTransfer.Amount;
            }
            int amountFinal = AmountTransfered - amoutWithdrawed;
            if (AmountTransfered != amount)
            {
                //throw new BadHttpRequestException(OrderMessage.AmountNotEqual, HttpStatusCodes.BadRequest);
                return new ResponseAPI
                {
                    MessageResponse = StoreMessage.GetStoreSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new { 
                        storeTrack,
                        AmountTransfered,
                        amoutWithdrawed,
                        amountCanWithdraw = AmountTransfered != amount ? 0 : amountFinal
                    },
                };
            }
            
            return new ResponseAPI
            {
                MessageResponse = StoreMessage.GetStoreSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new { 
                    storeTrack,
                    AmountTransfered,
                    amoutWithdrawed,
                    amountCanWithdraw = amountFinal 
                },
            };
        }
        public async Task<ResponseAPI> RequestCloseStore(Guid StoreId)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId && !x.Deflag, include: y => y.Include(z => z.UserStoreMappings));
            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            if (store.Status == (int)StoreStatusEnum.InActive)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.StorePendingVerifyClose,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var storeAccount = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == GetUserIdFromJwt());
            storeAccount.Status = (int)UserStatusEnum.Disable;
            storeAccount.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<User>().UpdateAsync(storeAccount);
            store.Status = (int)StoreStatusEnum.InActive; //implement count 7 days from blocked status (UPSDATE) here
            store.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Store>().UpdateAsync(store);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                #region send mail
                try
                {
                    var admin = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(EnvironmentVariableConstant.marketZoneId));
                    var subject = UserMessage.PendingApproveCloseStore;
                    //var body = "You have new Closing Store request is pending: " + store.Name;
                    var body = $"<div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>" +
                           $"<h1 style='color: #007bff;'>Welcome to our Vega City!</h1>" +
                           $"<p>Thanks for closing Store request.</p>" +
                           $"<p><strong>You have new Closing Store request is pending: {store.Name}</strong></p>" +
                       $"</div>";
                    await MailUtil.SendMailAsync(admin.Email, subject, body);
                }
                catch (Exception ex)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.SendMailFail
                    };
                }
                #endregion
                return new ResponseAPI()
                {

                    MessageResponse = UserMessage.PendingApproveCloseStore,
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

        //public async Task<ResponseAPI> GetMenuFromPos(string phone)
        //{
        //    //call api pos - take n parse into Object Menu
        //    var data = await CallApiUtils.CallApiGetEndpoint(
        //     // "https://6504066dc8869921ae2466d4.mockapi.io/api/Product"
        //     $"https://localhost:7131/api/v1/menus/{phone}/menus"
        //        );
        //    var productsPosResponse = await CallApiUtils.GenerateObjectFromResponse<List<ProductsPosResponse>>(data);
        //    //lưu chuỗi json này
        //    //parse object list sang json
        //    string json = JsonConvert.SerializeObject(productsPosResponse);
        //    //check menu
        //    var checkMenu = await _unitOfWork.GetRepository<Menu>()
        //        .SingleOrDefaultAsync(predicate: x => x.Store.PhoneNumber == phone && !x.Deflag);
        //    var store = await _unitOfWork.GetRepository<Store>()
        //        .SingleOrDefaultAsync(predicate: x => x.PhoneNumber == phone && !x.Deflag, include: z => z.Include(a => a.Wallets))
        //           ?? throw new BadHttpRequestException("Store not found", HttpStatusCodes.NotFound);
        //    if (store.Wallets.Count > 0)
        //    {

        //        if (store.StoreType.GetDescriptionFromEnum() == StoreTypeEnum.Service.GetDescriptionFromEnum())
        //            throw new BadHttpRequestException("This store is service store, not support menu");
        //        if (checkMenu == null)
        //        {
        //            var newMenu = new Menu()
        //            {
        //                Id = Guid.NewGuid(),
        //                StoreId = store.Id,
        //                Deflag = false,
        //                Address = store.Address,
        //                CrDate = TimeUtils.GetCurrentSEATime(),
        //                ImageUrl = "string",
        //                MenuJson = json,
        //                Name = store.ShortName + " Menu",
        //                PhoneNumber = store.PhoneNumber,
        //                UpsDate = TimeUtils.GetCurrentSEATime()
        //            };
        //            await _unitOfWork.GetRepository<Menu>().InsertAsync(newMenu);
        //            await _unitOfWork.CommitAsync();
        //            // tim productcategory, insert vao
        //            bool check = await InsertProductCategory(productsPosResponse, newMenu.Id);
        //            return check ? new ResponseAPI()
        //            {
        //                MessageResponse = "Synchronization successful",
        //                StatusCode = HttpStatusCodes.OK,
        //                Data = productsPosResponse
        //            } : new ResponseAPI()
        //            {
        //                MessageResponse = "Fail add product category",
        //                StatusCode = HttpStatusCodes.BadRequest,
        //            };
        //        }
        //        else
        //        {
        //            checkMenu.MenuJson = json;
        //            checkMenu.CrDate = TimeUtils.GetCurrentSEATime();
        //            _unitOfWork.GetRepository<Menu>().UpdateAsync(checkMenu);
        //            await _unitOfWork.CommitAsync();
        //            bool check = await InsertProductCategory(productsPosResponse, checkMenu.Id);
        //            return check ? new ResponseAPI()
        //            {
        //                MessageResponse = "Synchronization successful",
        //                StatusCode = HttpStatusCodes.OK,
        //                Data = productsPosResponse
        //            } : new ResponseAPI()
        //            {
        //                MessageResponse = "Fail add product category",
        //                StatusCode = HttpStatusCodes.BadRequest,
        //            };
        //        }
        //    }
        //    else
        //    {
        //        throw new BadHttpRequestException("Wallet not found", HttpStatusCodes.NotFound);
        //    }
        //}

        //private async Task<bool> InsertProductCategory(List<ProductsPosResponse> listProduct, Guid MenuId)
        //{
        //    List<ProductFromPos> products = new List<ProductFromPos>();
        //    //chay ham for cho productCate name
        //    var selectField = listProduct.Select(p => new
        //    {
        //        p.ProductCategory
        //    }).Distinct().ToList();
        //    foreach (var Category in selectField)
        //    {
        //        var productCategory = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync(
        //            predicate: x => x.Name == Category.ProductCategory && !x.Deflag);
        //        if (productCategory == null)
        //        {
        //            var newProductCateGory = new ProductCategory()
        //            {
        //                Id = Guid.NewGuid(),
        //                CrDate = TimeUtils.GetCurrentSEATime(),
        //                Name = Category.ProductCategory,
        //                Deflag = false,
        //                Description = "string",
        //                UpsDate = TimeUtils.GetCurrentSEATime()
        //            };
        //            await _unitOfWork.GetRepository<ProductCategory>().InsertAsync(newProductCateGory);
        //            var walletTypes = await _unitOfWork.GetRepository<WalletType>().GetListAsync(predicate: x => x.MarketZoneId == Guid.Parse(EnvironmentVariableConstant.marketZoneId),
        //            include: m => m.Include(n => n.WalletTypeMappings));

        //            foreach (var walletType in walletTypes)
        //            {
        //                if (walletType.Name == "SpecificWallet" || walletType.Name == "ServiceWallet")
        //                {

        //                    // Check if the mapping already exists between wallet type and product category
        //                    var existingMapping = await _unitOfWork.GetRepository<WalletTypeMapping>().SingleOrDefaultAsync(
        //                        predicate: x => x.WalletTypeId == walletType.Id && x.ProductCategoryId == newProductCateGory.Id);

        //                    if (existingMapping == null)
        //                    {
        //                        var newProductCategoryMappingWallet = new WalletTypeMapping
        //                        {
        //                            Id = Guid.NewGuid(),
        //                            WalletTypeId = walletType.Id,
        //                            ProductCategoryId = newProductCateGory.Id,
        //                        };
        //                        await _unitOfWork.GetRepository<WalletTypeMapping>().InsertAsync(newProductCategoryMappingWallet);
        //                    }
        //                }
        //            }
        //            foreach (var product in listProduct)
        //            {
        //                if (product.ProductCategory == Category.ProductCategory)
        //                {
        //                    var newProduct = new Product()
        //                    {
        //                        Id = Guid.Parse(product.Id),
        //                        CrDate = TimeUtils.GetCurrentSEATime(),
        //                        ImageUrl = product.ImgUrl,
        //                        ProductCategoryId = newProductCateGory.Id,
        //                        Status = "Active",
        //                        MenuId = MenuId,
        //                        Name = product.Name,
        //                        Price = product.Price,
        //                        UpsDate = TimeUtils.GetCurrentSEATime()
        //                    };
        //                    await _unitOfWork.GetRepository<Product>().InsertAsync(newProduct);

        //                }
        //            }
        //            await _unitOfWork.CommitAsync();
        //            //xoa product
        //            products.Clear();
        //        }
        //        else
        //        {
        //            var walletTypes = await _unitOfWork.GetRepository<WalletType>().GetListAsync
        //                (predicate: x => x.MarketZoneId == Guid.Parse(EnvironmentVariableConstant.marketZoneId),
        //               include: m => m.Include(n => n.WalletTypeMappings));
        //            foreach (var walletType in walletTypes)
        //            {
        //                if (walletType.Name == "SpecificWallet" || walletType.Name == "ServiceWallet")
        //                {
        //                    foreach (var item in selectField)
        //                    {
        //                        // Retrieve the ProductCategory based on the current item's category name
        //                        var productCategory2 = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync(
        //                            predicate: x => x.Name == item.ProductCategory && !x.Deflag);

        //                        if (productCategory2 != null) // Ensure productCategory2 is found
        //                        {
        //                            // Check if this mapping already exists
        //                            var existingMapping = await _unitOfWork.GetRepository<WalletTypeMapping>().SingleOrDefaultAsync(
        //                                predicate: x => x.WalletTypeId == walletType.Id && x.ProductCategoryId == productCategory2.Id);

        //                            if (existingMapping == null)
        //                            {
        //                                var newProductCategoryMappingWallet = new WalletTypeMapping
        //                                {
        //                                    Id = Guid.NewGuid(),
        //                                    WalletTypeId = walletType.Id,
        //                                    ProductCategoryId = productCategory2.Id,
        //                                };
        //                                await _unitOfWork.GetRepository<WalletTypeMapping>().InsertAsync(newProductCategoryMappingWallet);
        //                            }
        //                        }
        //                    }
        //                }
        //            }


        //            foreach (var product in listProduct)
        //            {
        //                if(product.ProductCategory == Category.ProductCategory)
        //                {
        //                    var productExist = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //                        predicate: x => x.Id == Guid.Parse(product.Id) && x.ProductCategoryId == productCategory.Id && x.Status == "Active");
        //                    if (productExist == null)
        //                    {
        //                        var newProduct = new Product()
        //                        {
        //                            Id = Guid.Parse(product.Id),
        //                            CrDate = TimeUtils.GetCurrentSEATime(),
        //                            ImageUrl = product.ImgUrl,
        //                            ProductCategoryId = productCategory.Id,
        //                            Status = "Active",
        //                            MenuId = MenuId,
        //                            Name = product.Name,
        //                            Price = product.Price,
        //                            UpsDate = TimeUtils.GetCurrentSEATime()
        //                        };
        //                        await _unitOfWork.GetRepository<Product>().InsertAsync(newProduct);
        //                    }
        //                    else
        //                    {
        //                        productExist.Name = product.Name;
        //                        productExist.ImageUrl = product.ImgUrl;
        //                        productExist.Price = product.Price;
        //                        productExist.UpsDate = TimeUtils.GetCurrentSEATime();
        //                        _unitOfWork.GetRepository<Product>().UpdateAsync(productExist);
        //                    }
        //                    products.Add(new ProductFromPos()
        //                    {
        //                        Name = product.Name,
        //                        Id = product.Id,
        //                        ImgUrl = product.ImgUrl,
        //                        Price = product.Price
        //                    });
        //                }
        //            }
        //            productCategory.UpsDate = TimeUtils.GetCurrentSEATime();
        //            _unitOfWork.GetRepository<ProductCategory>().UpdateAsync(productCategory);
        //            await _unitOfWork.CommitAsync();
        //            //xoa product
        //            products.Clear();
        //        }
        //    }
        //    return true;
        //}
        private static string NormalizeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Chuẩn hóa chuỗi để loại bỏ dấu
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Chuẩn hóa chuỗi, chuyển thành chữ thường và loại bỏ khoảng trắng
            var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
            result = Regex.Replace(result, @"\s+", ""); // Loại bỏ tất cả khoảng trắng

            return result;
        }
        #region CRUD Product
        public async Task<ResponseAPI> CreateProduct(Guid MenuId, CreateProductRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            if (req.Quantity <= 0) throw new BadHttpRequestException("Invalid Quantity", HttpStatusCodes.BadRequest);
            if (req.Price <= 0) throw new BadHttpRequestException(StoreMessage.InvalidProductPrice, HttpStatusCodes.BadRequest);
            var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
                predicate: x => x.Id == MenuId && !x.Deflag, include: z => z.Include(a => a.Store));
            if (menu == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundMenu
                };
            }
            if (menu.Store.StoreType == (int)StoreTypeEnum.Service)
            {
                if (req.Duration <= 0) throw new BadHttpRequestException("Invalid Duration", HttpStatusCodes.BadRequest);
                if (!(req.Unit.Equals(UnitEnum.Hour.GetDescriptionFromEnum()) || req.Unit.Equals(UnitEnum.Minute.GetDescriptionFromEnum()))) throw new BadHttpRequestException("Invalid Unit, The valid Unit is: Hour, Minute", HttpStatusCodes.BadRequest);
                req.Unit = req.Unit.Trim();
            }
            var productCategory = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == req.ProductCategoryId && !x.Deflag)
                ?? throw new BadHttpRequestException(StoreMessage.NotFoundProductCategory, HttpStatusCodes.NotFound);
            var newProduct = _mapper.Map<Product>(req);
            if (menu.Store.StoreType == (int)StoreTypeEnum.Service)
            {
                newProduct.Unit = req.Unit;
                newProduct.Duration = req.Duration;
            }
            newProduct.ImageUrl = req.ImageUrl?.Trim();
            newProduct.Name = req.Name.Trim();
            newProduct.Id = Guid.NewGuid();
            newProduct.CrDate = TimeUtils.GetCurrentSEATime();
            newProduct.UpsDate = TimeUtils.GetCurrentSEATime();
            newProduct.Status = "Active";
            newProduct.MenuId = menu.Id;
            newProduct.Quantity = req.Quantity;
            await _unitOfWork.GetRepository<Product>().InsertAsync(newProduct);
            //insert mapping
            var newMenuProductMapping = new MenuProductMapping()
            {
                Id = Guid.NewGuid(),
                MenuId = menu.Id,
                ProductId = newProduct.Id,
                CrDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<MenuProductMapping>().InsertAsync(newMenuProductMapping);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.CreateProductSuccess,
                StatusCode = HttpStatusCodes.Created,
                Data = newProduct
            };
        }
        public async Task<ResponseAPI> UpdateProduct(Guid ProductId, UpdateProductRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            if (req.Quantity <= 0) throw new BadHttpRequestException("Invalid Quantity", HttpStatusCodes.BadRequest);

            if (req.Price != null)
            {
                if (req.Price <= 0) throw new BadHttpRequestException(StoreMessage.InvalidProductPrice, HttpStatusCodes.BadRequest);
            }
            var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync
                (predicate: x => x.Id == ProductId && x.Status == "Active",
                 include: y => y.Include(t => t.ProductCategory).ThenInclude(m => m.Store));
            if (product == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundProduct
                };
            }
            if(product.ProductCategory.Store.StoreType ==  (int)StoreTypeEnum.Service)
            {
                if (req.Duration <= 0) throw new BadHttpRequestException("Invalid Duration", HttpStatusCodes.BadRequest);
                if (!(req.Unit.Equals(UnitEnum.Hour.GetDescriptionFromEnum()) || req.Unit.Equals(UnitEnum.Minute.GetDescriptionFromEnum()))) throw new BadHttpRequestException("Invalid Unit, The valid Unit is: Hour, Minute", HttpStatusCodes.BadRequest);
                req.Unit = req.Unit.Trim();
                 product.Duration = req.Duration != null ? req.Duration : product.Duration;
                 product.Unit = req.Unit.Trim() != null ? req.Unit : product.Unit ;
            }

            product.Name = req.Name != null ? req.Name.Trim() : product.Name;
            product.ImageUrl = req.ImageUrl != null ? req.ImageUrl.Trim() : product.ImageUrl;
            product.Price = (int)(req.Price != null ? req.Price : product.Price);
            product.Quantity = req.Quantity;
            product.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Product>().UpdateAsync(product);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateProductSuccess,
                    StatusCode = HttpStatusCodes.OK
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateProductFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }
        public async Task<ResponseAPI> DeleteProduct(Guid ProductId)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync
                (predicate: x => x.Id == ProductId && x.Status == "Active");
            if (product == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundProduct
                };
            }
            product.Status = "InActive";
            product.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Product>().UpdateAsync(product);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeleteProductSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeleteProductFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        public async Task<ResponseAPI<IEnumerable<GetProductResponse>>> SearchAllProduct(Guid MenuId, int page, int size)
        {
            try
            {
                IPaginate<GetProductResponse> data = await _unitOfWork.GetRepository<Product>().GetPagingListAsync(
                    selector: x => new GetProductResponse()
                    {
                        Id = x.Id,
                        MenuId = x.MenuId,
                        ImageUrl = x.ImageUrl,
                        Name = x.Name,
                        CrDate = x.CrDate,
                        UpsDate = x.UpsDate,
                        Status = x.Status,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        ProductCategoryId = x.ProductCategoryId,
                        ProductCategoryName = x.ProductCategory.Name,
                        Unit = x.Unit,
                        Duration = x.Duration,
                    },
                    page: page,
                    size: size,
                    orderBy: x => x.OrderByDescending(z => z.Name),
                    predicate: x => x.MenuId == MenuId && x.Status == "Active",
                    include: z => z.Include(a => a.ProductCategory));
                return new ResponseAPI<IEnumerable<GetProductResponse>>
                {
                    MessageResponse = StoreMessage.GetListProductSuccess,
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
                return new ResponseAPI<IEnumerable<GetProductResponse>>
                {
                    MessageResponse = StoreMessage.GetListProductFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<Product>> SearchProduct(Guid ProductId)
        {
            var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
                predicate: x => x.Id == ProductId && x.Status == "Active",
                include: z => z.Include(a => a.ProductCategory));
            if (product == null)
            {
                return new ResponseAPI<Product>()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundProduct
                };
            }
            return new ResponseAPI<Product>()
            {
                MessageResponse = StoreMessage.GetProductSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = product
            };
        }
        #endregion
        #region CRUD ProductCategory
        public async Task<ResponseAPI> CreateProductCategory(CreateProductCategoryRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var storeUserId = GetUserIdFromJwt();
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.UserStoreMappings.SingleOrDefault().UserId == storeUserId);
            req.Name = req.Name.Trim();
            if (req.Description != null)
            {
                req.Description = req.Description.Trim();
            }
            var newProductCategory = _mapper.Map<ProductCategory>(req);
            newProductCategory.Id = Guid.NewGuid();
            newProductCategory.CrDate = TimeUtils.GetCurrentSEATime();
            newProductCategory.UpsDate = TimeUtils.GetCurrentSEATime();
            newProductCategory.Deflag = false;
            newProductCategory.StoreId = store.Id;
            await _unitOfWork.GetRepository<ProductCategory>().InsertAsync(newProductCategory);
            var walletTypes = await _unitOfWork.GetRepository<WalletType>().GetListAsync(
                predicate: x => x.Name == WalletTypeEnum.SpecificWallet.GetDescriptionFromEnum()
                || x.Name == WalletTypeEnum.ServiceWallet.GetDescriptionFromEnum() && !x.Deflag);
            if (walletTypes.Count <= 0)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = WalletTypeMessage.NotFoundWalletType
                };
            }
            else
            {
                foreach (var walletType in walletTypes)
                {
                    //insert mapping
                    var waletTypeMapping = new WalletTypeMapping()
                    {
                        Id = Guid.NewGuid(),
                        ProductCategoryId = newProductCategory.Id,
                        WalletTypeId = walletType.Id,
                    };
                    await _unitOfWork.GetRepository<WalletTypeMapping>().InsertAsync(waletTypeMapping);
                }
            }
            await _unitOfWork.CommitAsync();
            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.CreateProductCategorySuccess,
                StatusCode = HttpStatusCodes.Created
            };
        }
        public async Task<ResponseAPI> UpdateProductCategory(Guid ProductCategoryId, UpdateProductCategoryRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var productCategory = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync
                (predicate: x => x.Id == ProductCategoryId && !x.Deflag);
            if (productCategory == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundProductCategory
                };
            }
            productCategory.Name = req.Name != null ? req.Name.Trim() : productCategory.Name;
            productCategory.Description = req.Description != null ? req.Description.Trim() : productCategory.Description;
            productCategory.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<ProductCategory>().UpdateAsync(productCategory);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateProductCategorySuccess,
                    StatusCode = HttpStatusCodes.OK
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.UpdateProductCategoryFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }
        public async Task<ResponseAPI> DeleteProductCategory(Guid ProductCategoryId)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var productCategory = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync
                (predicate: x => x.Id == ProductCategoryId && !x.Deflag, include: z => z.Include(a => a.WalletTypeMappings));
            if (productCategory == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundProductCategory
                };
            }
            productCategory.Deflag = true;
            productCategory.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<ProductCategory>().UpdateAsync(productCategory);
            _unitOfWork.GetRepository<WalletTypeMapping>().DeleteRangeAsync(productCategory.WalletTypeMappings);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeleteProductCategorySuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = StoreMessage.DeleteProductCategoryFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        public async Task<ResponseAPI<IEnumerable<GetProductCategoryResponse>>> SearchAllProductCategory(Guid StoreId, int page, int size)
        {
            try
            {
                var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                    predicate: x => x.Id == StoreId && !x.Deflag, include: z => z.Include(z => z.Menus))
                    ?? throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);

                IPaginate<GetProductCategoryResponse> data = await _unitOfWork.GetRepository<ProductCategory>().GetPagingListAsync(
                selector: x => new GetProductCategoryResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    Description = x.Description,
                    StoreId = (Guid)x.StoreId
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag && x.StoreId == store.Id,
                include: z => z.Include(a => a.WalletTypeMappings));
                return new ResponseAPI<IEnumerable<GetProductCategoryResponse>>
                {
                    MessageResponse = StoreMessage.GetListProductCategorySuccess,
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
                return new ResponseAPI<IEnumerable<GetProductCategoryResponse>>
                {
                    MessageResponse = StoreMessage.GetListProductCategoryFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<ProductCategory>> SearchProductCategory(Guid ProductCategoryId)
        {
            var productCategory = await _unitOfWork.GetRepository<ProductCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == ProductCategoryId && !x.Deflag, include: z => z.Include(a => a.Products));
            if (productCategory == null)
            {
                return new ResponseAPI<ProductCategory>()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = StoreMessage.NotFoundProductCategory
                };
            }
            return new ResponseAPI<ProductCategory>()
            {
                MessageResponse = StoreMessage.GetProductCategorySuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = productCategory
            };
        }
        #endregion
        public async Task<ResponseAPI> FinalSettlement(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId
                            && !x.Deflag
                            && x.Status == (int)StoreStatusEnum.Blocked,
                include: z => z.Include(z => z.Zone).ThenInclude(z => z.MarketZone).ThenInclude(a => a.MarketZoneConfig))
                ?? throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);
            //find order list store
            var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(
                predicate: x => x.StoreId == store.Id && x.Status == OrderStatus.Completed &&
                x.UpsDate <= TimeUtils.GetCurrentSEATime() && (x.SaleType == SaleType.Product || x.SaleType == SaleType.Service), include: z => z.Include(z => z.Payments));
            if (orders.Count <= 0) throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            List<Payment> payments = new List<Payment>(); //list payment QRCode
            foreach (var order in orders)
            {
                foreach (var payment in order.Payments)
                {
                    if (payment.Name == PaymentTypeEnum.QRCode.GetDescriptionFromEnum())
                    {
                        payments.Add(payment);
                    }
                }
            }
            int AmountPayment = 0;
            foreach (var payment in payments)
            {
                AmountPayment += payment.FinalAmount;
            }
            int AmountTransfered = (int)(AmountPayment - AmountPayment * store.Zone.MarketZone.MarketZoneConfig.StoreStranferRate); // amount transfered to store
            //find list orders store
            var storeMoneyTransfers = await _unitOfWork.GetRepository<StoreMoneyTransfer>().GetListAsync(
                predicate: x => x.StoreId == store.Id && x.Status == OrderStatus.Completed &&
                x.UpsDate <= TimeUtils.GetCurrentSEATime());
            if (storeMoneyTransfers.Count <= 0) throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            var transactionStoreWithdraws = (List<Transaction>)await _unitOfWork.GetRepository<Transaction>().GetListAsync(
                predicate: x => x.StoreId == store.Id && x.Status == TransactionStatus.Success &&
                x.Type == TransactionType.WithdrawMoney);
            List<StoreMoneyTransfer> storeMoneyTransfersListToStore = new List<StoreMoneyTransfer>();

            foreach (var storeTransfer in storeMoneyTransfers)
            {
                if (storeTransfer.Description.Split(" to ")[1].Trim() == "store")
                {
                    storeMoneyTransfersListToStore.Add(storeTransfer);
                }
            }
            int amoutWithdrawed = 0;
            foreach (var transaction in transactionStoreWithdraws)
            {
                amoutWithdrawed += transaction.Amount; //amount withdrawed from store
            }
            int amount = 0;
            foreach (var storeTransfer in storeMoneyTransfersListToStore)
            {
                amount += storeTransfer.Amount;
            }
            if (AmountTransfered != amount)
            {
                throw new BadHttpRequestException(OrderMessage.AmountNotEqual, HttpStatusCodes.BadRequest);
            }
            int amountFinal = AmountTransfered - amoutWithdrawed;
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = StoreMessage.FinalSettlementSuccess,
                Data = new { amountCanWithdraw = amountFinal }
            };
        }
    }
}

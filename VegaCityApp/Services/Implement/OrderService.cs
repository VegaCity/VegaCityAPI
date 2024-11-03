using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class OrderService : BaseService<OrderService>, IOrderService
    {

        public OrderService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<OrderService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            
        }


        public async Task<ResponseAPI> CreateOrder(CreateOrderRequest req)
        {
            if(PaymentTypeHelper.allowedPaymentTypes.Contains(req.PaymentType) == false)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = OrderMessage.PaymentTypeInvalid,
                };
            }
            if (SaleTypeHelper.allowedSaleType.Contains(req.SaleType) == false)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = OrderMessage.SaleTypeInvalid
                };
            }
            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == req.StoreId && !x.Deflag && x.Status ==(int) StoreStatusEnum.Opened);
            if(!ValidationUtils.CheckNumber(req.TotalAmount))
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.TotalAmountInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
    
            int amount = 0;
            int count = 0;
            foreach (var item in req.ProductData) {
                if(item.Quantity <= 0)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = OrderMessage.QuantityInvalid,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
                amount += item.Price * item.Quantity;
                count += item.Quantity;
            }
            string json = JsonConvert.SerializeObject(req.ProductData);
            //add user ID for Store Type
            Guid userID = GetUserIdFromJwt();
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = req.PaymentType,
                StoreId = store.Id,
                Name = req.OrderName,
                TotalAmount = (int)(req.TotalAmount * (1 + EnvironmentVariableConstant.VATRate)),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = req.InvoiceId,
                SaleType = req.SaleType,
                UserId = userID,
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            //create order Detail
            if (store != null)
            {
                var orderDetail = new OrderDetail()
                {
                    Id = Guid.NewGuid(),
                    OrderId = newOrder.Id,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Quantity = count,
                };
                await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }

            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = OrderMessage.CreateOrderSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    OrderId = newOrder.Id,
                    invoiceId = newOrder.InvoiceId
                }
            } : new ResponseAPI()
            {
                MessageResponse = OrderMessage.CreateOrderFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> DeleteOrder(Guid OrderId)
        {
            var orderExisted = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x => x.Id == OrderId && x.Status == OrderStatus.Pending);
            if (orderExisted == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            orderExisted.Status = OrderStatus.Canceled;
            orderExisted.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(orderExisted);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.CancelOrderSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.CancelFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
        public async Task<ResponseAPI<IEnumerable<GetOrderResponse>>> SearchAllOrders(int size, int page)
        {
            try
            {
                IPaginate<GetOrderResponse> data = await _unitOfWork.GetRepository<Order>().GetPagingListAsync(
                selector: x => new GetOrderResponse()
                {
                    Id = x.Id,
                    PaymentType = x.PaymentType,
                    Name = x.Name,
                    TotalAmount = x.TotalAmount,
                    CrDate = x.CrDate,
                    Status = x.Status,
                    InvoiceId = x.InvoiceId,
                    StoreId = x.StoreId,
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name));
                return new ResponseAPI<IEnumerable<GetOrderResponse>>
                {
                    MessageResponse = OrderMessage.GetOrdersSuccessfully,
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
                return new ResponseAPI<IEnumerable<GetOrderResponse>>
                {
                    MessageResponse = OrderMessage.GetOrdersFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData=null
                };
            }
        }
        public async Task<ResponseAPI> UpdateOrder(string InvoiceId, UpdateOrderRequest  req)
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.InvoiceId == InvoiceId && x.Status == OrderStatus.Pending);
            if (order == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            if(!ValidationUtils.CheckNumber(req.TotalAmount))
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.TotalAmountInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }

            if (order.Status == OrderStatus.Completed)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.OrderCompleted,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            foreach (var item in req.NewProducts)
            {
                if (item.Quantity <= 0)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = OrderMessage.QuantityInvalid,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
            }
            order.TotalAmount = req.TotalAmount;
            order.PaymentType = req.PaymentType?? order.PaymentType;

            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            //update order detail
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().SingleOrDefaultAsync(predicate: x => x.OrderId == order.Id);
            if (orderDetail == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrderDetail,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
  
            orderDetail.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetail);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.UpdateOrderSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        OrderId = order.Id,
                        invoiceId = order.InvoiceId
                    }
                }
                : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.UpdateOrderFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };

        }
        public async Task<ResponseAPI> SearchOrder(Guid? OrderId, string? InvoiceId)
        {
            var orderExist = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => (x.Id == OrderId || x.InvoiceId == InvoiceId)&& x.Status != OrderStatus.Canceled,
                include: order => order
                    .Include(o => o.Store)
                    .Include(o => o.Deposits)
                    .Include(z => z.OrderDetails));
            string json = "";
            string? customerInfo = "";

            List<OrderProductFromPosRequest>? productJson = JsonConvert.DeserializeObject<List<OrderProductFromPosRequest>>(json);
            if(customerInfo == null) customerInfo = "";
            CustomerInfo? customer = JsonConvert.DeserializeObject<CustomerInfo>(customerInfo);
            if (orderExist == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = OrderMessage.GetOrdersSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                Data = new { orderExist, productJson, customer }
            };
        }
        public async Task<ResponseAPI> CreateOrderForCashier(CreateOrderForCashierRequest req)
        {
            if (!ValidationUtils.CheckNumber(req.TotalAmount))
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.TotalAmountInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            int amount = 0;
            int count = 0;
            foreach (var item in req.ProductData)
            {
                if (item.Quantity <= 0)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = OrderMessage.QuantityInvalid,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
                amount += item.Price * item.Quantity;
                count += item.Quantity;
            }
            if (PaymentTypeHelper.allowedPaymentTypes.Contains(req.PaymentType) == false)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = OrderMessage.PaymentTypeInvalid,
                };
            }
            if (SaleTypeHelper.allowedSaleType.Contains(req.SaleType) == false)
            {
                return new ResponseAPI()
                { 
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = OrderMessage.SaleTypeInvalid
                };
            }
            string customerInfo = JsonConvert.SerializeObject(req.CustomerInfo);
            string json = JsonConvert.SerializeObject(req.ProductData);
            Guid userId = GetUserIdFromJwt();
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = req.PaymentType,
                Name = "Order Selling at Vega: " + TimeUtils.GetCurrentSEATime(),
                TotalAmount = (int)(req.TotalAmount * (1 + EnvironmentVariableConstant.VATRate)),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                SaleType = req.SaleType,
                UserId = userId,
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            //create order Detail
            var orderDetail = new OrderDetail() //add userId here
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Quantity = count,
            };
            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);

            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = OrderMessage.CreateOrderSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    OrderId = newOrder.Id,
                    invoiceId = newOrder.InvoiceId
                }
            } : new ResponseAPI()
            {
                MessageResponse = OrderMessage.CreateOrderFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> ConfirmOrderForCashier(ConfirmOrderForCashierRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(z => z.OrderDetails).Include(a => a.User).ThenInclude(b => b.Wallets));
            if (order == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            string etagTypeName = "";
            string packageName = "";
            string json = "";

            List<OrderProductFromCashierRequest> productJson = new List<OrderProductFromCashierRequest>();
            if (json != "") productJson = JsonConvert.DeserializeObject<List<OrderProductFromCashierRequest>>(json);
            int count = 0;
            foreach (var item in productJson)
            {
                if (order.SaleType == SaleType.EtagType)
                {
                    etagTypeName = item.Name;
                    count = item.Quantity;
                }
                else
                {
                    packageName = item.Name;
                    count = item.Quantity;
                }
            }
            int quantityEtagType = 0;
            // fix lại cái response khi nhập saleType
            //generate etag
            if (order.SaleType == SaleType.Package)
            {
                var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(
                    predicate: x => x.Name == packageName && !x.Deflag);
                if (package == null)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = OrderMessage.NotFoundPackage,
                        StatusCode = HttpStatusCodes.NotFound
                    };
                }


                order.Status = OrderStatus.Completed;
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        OrderId = order.Id,
                        invoiceId = order.InvoiceId,
                    }
                } : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.ConfirmOrderFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            else if (order.SaleType == SaleType.EtagType)
            {

                order.Status = OrderStatus.Completed;
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        OrderId = order.Id,
                        invoiceId = order.InvoiceId,
                    }
                } : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.ConfirmOrderFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            else
            {
                if (PaymentTypeHelper.allowedPaymentTypes.Contains(order.PaymentType) && PaymentTypeHelper.allowedPaymentTypes.Contains(order.PaymentType) && order.SaleType == SaleType.PackageItemCharge)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                    //update wallet admin
                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(
                        predicate: x => x.Id == order.User.MarketZoneId);//..
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                        predicate: x => x.Email == marketZone.Email, include: wallet => wallet.Include(z => z.Wallets));//..
                    foreach (var item in admin.Wallets)
                    {
                        item.BalanceHistory -= Int32.Parse(order.TotalAmount.ToString());
                        item.UpsDate = TimeUtils.GetCurrentSEATime();
                    }
                    foreach (var item in order.User.Wallets)
                    {
                        item.UpsDate = TimeUtils.GetCurrentSEATime();
                        item.Balance += Int32.Parse(order.TotalAmount.ToString());
                    }
                    _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
                    _unitOfWork.GetRepository<Wallet>().UpdateRange(order.User.Wallets);
                    //..
                    //update wallet

                    //transacitons cash here admin + cashier 
                    var newAdminTransaction = new VegaCityApp.Domain.Models.Transaction
                    {
                        Id = Guid.NewGuid(),
                        Amount = Int32.Parse(order.TotalAmount.ToString()),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        Description = "Withdraw balanceHistory from admin: " + admin.FullName,
                        IsIncrease = false,
                        Status = TransactionStatus.Success,
                        Type = TransactionType.WithdrawMoney,
                        WalletId = admin.Wallets.SingleOrDefault().Id,
                    };
                    //transaction cashier web
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction
                    {
                        Id = Guid.NewGuid(),
                        Type = TransactionType.ChargeMoney,
                        WalletId = order.User.Wallets.SingleOrDefault().Id,
                        Amount = Int32.Parse(order.TotalAmount.ToString()),
                        IsIncrease = true,
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Status = TransactionStatus.Success,
                        Description = "Add balance to cashier web: " + order.User.FullName,
                    };
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(newAdminTransaction);
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);
                    //create deposite
                    var newDeposit = new Deposit
                    {
                        Id = Guid.NewGuid(), // Tạo ID mới
                        PaymentType = "Cash",
                        Name = "Nạp tiền vào ETag với số tiền: " + order.TotalAmount,
                        IsIncrease = true, // Xác định rằng đây là nạp tiền
                        Amount = Int32.Parse(order.TotalAmount.ToString()),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
    
                        OrderId = order.Id,
                    };
                    await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
                    return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI() {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                        Data = new
                        {
                            OrderId = order.Id,
                            invoiceId = order.InvoiceId,
                        }
                    } : new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = OrderMessage.ConfirmOrderFail,
                    };
                }
            }
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = OrderMessage.ConfirmOrderFail,
            };
        }
    }
}

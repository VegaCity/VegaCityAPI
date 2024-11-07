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
            if (SaleTypeHelper.allowedSaleType.Contains(req.SaleType) == false)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = OrderMessage.SaleTypeInvalid
                };
            }
            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == req.StoreId && !x.Deflag && x.Status ==(int) StoreStatusEnum.Opened)
                ?? throw new BadHttpRequestException("Store not found", HttpStatusCodes.NotFound);
            if (!ValidationUtils.CheckNumber(req.TotalAmount))
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.TotalAmountInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var packageItemExsit = await _unitOfWork.GetRepository<PackageItem>().SingleOrDefaultAsync
                (predicate: x => x.Id == req.PackageItemId, include: z => z.Include(x => x.Wallet))
                ?? throw new BadHttpRequestException("Package item not found", HttpStatusCodes.NotFound);
            if(packageItemExsit.Wallet.Balance < req.TotalAmount) throw new BadHttpRequestException("Balance not enough", HttpStatusCodes.BadRequest);
            //add user ID for Store Type
            Guid userID = GetUserIdFromJwt();
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = PaymentTypeEnum.QRCode.GetDescriptionFromEnum(),
                StoreId = store.Id,
                Name = req.OrderName ?? "Order Sale " + req.SaleType + " At Vega City: " + TimeUtils.GetCurrentSEATime(),
                TotalAmount = req.TotalAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = req.InvoiceId ?? TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                SaleType = req.SaleType,
                UserId = userID,
                PackageItemId = req.PackageItemId
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            //create order Detail
            List<Product> products = new List<Product>();
            List<Domain.Models.StoreService> storeServices = new List<Domain.Models.StoreService>();
            if (req.SaleType == SaleType.Product)
            {
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
                    products.Add(await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(item.Id))
                        ?? throw new BadHttpRequestException("ProductId: " + item.Id + " is not found", HttpStatusCodes.NotFound));
                    var orderDetail = new OrderDetail()
                    {
                        Id = Guid.NewGuid(),
                        OrderId = newOrder.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Quantity = item.Quantity,
                        Amount = item.Price,
                        FinalAmount = item.Price * item.Quantity,
                        PromotionAmount = 0,
                        Vatamount = (int)(EnvironmentVariableConstant.VATRate * item.Price * item.Quantity),
                        ProductId = Guid.Parse(item.Id)
                    };
                    await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
                }
                var newTransaction = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    Amount = req.TotalAmount,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Description = "Transaction selling " + req.SaleType + " at Vega",
                    IsIncrease = true,
                    Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                    OrderId = newOrder.Id,
                    Type = TransactionType.SellingProduct.GetDescriptionFromEnum(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    UserId = userID,
                    StoreId = store.Id
                };
                await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.CreateOrderSuccessfully,
                    StatusCode = HttpStatusCodes.Created,
                    Data = new
                    {
                        OrderId = newOrder.Id,
                        invoiceId = newOrder.InvoiceId,
                        transactionId = newTransaction.Id
                    }
                } : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.CreateOrderFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            else if (req.SaleType == SaleType.StoreService)
            {
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
                    storeServices.Add(await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.StoreService>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(item.Id))
                        ?? throw new BadHttpRequestException("StoreServiceId: " + item.Id + " is not found", HttpStatusCodes.NotFound));
                    var orderDetail = new OrderDetail()
                    {
                        Id = Guid.NewGuid(),
                        OrderId = newOrder.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Quantity = item.Quantity,
                        Amount = item.Price,
                        FinalAmount = item.Price * item.Quantity,
                        PromotionAmount = 0,
                        Vatamount = (int)(EnvironmentVariableConstant.VATRate * item.Price * item.Quantity),
                        StoreServiceId = Guid.Parse(item.Id)
                    };
                    await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
                }
                var newTransaction = new Transaction()
                {
                    Id = Guid.NewGuid(),
                    Amount = req.TotalAmount,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Description = "Transaction selling " + req.SaleType + " at Vega",
                    IsIncrease = true,
                    Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                    OrderId = newOrder.Id,
                    Type = TransactionType.SellingService.GetDescriptionFromEnum(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    UserId = userID,
                    StoreId = store.Id
                };
                await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.CreateOrderSuccessfully,
                    StatusCode = HttpStatusCodes.Created,
                    Data = new
                    {
                        orderId = newOrder.Id,
                        invoiceId = newOrder.InvoiceId,
                        transactionId = newTransaction.Id
                    }
                } : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.CreateOrderFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            else 
                throw new BadHttpRequestException("Sale type in valid", HttpStatusCodes.NotFound);
        }
        public async Task<ResponseAPI> DeleteOrder(Guid OrderId)
        {
            var orderExisted = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x => x.Id == OrderId && x.Status == OrderStatus.Pending, 
                include: z => z.Include(u => u.PackageOrders));
            if (orderExisted == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if(orderExisted.PackageOrders.Count != 0)
            {
                foreach (var packageOrder in orderExisted.PackageOrders)
                {
                    packageOrder.Status = OrderStatus.Canceled;
                    packageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder);
                }
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
                predicate: z => z.UserId == GetUserIdFromJwt(),
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
            Guid packageId = Guid.NewGuid();
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
                var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(item.Id))
                    ?? throw new BadHttpRequestException("Package not found", HttpStatusCodes.NotFound);
                packageId = package.Id;
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
            
            string json = JsonConvert.SerializeObject(req.ProductData);
            Guid userId = GetUserIdFromJwt();
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = req.PaymentType,
                Name = "Order Selling Package Vega: " + TimeUtils.GetCurrentSEATime(),
                TotalAmount = req.TotalAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                SaleType = req.SaleType,
                UserId = userId,
                PackageId = packageId
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            var newPackageOrder = new PackageOrder()
            {
                Id = Guid.NewGuid(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                CusCccdpassport = req.CustomerInfo.CccdPassport,
                CusEmail = req.CustomerInfo.Email,
                CusName = req.CustomerInfo.FullName,
                OrderId = newOrder.Id,
                PackageId = packageId,
                PhoneNumber = req.CustomerInfo.PhoneNumber
            };
            await _unitOfWork.GetRepository<PackageOrder>().InsertAsync(newPackageOrder);
            //create order Detail
            var orderDetail = new OrderDetail() //add userId here
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Quantity = count,
                Amount = req.TotalAmount,
                FinalAmount = amount,
                PromotionAmount = 0,
                Vatamount = (int)(EnvironmentVariableConstant.VATRate * amount),
            };
            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = req.TotalAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Description = "Transaction selling package at Vega",
                IsIncrease = true,
                Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                OrderId = newOrder.Id,
                Type = TransactionType.SellingPackage.GetDescriptionFromEnum(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                UserId = userId
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transaction);
            return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
            {
                MessageResponse = OrderMessage.CreateOrderSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    orderId = newOrder.Id,
                    invoiceId = newOrder.InvoiceId,
                    packageOrderId = newPackageOrder.Id,
                    transactionId = transaction.Id,
                    balance = req.TotalAmount
                }
            } : new ResponseAPI()
            {
                MessageResponse = OrderMessage.CreateOrderFail,
                StatusCode = HttpStatusCodes.BadRequest
            };
        }
        public async Task<ResponseAPI> ConfirmOrderForCashier(ConfirmOrderForCashierRequest req)
        {
            Guid marketZoneId = GetMarketZoneIdFromJwt();
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.PackageOrders)
                                       .Include(c => c.PackageItem).ThenInclude(r => r.Wallet).Include(p => p.PromotionOrders));
            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //session check

            //
            if (order.SaleType == SaleType.PackageItemCharge)
            {
                if(order.PackageItem == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
            }
            if(order.PaymentType != PaymentTypeEnum.Cash.GetDescriptionFromEnum()) 
                throw new BadHttpRequestException("Payment is not cash", HttpStatusCodes.BadRequest);

            if(order.SaleType == SaleType.PackageItemCharge && order.PaymentType == PaymentTypeEnum.Cash.GetDescriptionFromEnum())
            {
                if(order.PromotionOrders.Count == 1)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //session update
                    sessionUser.TotalQuantityOrder += 1;
                    sessionUser.TotalCashReceive += order.TotalAmount;
                    sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                    _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

                    if (order.PackageOrders.Count != 0)
                    {
                        foreach (var packageOrder in order.PackageOrders)
                        {
                            packageOrder.Status = OrderStatus.Completed;
                            packageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder);
                        }
                    }
                    else throw new BadHttpRequestException("Package order not found", HttpStatusCodes.NotFound);

                    //wallet cashier
                    var wallet = order.User.Wallets.FirstOrDefault();
                    wallet.Balance += order.TotalAmount;
                    wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                    //create deposit
                    var deposit = new Deposit()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        IsIncrease = true,
                        Name = "Deposit from order " + order.InvoiceId,
                        PackageItemId = (Guid)order.PackageItemId,
                        PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum()
                    };
                    await _unitOfWork.GetRepository<Deposit>().InsertAsync(deposit);

                    //update wallet package item
                    order.PackageItem.Wallet.Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount; 
                    order.PackageItem.Wallet.BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageItem.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageItem.Wallet);

                    var transactionCharge = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.Id == Guid.Parse(req.TransactionChargeId))
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    transactionCharge.DespositId = deposit.Id;
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new Transaction()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        Description = "Receive money from order " + order.InvoiceId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.ReceiveMoney,
                        DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = order.User.Wallets.FirstOrDefault().Id,
                        UserId = order.UserId,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == marketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZoneId, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new Transaction()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        Description = "Refund money from order: " + order.InvoiceId + "to PackageItem: " + order.PackageItem.Name,
                        IsIncrease = false,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.RefundMoney,
                        DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = walletAdmin.Id,
                        UserId = admin.Id,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);

                    await _unitOfWork.CommitAsync();
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                    };
                }
                else
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //session update
                    sessionUser.TotalQuantityOrder += 1;
                    sessionUser.TotalCashReceive += order.TotalAmount;
                    sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                    _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

                    if (order.PackageOrders.Count != 0)
                    {
                        foreach (var packageOrder in order.PackageOrders)
                        {
                            packageOrder.Status = OrderStatus.Completed;
                            packageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder);
                        }
                    }
                    else throw new BadHttpRequestException("Package order not found", HttpStatusCodes.NotFound);

                    //wallet cashier
                    var wallet = order.User.Wallets.FirstOrDefault();
                    wallet.Balance += order.TotalAmount;
                    wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                    //create deposit
                    var deposit = new Deposit()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount,
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        IsIncrease = true,
                        Name = "Deposit from order " + order.InvoiceId,
                        PackageItemId = (Guid)order.PackageItemId,
                        PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum()
                    };
                    await _unitOfWork.GetRepository<Deposit>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageItem.Wallet.Balance += order.TotalAmount;
                    order.PackageItem.Wallet.BalanceHistory += order.TotalAmount;
                    order.PackageItem.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageItem.Wallet);

                    var transactionCharge = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.Id == Guid.Parse(req.TransactionChargeId))
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    transactionCharge.DespositId = deposit.Id;
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new Transaction()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        Description = "Receive money from order " + order.InvoiceId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.ReceiveMoney,
                        DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = order.User.Wallets.FirstOrDefault().Id,
                        UserId = order.UserId,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == marketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZoneId, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new Transaction()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        Description = "Refund money from order: " + order.InvoiceId + "to PackageItem: " + order.PackageItem.Name,
                        IsIncrease = false,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.RefundMoney,
                        DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = walletAdmin.Id,
                        UserId = admin.Id,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);

                    await _unitOfWork.CommitAsync();
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                    };
                }
            }
            else if(order.SaleType == SaleType.Package && order.PaymentType == PaymentTypeEnum.Cash.GetDescriptionFromEnum())
            {
                order.Status = OrderStatus.Completed;
                order.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                //session update
                sessionUser.TotalQuantityOrder += 1;
                sessionUser.TotalCashReceive += order.TotalAmount;
                sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

                if (order.PackageOrders.Count != 0)
                {
                    foreach (var packageOrder in order.PackageOrders)
                    {
                        packageOrder.Status = OrderStatus.Completed;
                        packageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder);
                    }
                }
                else throw new BadHttpRequestException("Package order not found", HttpStatusCodes.NotFound);

                //wallet cashier
                var wallet = order.User.Wallets.FirstOrDefault();
                wallet.Balance += order.TotalAmount;
                wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);

                var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                    (predicate: x => x.Id == Guid.Parse(req.TransactionId))
                    ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);

                transaction.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                transaction.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                };
            }
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = OrderMessage.ConfirmOrderFail,
            };
        }
        public async Task<ResponseAPI> ConfirmOrder(ConfirmOrderRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.OrderDetails)
                                       .Include(c => c.Store)
                                       .Include(p => p.PromotionOrders)
                                       .Include(f => f.PackageItem).ThenInclude(r => r.Wallet)
                ) ?? throw new BadHttpRequestException("Order not found", HttpStatusCodes.NotFound);
            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //session check
            //
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            //session update
            sessionUser.TotalQuantityOrder += 1;
            sessionUser.TotalCashReceive += order.TotalAmount;
            sessionUser.TotalFinalAmountOrder += order.TotalAmount;
            _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

            Wallet walletStore = null;
            foreach(var item in order.User.Wallets)
            {
                if(item.StoreId == order.StoreId)
                {
                    walletStore = item;
                    break;
                }
            }
            if (walletStore == null) throw new BadHttpRequestException("Wallet store not found", HttpStatusCodes.NotFound);
                
            //update wallet package item
            var deposit = new Deposit()
            {
                Id = Guid.NewGuid(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Amount = order.TotalAmount,
                WalletId = order.PackageItem.WalletId,
                OrderId = order.Id,
                IsIncrease = false,
                Name = "Deposit from order " + order.InvoiceId,
                PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
                PackageItemId = (Guid)order.PackageItemId
            };
            await _unitOfWork.GetRepository<Deposit>().InsertAsync(deposit);
            order.PackageItem.Wallet.Balance -= order.TotalAmount;
            order.PackageItem.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageItem.Wallet);
            var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                (predicate: x => x.Id == Guid.Parse(req.TransactionId))
                ?? throw new BadHttpRequestException("Transaction sale not found", HttpStatusCodes.NotFound);
            transaction.Status = TransactionStatus.Success.GetDescriptionFromEnum();
            transaction.UpsDate = TimeUtils.GetCurrentSEATime();
            transaction.DespositId = deposit.Id;
            _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);
            //
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId,
                include: z => z.Include(g => g.MarketZoneConfig));
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
            var transactionStoreTransfer = new Transaction
            {
                Id = Guid.NewGuid(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Amount = (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Description = "Transfer money from order " + order.InvoiceId + " to store",
                IsIncrease = true,
                Status = TransactionStatus.Success,
                StoreId = order.StoreId,
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Type = TransactionType.TransferMoney,
                UserId = order.UserId,
                WalletId = walletStore.Id,
                OrderId = order.Id
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionStoreTransfer);
            walletStore.Balance += (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate);
            walletStore.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletStore);
            var transfer = new StoreMoneyTransfer()
            {
                Id = Guid.NewGuid(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Amount = (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                IsIncrease = true,
                MarketZoneId = order.User.MarketZoneId,
                StoreId = (Guid)order.StoreId,
                TransactionId = transactionStoreTransfer.Id,
                Status = OrderStatus.Completed,
                Description = "Transfer money from order " + order.InvoiceId + " to store"
            };
            await _unitOfWork.GetRepository<StoreMoneyTransfer>().InsertAsync(transfer);
            var transactionVega = new Transaction
            {
                Id = Guid.NewGuid(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Amount = (int)(order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                Description = "Transfer money from order " + order.InvoiceId + " to Vega",
                IsIncrease = true,
                Status = TransactionStatus.Success,
                StoreId = order.StoreId,
                UpsDate = TimeUtils.GetCurrentSEATime(),
                OrderId = order.Id,
                Type = TransactionType.TransferMoney,
                UserId = admin.Id,
                WalletId = admin.Wallets.FirstOrDefault().Id
            };
            await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionVega);
            var walletAdmin = admin.Wallets.FirstOrDefault();
            walletAdmin.Balance += (int)(order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate);
            walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
            var transfertoVega = new StoreMoneyTransfer
            {
                Id = Guid.NewGuid(),
                Amount = (int)(order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Description = "Transfer money from order " + order.InvoiceId + " to Vega",
                IsIncrease = true,
                MarketZoneId = order.User.MarketZoneId,
                Status = OrderStatus.Completed,
                StoreId = (Guid)order.StoreId,
                TransactionId = transactionVega.Id,
                UpsDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<StoreMoneyTransfer>().InsertAsync(transfertoVega);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
            };
        }
    }
}

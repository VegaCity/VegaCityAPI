using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Response;
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
        private IUtilService _util;
        public OrderService(IUnitOfWork<VegaCityAppContext> unitOfWork,
                            ILogger<OrderService> logger,
                            IHttpContextAccessor httpContextAccessor,
                            IMapper mapper,
                            IUtilService util) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _util = util;
        }
        public async Task<ResponseAPI> CreateOrder(CreateOrderRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            if (SaleTypeHelper.allowedSaleType.Contains(req.SaleType) == false)
                throw new BadHttpRequestException(OrderMessage.SaleTypeInvalid, HttpStatusCodes.BadRequest);
            if (PaymentTypeHelper.allowedPaymentTypes.Contains(req.paymentType) == false)
                throw new BadHttpRequestException(OrderMessage.PaymentTypeInvalid, HttpStatusCodes.BadRequest);
            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == req.StoreId && !x.Deflag && x.Status == (int)StoreStatusEnum.Opened,
                                      include: z => z.Include(a => a.Wallets))
                ?? throw new BadHttpRequestException("Store is not opened", HttpStatusCodes.NotFound);
            if (req.TotalAmount <= 0)
                throw new BadHttpRequestException(OrderMessage.TotalAmountInvalid, HttpStatusCodes.BadRequest);
            Guid userID = GetUserIdFromJwt();
            Order newOrder;
            Transaction newTransaction;
            Transaction newTransactionForStore;
            Guid parse = Guid.Empty;
            string vcardcode = null;
            //create order Detail
            List<Product> products = new List<Product>();
            if (req.SaleType == SaleType.Product)
            {
                if (req.PackageOrderId != null)
                {
                    //check package order id
                    //check guid
                    bool isValid = Guid.TryParse(req.PackageOrderId, out Guid packageOrderId);
                    if (isValid)
                    {
                        parse = Guid.Parse(req.PackageOrderId);
                        var packageOrderExist = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync
                                        (predicate: x => x.Id == parse
                                                      && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(),
                                         include: a => a.Include(v => v.Wallets)
                                                        .Include(p => p.Package))
                        ?? throw new BadHttpRequestException("Package Order not found", HttpStatusCodes.NotFound);
                        if (packageOrderExist.Wallets.SingleOrDefault().Balance < req.TotalAmount) throw new BadHttpRequestException("Balance not enough", HttpStatusCodes.BadRequest);
                        //add user ID for Store Type
                        newOrder = new Order()
                        {
                            Id = Guid.NewGuid(),
                            StoreId = store.Id,
                            Name = "Order Sale " + req.SaleType + " At Vega City: " + TimeUtils.GetCurrentSEATime(),
                            TotalAmount = req.TotalAmount,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Status = OrderStatus.Pending,
                            InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                            SaleType = req.SaleType,
                            UserId = userID,
                            PackageOrderId = packageOrderExist.Id,
                            PackageId = packageOrderExist.PackageId,
                            StartRent = req.StartRent,
                            EndRent = req.EndRent,
                            BalanceBeforePayment = store.Wallets.SingleOrDefault().Balance,
                            BalanceHistoryBeforePayment = store.Wallets.SingleOrDefault().BalanceHistory
                        };
                        await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                        foreach (var item in req.ProductData)
                        {
                            if (item.Quantity <= 0)
                                throw new BadHttpRequestException(OrderMessage.QuantityInvalid, HttpStatusCodes.BadRequest);
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
                        //payment
                        var newPayment = new Payment()
                        {
                            Id = Guid.NewGuid(),
                            FinalAmount = req.TotalAmount,
                            Name = req.paymentType,
                            OrderId = newOrder.Id,
                            Status = PaymentStatus.Pending,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime()
                        };
                        await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                        //transaction for package order
                        newTransaction = new Transaction()
                        {
                            Id = Guid.NewGuid(),
                            Amount = req.TotalAmount,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                            Description = "Transaction selling " + req.SaleType + "with InvoiceId: " + newOrder.InvoiceId + " at Vega",
                            IsIncrease = false,
                            Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                            OrderId = newOrder.Id,
                            Type = TransactionType.SellingProduct.GetDescriptionFromEnum(),
                            WalletId = packageOrderExist.Wallets.SingleOrDefault().Id,
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            UserId = userID,
                            StoreId = store.Id,
                            PaymentId = newPayment.Id
                        };
                        await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
                        newTransactionForStore = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            Amount = req.TotalAmount,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                            Description = "Transaction selling " + req.SaleType + "with InvoiceId: " + newOrder.InvoiceId + " at Vega",
                            IsIncrease = true,
                            Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                            OrderId = newOrder.Id,
                            Type = TransactionType.SellingProduct.GetDescriptionFromEnum(),
                            WalletId = store.Wallets.SingleOrDefault().Id,
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            UserId = userID,
                            StoreId = store.Id,
                            PaymentId = newPayment.Id
                        };
                        await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransactionForStore); //update transaction for store
                        return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                        {
                            MessageResponse = OrderMessage.CreateOrderSuccessfully,
                            StatusCode = HttpStatusCodes.Created,
                            Data = new
                            {
                                OrderId = newOrder.Id,
                                invoiceId = newOrder.InvoiceId,
                                transactionId = newTransaction.Id,
                                newTransactionForStoreId = newTransactionForStore.Id
                            }
                        } : new ResponseAPI()
                        {
                            MessageResponse = OrderMessage.CreateOrderFail,
                            StatusCode = HttpStatusCodes.BadRequest
                        };
                    }
                    else
                    {
                        vcardcode = req.PackageOrderId;
                        var packageOrderExist = await _unitOfWork.GetRepository<PackageOrder>().SingleOrDefaultAsync
                                        (predicate: x => x.VcardId == vcardcode
                                                      && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(),
                                         include: a => a.Include(v => v.Wallets)
                                                        .Include(p => p.Package))
                        ?? throw new BadHttpRequestException("Package Order not found", HttpStatusCodes.NotFound);
                        if (packageOrderExist.Wallets.SingleOrDefault().Balance < req.TotalAmount) throw new BadHttpRequestException("Balance not enough", HttpStatusCodes.BadRequest);
                        //add user ID for Store Type
                        newOrder = new Order()
                        {
                            Id = Guid.NewGuid(),
                            StoreId = store.Id,
                            Name = "Order Sale " + req.SaleType + " At Vega City: " + TimeUtils.GetCurrentSEATime(),
                            TotalAmount = req.TotalAmount,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            Status = OrderStatus.Pending,
                            InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                            SaleType = req.SaleType,
                            UserId = userID,
                            PackageOrderId = packageOrderExist.Id,
                            PackageId = packageOrderExist.PackageId,
                            StartRent = req.StartRent,
                            EndRent = req.EndRent,
                            BalanceBeforePayment = store.Wallets.SingleOrDefault().Balance,
                            BalanceHistoryBeforePayment = store.Wallets.SingleOrDefault().BalanceHistory
                        };
                        await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                        foreach (var item in req.ProductData)
                        {
                            if (item.Quantity <= 0)
                                throw new BadHttpRequestException(OrderMessage.QuantityInvalid, HttpStatusCodes.BadRequest);
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
                        //payment
                        var newPayment = new Payment()
                        {
                            Id = Guid.NewGuid(),
                            FinalAmount = req.TotalAmount,
                            Name = req.paymentType,
                            OrderId = newOrder.Id,
                            Status = PaymentStatus.Pending,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            UpsDate = TimeUtils.GetCurrentSEATime()
                        };
                        await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                        //transaction for package order
                        newTransaction = new Transaction()
                        {
                            Id = Guid.NewGuid(),
                            Amount = req.TotalAmount,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                            Description = "Transaction selling " + req.SaleType + "with InvoiceId: " + newOrder.InvoiceId + " at Vega",
                            IsIncrease = false,
                            Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                            OrderId = newOrder.Id,
                            Type = TransactionType.SellingProduct.GetDescriptionFromEnum(),
                            WalletId = packageOrderExist.Wallets.SingleOrDefault().Id,
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            UserId = userID,
                            StoreId = store.Id,
                            PaymentId = newPayment.Id
                        };
                        await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
                        newTransactionForStore = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            Amount = req.TotalAmount,
                            CrDate = TimeUtils.GetCurrentSEATime(),
                            Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                            Description = "Transaction selling " + req.SaleType + "with InvoiceId: " + newOrder.InvoiceId + " at Vega",
                            IsIncrease = true,
                            Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                            OrderId = newOrder.Id,
                            Type = TransactionType.SellingProduct.GetDescriptionFromEnum(),
                            WalletId = store.Wallets.SingleOrDefault().Id,
                            UpsDate = TimeUtils.GetCurrentSEATime(),
                            UserId = userID,
                            StoreId = store.Id,
                            PaymentId = newPayment.Id
                        };
                        await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransactionForStore); //update transaction for store
                        return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                        {
                            MessageResponse = OrderMessage.CreateOrderSuccessfully,
                            StatusCode = HttpStatusCodes.Created,
                            Data = new
                            {
                                OrderId = newOrder.Id,
                                invoiceId = newOrder.InvoiceId,
                                transactionId = newTransaction.Id,
                                newTransactionForStoreId = newTransactionForStore.Id
                            }
                        } : new ResponseAPI()
                        {
                            MessageResponse = OrderMessage.CreateOrderFail,
                            StatusCode = HttpStatusCodes.BadRequest
                        };
                    }

                }
                else
                {
                    newOrder = new Order()
                    {
                        Id = Guid.NewGuid(),
                        StoreId = store.Id,
                        Name = "Order Sale " + req.SaleType + " At Vega City: " + TimeUtils.GetCurrentSEATime(),
                        TotalAmount = req.TotalAmount,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Status = OrderStatus.Pending,
                        InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                        SaleType = req.SaleType,
                        StartRent = req.StartRent,
                        EndRent = req.EndRent,
                        UserId = userID,
                        BalanceBeforePayment = store.Wallets.SingleOrDefault().Balance,
                        BalanceHistoryBeforePayment = store.Wallets.SingleOrDefault().BalanceHistory
                    };
                    await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                    foreach (var item in req.ProductData)
                    {
                        if (item.Quantity <= 0)
                            throw new BadHttpRequestException(OrderMessage.QuantityInvalid, HttpStatusCodes.BadRequest);
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
                    //payment
                    var newPayment = new Payment()
                    {
                        Id = Guid.NewGuid(),
                        FinalAmount = req.TotalAmount,
                        Name = req.paymentType,
                        OrderId = newOrder.Id,
                        Status = PaymentStatus.Pending,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
                    //newTransaction = new Transaction()
                    //{
                    //    Id = Guid.NewGuid(),
                    //    Amount = req.TotalAmount,
                    //    CrDate = TimeUtils.GetCurrentSEATime(),
                    //    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    //    Description = "Transaction selling " + req.SaleType + " at Vega",
                    //    IsIncrease = true,
                    //    Status = TransactionStatus.Pending.GetDescriptionFromEnum(),
                    //    OrderId = newOrder.Id,
                    //    Type = TransactionType.SellingProduct.GetDescriptionFromEnum(),
                    //    WalletId = null,
                    //    UpsDate = TimeUtils.GetCurrentSEATime(),
                    //    UserId = userID,
                    //    StoreId = store.Id,
                    //    PaymentId = newPayment.Id
                    //};
                    //await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
                    newTransactionForStore = new Transaction
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
                        WalletId = store.Wallets.SingleOrDefault().Id,
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        UserId = userID,
                        StoreId = store.Id,
                        PaymentId = newPayment.Id
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransactionForStore); //update transaction for store with case no package order
                    return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                    {
                        MessageResponse = OrderMessage.CreateOrderSuccessfully,
                        StatusCode = HttpStatusCodes.Created,
                        Data = new
                        {
                            OrderId = newOrder.Id,
                            invoiceId = newOrder.InvoiceId,
                            transactionId = newTransactionForStore.Id,
                            //newTransactionForStoreId = newTransactionForStore.Id
                        }
                    } : new ResponseAPI()
                    {
                        MessageResponse = OrderMessage.CreateOrderFail,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
            }
            else
                throw new BadHttpRequestException("Sale type in valid", HttpStatusCodes.NotFound);
        }
        public async Task<ResponseAPI> DeleteOrder(Guid OrderId)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var orderExisted = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x => x.Id == OrderId && x.Status == OrderStatus.Pending,
                include: z => z.Include(a => a.Transactions).Include(p => p.PromotionOrders).Include(z => z.Payments))
                ?? throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            if (orderExisted.PromotionOrders.Count() != 0)
            {
                foreach (var promotionOrder in orderExisted.PromotionOrders)
                {
                    promotionOrder.Deflag = true;
                    promotionOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<PromotionOrder>().UpdateAsync(promotionOrder);
                }
            }
            if (orderExisted.Payments.Count() != 0)
            {
                foreach (var payment in orderExisted.Payments)
                {
                    payment.Status = PaymentStatus.Canceled;
                    payment.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(payment);
                }
            }
            if (orderExisted.Transactions.Count() != 0)
            {
                foreach (var transaction in orderExisted.Transactions)
                {
                    transaction.Status = TransactionStatus.Fail.GetDescriptionFromEnum();
                    transaction.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);
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
        public async Task<ResponseAPI<IEnumerable<GetOrderResponse>>> SearchAllOrders(int size, int page, string status)
        {
            try
            {
                Expression<Func<Order, bool>> predicate;
                if (status == "ALL")
                {
                    predicate = z => z.UserId == GetUserIdFromJwt();
                }
                else
                {
                    predicate = z => z.UserId == GetUserIdFromJwt() && z.Status.ToUpper() == status.ToUpper();
                }
                IPaginate<GetOrderResponse> data = await _unitOfWork.GetRepository<Order>().GetPagingListAsync(
                selector: x => new GetOrderResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    TotalAmount = x.TotalAmount,
                    CrDate = x.CrDate,
                    Status = x.Status,
                    InvoiceId = x.InvoiceId,
                    StoreId = x.StoreId,
                    PackageId = x.PackageId,
                    UserId = x.UserId,
                    PaymentType = x.Payments.SingleOrDefault().Name
                },
                predicate: predicate,
                include: x => x.Include(p => p.Payments),
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
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> UpdateOrder(string InvoiceId, UpdateOrderRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            var order = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.InvoiceId == InvoiceId && x.Status == OrderStatus.Pending,
                                      include: p => p.Include(d => d.OrderDetails).Include(a => a.Payments))
                ?? throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            if (req.TotalAmount <= 0)
                throw new BadHttpRequestException(OrderMessage.TotalAmountInvalid, HttpStatusCodes.BadRequest);

            if (order.Status == OrderStatus.Completed)
                throw new BadHttpRequestException(OrderMessage.OrderCompleted, HttpStatusCodes.BadRequest);
            foreach (var item in req.NewProducts)
            {
                if (item.Quantity <= 0)
                    throw new BadHttpRequestException(OrderMessage.QuantityInvalid, HttpStatusCodes.BadRequest);
                _unitOfWork.GetRepository<OrderDetail>().DeleteRangeAsync(order.OrderDetails);
                var newOrderDetail = new OrderDetail()
                {
                    Id = Guid.NewGuid(),
                    Amount = item.Price,
                    Quantity = item.Quantity,
                    FinalAmount = item.Price * item.Quantity,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Vatamount = (int)(EnvironmentVariableConstant.VATRate * (item.Price * item.Quantity)),
                    OrderId = order.Id,
                    ProductId = Guid.Parse(item.Id),
                    PromotionAmount = 0
                };
                await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(newOrderDetail);
            }
            order.TotalAmount = req.TotalAmount;
            order.Payments.SingleOrDefault().Name = req.PaymentType ?? order.Payments.SingleOrDefault().Name;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());

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
            foreach (var item in order.OrderDetails)
            {
                //orderDetail.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<OrderDetail>().DeleteAsync(item);
            }
            foreach (var product in req.NewProducts)
            {
                var newOrderDetail = new OrderDetail()
                {
                    Id = Guid.NewGuid(),
                    Amount = product.Price,
                    Quantity = product.Quantity,
                    FinalAmount = product.Price * product.Quantity,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Vatamount = (int)(EnvironmentVariableConstant.VATRate * (product.Price * product.Quantity)),
                    OrderId = order.Id,
                    ProductId = Guid.Parse(product.Id),
                    PromotionAmount = 0,
                };
                await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(newOrderDetail);

            }
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
                predicate: x => (x.Id == OrderId || x.InvoiceId == InvoiceId),
                include: order => order
                    .Include(a => a.Payments)
                    .Include(u => u.User)
                    .Include(o => o.Store)
                    .Include(z => z.OrderDetails)
                    .Include(p => p.PackageOrder)
                    .Include(h => h.PromotionOrders)
                    .Include(a => a.Customer)) ?? throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            string json = "";
            //string? customerInfo = "";
            //command here //

            //here//

            var seller = orderExist.User.FullName;
            List<OrderProductFromPosRequest>? productJson = JsonConvert.DeserializeObject<List<OrderProductFromPosRequest>>(json);

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
                Data = new { orderExist, productJson, seller }
            };
        }
        public async Task<ResponseAPI> CreateOrderForCashier(CreateOrderForCashierRequest req)
        {
            await _util.CheckUserSession(GetUserIdFromJwt());
            if (SaleTypeHelper.allowedSaleType.Contains(req.SaleType) == false)
                throw new BadHttpRequestException(OrderMessage.SaleTypeInvalid, HttpStatusCodes.BadRequest);
            if (PaymentTypeHelper.allowedPaymentTypes.Contains(req.PaymentType) == false)
                throw new BadHttpRequestException(OrderMessage.PaymentTypeInvalid, HttpStatusCodes.BadRequest);
            if (req.TotalAmount <= 0)
                throw new BadHttpRequestException(OrderMessage.TotalAmountInvalid, HttpStatusCodes.BadRequest);
            int amount = 0;
            int count = 0;
            Guid packageId = Guid.NewGuid();
            foreach (var item in req.ProductData)
            {
                if (item.Quantity <= 0)
                    throw new BadHttpRequestException(OrderMessage.QuantityInvalid, HttpStatusCodes.BadRequest);
                var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(item.Id))
                    ?? throw new BadHttpRequestException("Package not found", HttpStatusCodes.NotFound);
                packageId = package.Id;
                amount = item.Price;
                count += item.Quantity;
            }
            Guid userId = GetUserIdFromJwt();
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: z => z.Id == userId, include: z => z.Include(s => s.Wallets));
            var customer = await _unitOfWork.GetRepository<Customer>()
                .SingleOrDefaultAsync(predicate: x => x.Email == req.CustomerInfo.Email
                                                   && x.Cccdpassport == req.CustomerInfo.CccdPassport
                                                   && x.FullName == req.CustomerInfo.FullName
                                                   && x.PhoneNumber == req.CustomerInfo.PhoneNumber);
            if (customer == null)
            {
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Cccdpassport = req.CustomerInfo.CccdPassport,
                    Email = req.CustomerInfo.Email,
                    FullName = req.CustomerInfo.FullName,
                    PhoneNumber = req.CustomerInfo.PhoneNumber
                };
                await _unitOfWork.GetRepository<Customer>().InsertAsync(customer);
            }
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                Name = "Order Selling Package Vega: " + TimeUtils.GetCurrentSEATime(),
                TotalAmount = req.TotalAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()),
                SaleType = req.SaleType,
                UserId = userId,
                PackageId = packageId,
                CustomerId = customer.Id,
                BalanceBeforePayment = user.Wallets.SingleOrDefault().Balance,
                BalanceHistoryBeforePayment = user.Wallets.SingleOrDefault().BalanceHistory
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            var newOrderDetail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Quantity = count,
                Amount = amount,
                PackageId = packageId,
                FinalAmount = req.TotalAmount,
                PromotionAmount = 0,
                Vatamount = (int)(EnvironmentVariableConstant.VATRate * amount)
            };
            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(newOrderDetail);
            //payment
            var newPayment = new Payment()
            {
                Id = Guid.NewGuid(),
                FinalAmount = req.TotalAmount,
                Name = req.PaymentType,
                OrderId = newOrder.Id,
                Status = PaymentStatus.Pending,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
            //transaction for cashierWeb
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = req.TotalAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                Description = "Transaction selling package at Vega",
                IsIncrease = true,
                OrderId = newOrder.Id,
                Type = TransactionType.SellingPackage.GetDescriptionFromEnum(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                UserId = userId,
                PaymentId = newPayment.Id,
                Status = TransactionStatus.Pending.GetDescriptionFromEnum()
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
            var sessionUser = await _util.CheckUserSession(GetUserIdFromJwt());
            Guid marketZoneId = GetMarketZoneIdFromJwt();
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == req.InvoiceId,
                include: order => order.Include(h => h.Payments)
                                       .Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.Package)
                                       .Include(g => g.PackageOrder).ThenInclude(r => r.Wallets)
                                       .Include(p => p.PromotionOrders))
                ?? throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            //begin confirm order cash from Lost Package
            //not allow Staus Canceled
            //, Active package, -return packageitemID, update status transaction where order Id of this one 
            if (order.Status == OrderStatus.Canceled.GetDescriptionFromEnum())
                throw new BadHttpRequestException(OrderMessage.Canceled, HttpStatusCodes.BadRequest);

            if (order.SaleType == SaleType.FeeChargeCreate.ToString())
            {
                var transactionFee = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                    (predicate: x => x.Id == Guid.Parse(req.TransactionId))
                    ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                if (transactionFee.Status == "Success")
                    throw new BadHttpRequestException("Order had been PAID", HttpStatusCodes.BadRequest);
                transactionFee.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                transactionFee.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionFee);

                order.Status = OrderStatus.Completed.GetDescriptionFromEnum();
                order.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());

                order.PackageOrder.Status = PackageItemStatusEnum.Active.GetDescriptionFromEnum();
                order.PackageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(order.PackageOrder);
                ////UPDATE CASHIER WALLET
                order.User.Wallets.SingleOrDefault().Balance += 50000;
                //order.User.Wallets.SingleOrDefault().BalanceHistory += 50000;
                order.User.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.User.Wallets.SingleOrDefault());

                //session update
                sessionUser.TotalQuantityOrder += 1;
                sessionUser.TotalCashReceive += 50000;
                sessionUser.TotalFinalAmountOrder += 50000;
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);
                return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.SuccessGenerateNewPAID,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageOrderIIId = order.PackageOrderId,
                    }
                } : new ResponseAPI()
                {
                    MessageResponse = PackageItemMessage.FailedToGenerateNew,
                    StatusCode = HttpStatusCodes.BadRequest
                };

            }

            //-----check carefully-------
            if (order.SaleType == SaleType.PackageItemCharge)
            {
                if (order.PackageOrder == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
            }
            if (order.Payments.SingleOrDefault().Name != PaymentTypeEnum.Cash.GetDescriptionFromEnum())
                throw new BadHttpRequestException("Payment is not cash", HttpStatusCodes.BadRequest);
            //---------------------------
            if (order.SaleType == SaleType.PackageItemCharge && order.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum())
            {
                if (order.PromotionOrders.Count > 0)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                    //update payment 
                    order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                    order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                    //session update
                    sessionUser.TotalQuantityOrder += 1;
                    sessionUser.TotalCashReceive += order.TotalAmount;
                    sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                    _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

                    //wallet cashier
                    var wallet = order.User.Wallets.FirstOrDefault();
                    wallet.Balance += order.TotalAmount;
                    wallet.BalanceHistory -= order.TotalAmount;
                    wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                    var transactionCharge = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.Id == Guid.Parse(req.TransactionChargeId))
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);

                    transactionCharge.Status = TransactionStatus.Success;
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionCharge);
                    var customerMoneyTransfer = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        IsIncrease = true,
                        MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                        PackageOrderId = (Guid)order.PackageOrderId,
                        TransactionId = transactionCharge.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Status = OrderStatus.Completed,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(customerMoneyTransfer);

                    //update wallet package order
                    var packageOrderWallet = order.PackageOrder.Wallets.SingleOrDefault();


                    packageOrderWallet.Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    packageOrderWallet.BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    packageOrderWallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderWallet);
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
                        OrderId = order.Id,
                        WalletId = order.User.Wallets.FirstOrDefault().Id,
                        UserId = order.UserId,
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        PaymentId = order.Payments.SingleOrDefault().Id
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == marketZoneId)
                        ?? throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
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
                        Description = "Refund money from order: " + order.InvoiceId + "to PackageItem: " + order.PackageOrder.CusName,
                        IsIncrease = false,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.RefundMoney,
                        OrderId = order.Id,
                        WalletId = walletAdmin.Id,
                        UserId = admin.Id,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionAdminBalanceHistory);
                    //walletAdmin.BalanceHistory -= order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    //walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    //_unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    int check = await _unitOfWork.CommitAsync();
                    if (check > 0)
                    {
                        try
                        {
                            string subject = "Charge Money VCard Vega City Successfully";
                            string body = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
                                <div style='background-color: #007bff; color: white; padding: 20px; text-align: center;'>
                                    <h1 style='margin: 0;'>Welcome to our Vega City!</h1>
                                </div>
                                <div style='padding: 20px; background-color: #f9f9f9;'>
                                    <p style='font-size: 16px; color: #333;'>Hello, <strong>{order.PackageOrder.CusName}</strong>,</p>
                                    <p style='font-size: 16px; color: #333; line-height: 1.5;'>
                                        Thanks for charging Money to V-Card our service. We are happy to accompany you on the upcoming journey.
                                    </p>
                                    <p style='font-size: 16px; color: #333; line-height: 1.5;'>
                                        Please access link to know more about us <a href='https://vega-city-landing-page.vercel.app/' style='color: #007bff; text-decoration: none;'>our website</a> to learn more about special offers just for you.
                                    </p>
                                    <div style='margin-top: 20px; text-align: center;'>
                                        <a style='display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; border-radius: 5px; text-decoration: none; font-size: 16px;'>
                                            Your VCard {order.PackageOrder.CusName} is charging money with {order.TotalAmount} successfully !!
                                        </a>
                                    </div>
                                    <div style='margin-top: 20px; text-align: center;'>
                                        <a href='https://vegacity.id.vn/etagEdit/{order.PackageOrder.Id}' style='display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; border-radius: 5px; text-decoration: none; font-size: 16px;'>
                                             Click here to see your balance
                                        </a>
                                    </div>
                                </div>
                                <div style='background-color: #333; color: white; padding: 10px; text-align: center; font-size: 14px;'>
                                    <p style='margin: 0;'>© 2024 Vega City. All rights reserved.</p>
                                </div>
                            </div>";
                            await MailUtil.SendMailAsync(order.PackageOrder.CusEmail, subject, body);
                        }
                        catch (Exception ex)
                        {
                            return new ResponseAPI
                            {
                                StatusCode = HttpStatusCodes.OK,
                                MessageResponse = UserMessage.SendEmailChargeError
                            };
                        }
                    }
                    else throw new BadHttpRequestException("Error when commit", HttpStatusCodes.BadRequest);
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
                    //payment
                    order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                    order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                    //session update
                    sessionUser.TotalQuantityOrder += 1;
                    sessionUser.TotalCashReceive += order.TotalAmount;
                    sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                    _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);
                    //wallet cashier
                    var wallet = order.User.Wallets.FirstOrDefault();
                    wallet.Balance += order.TotalAmount;
                    wallet.BalanceHistory -= order.TotalAmount;
                    wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
                    var transactionCharge = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.Id == Guid.Parse(req.TransactionChargeId))
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    // transactionCharge.DespositId = deposit.Id;
                    _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionCharge);
                    var customerMoneyTransfer = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        IsIncrease = true,
                        MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                        PackageOrderId = (Guid)order.PackageOrderId,
                        TransactionId = transactionCharge.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Status = OrderStatus.Completed,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(customerMoneyTransfer);
                    //create new CUSTOMERMONEY TRANSFER
                    //update wallet package order
                    var packageOrderWallet = order.PackageOrder.Wallets.SingleOrDefault();
                    packageOrderWallet.Balance += order.TotalAmount;
                    packageOrderWallet.BalanceHistory += order.TotalAmount;
                    packageOrderWallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderWallet);

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
                        OrderId = order.Id,
                        WalletId = order.User.Wallets.FirstOrDefault().Id,
                        UserId = order.UserId,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == marketZoneId)
                        ?? throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZoneId, include: z => z.Include(w => w.Wallets))
                        ?? throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new Transaction()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                        Description = "Refund money from order: " + order.InvoiceId + "to PackageItem: " + order.PackageOrder.CusName,
                        IsIncrease = false,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.RefundMoney,
                        OrderId = order.Id,
                        WalletId = walletAdmin.Id,
                        UserId = admin.Id,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    //walletAdmin.BalanceHistory -= order.TotalAmount;
                    //walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    //_unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);

                    await _unitOfWork.CommitAsync();
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                    };
                }
            }
            else if (order.SaleType == SaleType.Package && order.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum())
            {
                order.Status = OrderStatus.Completed;
                order.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                //payment
                order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                //session update
                sessionUser.TotalQuantityOrder += 1;
                sessionUser.TotalCashReceive += order.TotalAmount;
                sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

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
            var sessionUser = await _util.CheckUserSession(GetUserIdFromJwt());
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(g => g.PackageOrder).ThenInclude(w => w.Wallets)
                                       .Include(p => p.PromotionOrders)
                                       .Include(o => o.OrderDetails)
                                       .Include(t => t.Payments).Include(r => r.Store))
                                       ?? throw new BadHttpRequestException("Order not found", HttpStatusCodes.NotFound);
            if (order.Payments.SingleOrDefault().Name == PaymentTypeEnum.QRCode.GetDescriptionFromEnum())
            {
                var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == order.StoreId);
                if(store.StoreType == (int)StoreTypeEnum.Service) //product 1, srv2
                {
                    order.Status = OrderStatus.Renting;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                }
                else
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                }
                //payment
                order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());

                //session update
                sessionUser.TotalQuantityOrder += 1;
                sessionUser.TotalCashReceive += order.TotalAmount;
                sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

                Wallet walletStore = null;
                foreach (var item in order.User.Wallets)
                {
                    if (item.StoreId == order.StoreId)
                    {
                        walletStore = item;
                        break;
                    }
                }
                if (walletStore == null) throw new BadHttpRequestException("Wallet store not found", HttpStatusCodes.NotFound);

                //create new CUSTOMERMONEY TRANSFER
                var packageOrderWallet = order.PackageOrder.Wallets.SingleOrDefault();
                packageOrderWallet.Balance -= order.TotalAmount;
                packageOrderWallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderWallet);

                //update product
                foreach(var product in order.OrderDetails)
                {
                    var productInOrderDetail = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(predicate: x => x.Id == product.ProductId);
                    if (productInOrderDetail.Quantity < product.Quantity)
                        throw new BadHttpRequestException("Not enough available item for this Order", HttpStatusCodes.BadRequest);
                    productInOrderDetail.Quantity -= product.Quantity;
                    _unitOfWork.GetRepository<Product>().UpdateAsync(productInOrderDetail);
                }

                var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                    (predicate: x => x.Id == Guid.Parse(req.TransactionId))
                    ?? throw new BadHttpRequestException("Transaction sale was not found", HttpStatusCodes.NotFound);
                transaction.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                transaction.UpsDate = TimeUtils.GetCurrentSEATime();
                //update another transaction
                var OrderTransaction = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(predicate: x => x.Id == transaction.OrderId);
                var transactionOther = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                    (predicate: x => x.OrderId == OrderTransaction.Id)
                    ?? throw new BadHttpRequestException("Transaction other was not found", HttpStatusCodes.NotFound);
                transactionOther.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                transactionOther.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Transaction>().UpdateAsync(transactionOther);

                var newCusTransfer = new CustomerMoneyTransfer()
                {
                    Id = Guid.NewGuid(),
                    Amount = order.TotalAmount,
                    IsIncrease = false,
                    MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                    PackageOrderId = (Guid)order.PackageOrderId,
                    TransactionId = transaction.Id,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Status = OrderStatus.Completed,
                };
                await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(newCusTransfer);

                //
                var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId,
                    include: z => z.Include(g => g.MarketZoneConfig));
                var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                    (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                var transactionStoreTransfer = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                    Amount = (int)(order.TotalAmount - order.TotalAmount * order.Store.StoreTransferRate),
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
                //walletStore.Balance += (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate);
                walletStore.BalanceHistory += (int)(order.TotalAmount - order.TotalAmount * order.Store.StoreTransferRate);
                walletStore.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletStore);
                var transfer = new StoreMoneyTransfer()
                {
                    Id = Guid.NewGuid(),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Amount = (int)(order.TotalAmount - order.TotalAmount * order.Store.StoreTransferRate),
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
                    Amount = (int)(order.TotalAmount * order.Store.StoreTransferRate),
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
                walletAdmin.Balance += (int)(order.TotalAmount * order.Store.StoreTransferRate);
                walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                var transfertoVega = new StoreMoneyTransfer
                {
                    Id = Guid.NewGuid(),
                    Amount = (int)(order.TotalAmount * order.Store.StoreTransferRate),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    Description = "Transfer money from order " + order.InvoiceId + " to Vega",
                    IsIncrease = true,
                    MarketZoneId = marketZone.Id,
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
            else if (order.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum())
            {
                var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == order.StoreId);
                if (store.StoreType == (int)StoreTypeEnum.Service) //product 1, srv2
                {
                    order.Status = OrderStatus.Renting;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                }
                else
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                }

                //payment
                order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());

                //session update
                sessionUser.TotalQuantityOrder += 1;
                sessionUser.TotalCashReceive += order.TotalAmount;
                sessionUser.TotalFinalAmountOrder += order.TotalAmount;
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

                Wallet walletStore = null;
                foreach (var item in order.User.Wallets)
                {
                    if (item.StoreId == order.StoreId)
                    {
                        walletStore = item;
                        break;
                    }
                }
                if (walletStore == null) throw new BadHttpRequestException("Wallet store not found", HttpStatusCodes.NotFound);

                //create new CUSTOMERMONEY TRANSFER
                //var packageOrderWallet = order.PackageOrder.Wallets.SingleOrDefault();
                //packageOrderWallet.Balance -= order.TotalAmount;
                //packageOrderWallet.UpsDate = TimeUtils.GetCurrentSEATime();
                //_unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderWallet);
                //update product
                foreach (var product in order.OrderDetails)
                {
                    var productInOrderDetail = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(predicate: x => x.Id == product.ProductId);
                    if (productInOrderDetail.Quantity < product.Quantity)
                        throw new BadHttpRequestException("Not enough available item for this Order", HttpStatusCodes.BadRequest);
                    productInOrderDetail.Quantity -= product.Quantity;
                    _unitOfWork.GetRepository<Product>().UpdateAsync(productInOrderDetail);
                }

                var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                    (predicate: x => x.Id == Guid.Parse(req.TransactionId))
                    ?? throw new BadHttpRequestException("Transaction sale not found", HttpStatusCodes.NotFound);
                transaction.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                transaction.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);

                //var newCusTransfer = new CustomerMoneyTransfer()
                //{
                //    Id = Guid.NewGuid(),
                //    Amount = order.TotalAmount,
                //    IsIncrease = false,
                //    MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                //    PackageOrderId = (Guid)order.PackageOrderId,
                //    TransactionId = transaction.Id,
                //    CrDate = TimeUtils.GetCurrentSEATime(),
                //    UpsDate = TimeUtils.GetCurrentSEATime(),
                //    Status = OrderStatus.Completed,
                //};
                //await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(newCusTransfer);

                //
                //var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId,
                //    include: z => z.Include(g => g.MarketZoneConfig));
                //var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                //    (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                //var transactionStoreTransfer = new Transaction
                //{
                //    Id = Guid.NewGuid(),
                //    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                //    Amount = (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                //    CrDate = TimeUtils.GetCurrentSEATime(),
                //    Description = "Transfer money from order " + order.InvoiceId + " to store",
                //    IsIncrease = true,
                //    Status = TransactionStatus.Success,
                //    StoreId = order.StoreId,
                //    UpsDate = TimeUtils.GetCurrentSEATime(),
                //    Type = TransactionType.TransferMoney,
                //    UserId = order.UserId,
                //    WalletId = walletStore.Id,
                //    OrderId = order.Id
                //};
                //await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionStoreTransfer);
                walletStore.Balance += order.TotalAmount;
                //walletStore.BalanceHistory += order.TotalAmount;
                walletStore.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletStore);
                //var transfer = new StoreMoneyTransfer()
                //{
                //    Id = Guid.NewGuid(),
                //    CrDate = TimeUtils.GetCurrentSEATime(),
                //    UpsDate = TimeUtils.GetCurrentSEATime(),
                //    Amount = (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                //    IsIncrease = true,
                //    MarketZoneId = order.User.MarketZoneId,
                //    StoreId = (Guid)order.StoreId,
                //    TransactionId = transactionStoreTransfer.Id,
                //    Status = OrderStatus.Completed,
                //    Description = "Transfer money from order " + order.InvoiceId + " to store"
                //};
                //await _unitOfWork.GetRepository<StoreMoneyTransfer>().InsertAsync(transfer);
                //var transactionVega = new Transaction
                //{
                //    Id = Guid.NewGuid(),
                //    CrDate = TimeUtils.GetCurrentSEATime(),
                //    Currency = CurrencyEnum.VND.GetDescriptionFromEnum(),
                //    Amount = (int)(order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                //    Description = "Transfer money from order " + order.InvoiceId + " to Vega",
                //    IsIncrease = true,
                //    Status = TransactionStatus.Success,
                //    StoreId = order.StoreId,
                //    UpsDate = TimeUtils.GetCurrentSEATime(),
                //    OrderId = order.Id,
                //    Type = TransactionType.TransferMoney,
                //    UserId = admin.Id,
                //    WalletId = admin.Wallets.FirstOrDefault().Id
                //};
                //await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionVega);
                //var walletAdmin = admin.Wallets.FirstOrDefault();
                //walletAdmin.Balance += (int)(order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate);
                //walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                //_unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                //var transfertoVega = new StoreMoneyTransfer
                //{
                //    Id = Guid.NewGuid(),
                //    Amount = (int)(order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate),
                //    CrDate = TimeUtils.GetCurrentSEATime(),
                //    Description = "Transfer money from order " + order.InvoiceId + " to Vega",
                //    IsIncrease = true,
                //    MarketZoneId = marketZone.Id,
                //    Status = OrderStatus.Completed,
                //    StoreId = (Guid)order.StoreId,
                //    TransactionId = transactionVega.Id,
                //    UpsDate = TimeUtils.GetCurrentSEATime()
                //};
                //await _unitOfWork.GetRepository<StoreMoneyTransfer>().InsertAsync(transfertoVega);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = OrderMessage.ConfirmOrderSuccessfully,
                };
            }
            else throw new BadHttpRequestException("Payment type Invalid", HttpStatusCodes.BadRequest);

        }
        public async Task CheckOrderPending()
        {
            var orders = await _unitOfWork.GetRepository<Order>().
                GetListAsync(predicate: x => x.Status == OrderStatus.Pending,
                             include: z => z.Include(p => p.PromotionOrders)
                                            .Include(a => a.Transactions)
                                            .Include(a => a.Payments));
            foreach (var order in orders)
            {
                if (TimeUtils.GetCurrentSEATime().Subtract(order.CrDate).TotalMinutes > 5)
                {
                    order.Status = OrderStatus.Canceled;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                }
                if (order.PromotionOrders.Count > 0)
                {
                    foreach (var orderPromotion in order.PromotionOrders)
                    {
                        orderPromotion.Deflag = true;
                        orderPromotion.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<PromotionOrder>().UpdateAsync(orderPromotion);
                    }
                }
                if (order.Transactions.Count > 0)
                {
                    foreach (var transaction in order.Transactions)
                    {
                        transaction.Status = TransactionStatus.Fail.GetDescriptionFromEnum();
                        transaction.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);
                    }
                }
                if (order.Payments.Count > 0)
                {
                    foreach (var payment in order.Payments)
                    {
                        payment.Status = PaymentStatus.Canceled;
                        payment.UpsDate = TimeUtils.GetCurrentSEATime();
                        _unitOfWork.GetRepository<Payment>().UpdateAsync(payment);
                    }
                }
            }
            await _unitOfWork.CommitAsync();
        }
        public async Task CheckRentingOrder()
        {
            var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(predicate: x => x.Status == OrderStatus.Renting && x.EndRent != null && x.StartRent != null,
                                                                                 include: y => y.Include(d => d.OrderDetails));
            foreach (var order in orders)
            {
                if(order.EndRent <= TimeUtils.GetCurrentSEATime())
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().GetListAsync(predicate: x => x.OrderId == order.Id);

                    foreach(var detail in orderDetails)
                    {
                        var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(predicate: x => x.Id == detail.ProductId);
                        product.Quantity += detail.Quantity;
                        _unitOfWork.GetRepository<Product>().UpdateAsync(product);
                    }
                    await _unitOfWork.CommitAsync();
                }
            }
        }
        public async Task<ResponseAPI> GetDetailMoneyFromOrder(Guid orderId)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: z => z.Id == orderId, 
                include: o => o.Include(s => s.Transactions).ThenInclude(z => z.StoreMoneyTransfers)
                               .Include(z => z.Store)
                               .Include(z => z.Payments))
                ?? throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
            int PriceTransferToVega = 0;
            int PriceStoreHandle = 0;

            if(GetRoleFromJwt() == RoleEnum.Store.GetDescriptionFromEnum())
            {
                PriceTransferToVega = (int)(order.TotalAmount * order.Store.StoreTransferRate);
                PriceStoreHandle = order.TotalAmount - PriceTransferToVega;
                if(order.StoreId != null && order.Payments.SingleOrDefault().Name == PaymentTypeEnum.QRCode.GetDescriptionFromEnum())
                {
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.GetOrdersSuccessfully,
                        Data = new
                        {
                            PriceTransferToVega,
                            PriceStoreHandle,
                            BalanceAtPresent = order.BalanceBeforePayment,
                            BalanceHistoryBefore = order.BalanceHistoryBeforePayment,
                            BalanceHistoryAfter = order.BalanceHistoryBeforePayment + PriceStoreHandle
                        }
                    };
                }
                else
                {
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.GetOrdersSuccessfully,
                        Data = new
                        {
                            BalanceBefore = order.BalanceBeforePayment,
                            BalanceAfter = order.BalanceBeforePayment + order.TotalAmount,
                            BalanceHistoryAtPresent = order.BalanceHistoryBeforePayment
                        }
                    };
                }
            }
            else if (GetRoleFromJwt() == RoleEnum.CashierWeb.GetDescriptionFromEnum() || GetRoleFromJwt() == RoleEnum.CashierApp.GetDescriptionFromEnum())
            {
                if (order.StoreId == null && order.Payments.SingleOrDefault().Name != PaymentTypeEnum.QRCode.GetDescriptionFromEnum())
                {
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = OrderMessage.GetOrdersSuccessfully,
                        Data = new
                        {
                            BalanceBefore = order.BalanceBeforePayment,
                            BalanceAfter = order.BalanceBeforePayment + order.TotalAmount,
                            BalanceHistoryAtPresent = order.BalanceHistoryBeforePayment
                        }
                    };
                }  
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = OrderMessage.GetOrdersFail,
            };
        }
    }
}

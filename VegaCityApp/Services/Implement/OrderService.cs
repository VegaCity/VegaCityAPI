﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public OrderService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<OrderService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {

        }


        public async Task<ResponseAPI> CreateOrder(CreateOrderRequest req)
        {
            if (PaymentTypeHelper.allowedPaymentTypes.Contains(req.paymentType) == false)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = OrderMessage.PaymentTypeInvalid
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
                .SingleOrDefaultAsync(predicate: x => x.Id == req.StoreId && !x.Deflag && x.Status == (int)StoreStatusEnum.Opened)
                ?? throw new BadHttpRequestException("Store not found", HttpStatusCodes.NotFound);
            if (req.TotalAmount <= 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.TotalAmountInvalid,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var packageOrderExist = await _unitOfWork.GetRepository<PackageOrder>()
                    .SingleOrDefaultAsync(predicate: x => x.Id == req.PackageOrderId,
                                          include: a => a.Include(v => v.Wallets)
                                                         .Include(p => p.Package))
                ?? throw new BadHttpRequestException("Package Order not found", HttpStatusCodes.NotFound);
            if (packageOrderExist.Wallets.SingleOrDefault().Balance < req.TotalAmount) throw new BadHttpRequestException("Balance not enough", HttpStatusCodes.BadRequest);
            //add user ID for Store Type
            Guid userID = GetUserIdFromJwt();
            var newOrder = new Order()
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
                PackageOrderId = req.PackageOrderId,
                PackageId = packageOrderExist.PackageId,
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
            //create order Detail
            List<Product> products = new List<Product>();
            //List<Domain.Models.StoreService> storeServices = new List<Domain.Models.StoreService>();
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
                    WalletId = packageOrderExist.Wallets.SingleOrDefault().Id,
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    UserId = userID,
                    StoreId = store.Id,
                    PaymentId = newPayment.Id
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
            else
                throw new BadHttpRequestException("Sale type in valid", HttpStatusCodes.NotFound);
        }
        public async Task<ResponseAPI> DeleteOrder(Guid OrderId)
        {
            var orderExisted = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x => x.Id == OrderId && x.Status == OrderStatus.Pending,
                include: z => z.Include(u => u.Package).ThenInclude(o => o.PackageOrders).Include(p => p.PromotionOrders).Include(z => z.Payments));
            if (orderExisted == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (orderExisted.Package.PackageOrders.Count() != 0)
            {
                //foreach (var packageOrder in orderExisted.Package.PackageOrders)
                //{
                //    packageOrder.Status = OrderStatus.Canceled;
                //    packageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                //    _unitOfWork.GetRepository<PackageOrder>().UpdateAsync(packageOrder);
                //}
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
                    Name = x.Name,
                    TotalAmount = x.TotalAmount,
                    CrDate = x.CrDate,
                    Status = x.Status,
                    InvoiceId = x.InvoiceId,
                    StoreId = x.StoreId,
                    PackageId = x.PackageId,
                    UserId = x.UserId,
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
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> UpdateOrder(string InvoiceId, UpdateOrderRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.InvoiceId == InvoiceId && x.Status == OrderStatus.Pending,
                                      include: p => p.Include(a => a.Payments).Include(d => d.OrderDetails));
            if (order == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            if (req.TotalAmount <= 0)
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
            //order.PaymentType = req.PaymentType?? order.PaymentType;
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
                    .Include(h => h.PromotionOrders)) ?? throw new BadHttpRequestException(OrderMessage.NotFoundOrder, HttpStatusCodes.NotFound);
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
            if (req.TotalAmount <= 0)
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
            //packageOrder Here
            var newPackageOrder = new PackageOrder()
            {
                Id = Guid.NewGuid(),
                CrDate = TimeUtils.GetCurrentSEATime(),
                CusCccdpassport = req.CustomerInfo.CccdPassport,
                CusEmail = req.CustomerInfo.Email,
                CusName = req.CustomerInfo.FullName,
                PhoneNumber = req.CustomerInfo.PhoneNumber,
                UpsDate = TimeUtils.GetCurrentSEATime(),
            };
            await _unitOfWork.GetRepository<PackageOrder>().InsertAsync(newPackageOrder);
            //
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
                PackageOrderId = newPackageOrder.Id,
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
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
            //create order Detail
            var orderDetail = new OrderDetail() //add userId here
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                //ProductId = Guid.Parse(req.ProductData.SingleOrDefault().Id),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Quantity = count,
                Amount = req.TotalAmount,
                FinalAmount = amount,
                PromotionAmount = 0,
                Vatamount = (int)(EnvironmentVariableConstant.VATRate * amount)
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
                UserId = userId,
                PaymentId = newPayment.Id,
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
                predicate: x => x.InvoiceId == req.InvoiceId,
                include: order => order.Include(h => h.Payments).Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(a => a.OrderDetails).Include(x => x.Package)
                                       .Include(g => g.PackageOrder)
                                       .ThenInclude(r => r.Wallets).Include(p => p.PromotionOrders));
            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //session check
            //begin confirm order cash from Lost Package
            //not allow Staus Canceled
            //, Active package, -return packageitemID, update status transaction where order Id of this one 
            if (order.Status == OrderStatus.Canceled.GetDescriptionFromEnum())
            {
                throw new BadHttpRequestException(OrderMessage.Canceled, HttpStatusCodes.BadRequest);
            }
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
                order.User.Wallets.SingleOrDefault().BalanceHistory += 50000;
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

            //end Lost Package case
            //
            if (order.SaleType == SaleType.PackageItemCharge)
            {
                if (order.PackageOrder == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
            }
            if (order.Payments.SingleOrDefault().Name != PaymentTypeEnum.Cash.GetDescriptionFromEnum())
                throw new BadHttpRequestException("Payment is not cash", HttpStatusCodes.BadRequest);

            if (order.SaleType == SaleType.PackageItemCharge && order.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum())
            {
                if (order.PromotionOrders.Count == 1)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                    ////updateOrderDetail
                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
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
                        PackageOrderId = order.PackageOrderId,
                        TransactionId = transactionCharge.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Status = OrderStatus.Completed.GetDescriptionFromEnum(),
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(customerMoneyTransfer);

                    //update wallet package item
                    var packageOrderWallet = order.PackageOrder.Wallets.SingleOrDefault();


                    packageOrderWallet.Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    packageOrderWallet.BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    packageOrderWallet.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderWallet);


                    //create new CUSTOMERMONEY TRANSFER
                    var newCusTransfer = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        IsIncrease = true,
                        MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                        PackageOrderId = order.PackageOrderId,
                        TransactionId = transactionCharge.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(newCusTransfer);
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
                        //DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = order.User.Wallets.FirstOrDefault().Id,
                        UserId = order.UserId,
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        PaymentId = order.Payments.SingleOrDefault().Id,
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
                        Description = "Refund money from order: " + order.InvoiceId + "to PackageItem: " + order.PackageOrder.CusName,
                        IsIncrease = false,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.RefundMoney,
                        //DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = walletAdmin.Id,
                        UserId = admin.Id,
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);

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
                    //order.PackageOrder.Status = OrderStatus.Completed;
                    //order.PackageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                    //_unitOfWork.GetRepository<PackageOrder>().UpdateAsync(order.PackageOrder);
                    //wallet cashier
                    var wallet = order.User.Wallets.FirstOrDefault();
                    wallet.Balance += order.TotalAmount;
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
                        PackageOrderId = order.PackageOrderId,
                        TransactionId = transactionCharge.Id,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        Status = OrderStatus.Completed.GetDescriptionFromEnum(),
                        UpsDate = TimeUtils.GetCurrentSEATime()
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(customerMoneyTransfer);
                    //create new CUSTOMERMONEY TRANSFER
                    //update wallet package item
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
                        // DespositId = deposit.Id,
                        OrderId = order.Id,
                        WalletId = order.User.Wallets.FirstOrDefault().Id,
                        UserId = order.UserId,
                        UpsDate = TimeUtils.GetCurrentSEATime(),
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
                        Description = "Refund money from order: " + order.InvoiceId + "to PackageItem: " + order.PackageOrder.CusName,
                        IsIncrease = false,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        Type = TransactionType.RefundMoney,
                        // DespositId = deposit.Id,
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

                //order.PackageOrder.Status = OrderStatus.Completed;
                //order.PackageOrder.UpsDate = TimeUtils.GetCurrentSEATime();
                //_unitOfWork.GetRepository<PackageOrder>().UpdateAsync(order.PackageOrder);

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
                                       .Include(x => x.Package)
                                       .Include(g => g.PackageOrder).ThenInclude(w => w.Wallets)
                                       .Include(p => p.PromotionOrders)
                ) ?? throw new BadHttpRequestException("Order not found", HttpStatusCodes.NotFound);
            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //session check
            //
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

            //update wallet package item
            //var deposit = new Deposit()
            //{
            //    Id = Guid.NewGuid(),
            //    CrDate = TimeUtils.GetCurrentSEATime(),
            //    UpsDate = TimeUtils.GetCurrentSEATime(),
            //    Amount = order.TotalAmount,
            //    WalletId = order.PackageItem.WalletId,
            //    OrderId = order.Id,
            //    IsIncrease = false,
            //    Name = "Deposit from order " + order.InvoiceId,
            //    PaymentType = PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
            //    PackageItemId = (Guid)order.PackageItemId
            //};
            //await _unitOfWork.GetRepository<Deposit>().InsertAsync(deposit);
            //create new CUSTOMERMONEY TRANSFER
            var packageOrderWallet = order.PackageOrder.Wallets.SingleOrDefault();
            packageOrderWallet.Balance -= order.TotalAmount;
            packageOrderWallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(packageOrderWallet);
            var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync
                (predicate: x => x.Id == Guid.Parse(req.TransactionId))
                ?? throw new BadHttpRequestException("Transaction sale not found", HttpStatusCodes.NotFound);
            transaction.Status = TransactionStatus.Success.GetDescriptionFromEnum();
            transaction.UpsDate = TimeUtils.GetCurrentSEATime();
            //transaction.DespositId = deposit.Id;
            _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);

            var newCusTransfer = new CustomerMoneyTransfer()
            {
                Id = Guid.NewGuid(),
                Amount = order.TotalAmount,
                IsIncrease = false,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.marketZoneId),
                PackageOrderId = order.PackageOrderId,
                TransactionId = transaction.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime()
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
            walletStore.BalanceHistory += (int)(order.TotalAmount - order.TotalAmount * marketZone.MarketZoneConfig.StoreStranferRate);
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
    }
}

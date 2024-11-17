using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Net.payOS;
using Newtonsoft.Json;
using System.Net;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PaymentResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static VegaCityApp.API.Constants.MessageConstant;
using Microsoft.AspNetCore.Http;
using VegaCityApp.API.Payload.Request.Order;

namespace VegaCityApp.API.Services.Implement
{
    public class PaymentService : BaseService<PaymentService>, IPaymentService
    {
        private readonly PayOS _payOs;
        public PaymentService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PaymentService> logger, PayOS payOs,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _payOs = payOs;
        }

        public async Task<ResponseAPI> MomoPayment(PaymentRequest request)
        {
            var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                             (predicate: x => x.InvoiceId == request.InvoiceId 
                                           && x.Status == OrderStatus.Pending);
            if (checkOrder == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PaymentMessage.OrderNotFound
                };
            }
            
            //create momo payment request
            if (request.Key != null) {
                //checkKey
                try
                {
                    if (PaymentTypeHelper.allowedPaymentTypes.Contains(request.Key.Split("_")[0]) && request.Key.Split("_")[1] == checkOrder.InvoiceId)
                    {
                        var rawSignature = "accessKey=" + PaymentMomo.MomoAccessKey + "&amount=" + checkOrder.TotalAmount
                                + "&extraData=" + "&ipnUrl=" + request.UrlIpn + "&orderId=" + request.InvoiceId
                                + "&orderInfo=" + PaymentMomo.orderInfo + "&partnerCode=" + PaymentMomo.MomoPartnerCode
                                + "&redirectUrl=" + request.UrlDirect + "&requestId=" + request.InvoiceId
                                + "&requestType=" + PaymentMomo.requestType;
                        //create signature with sha256 and sercetkey
                        string signature = PasswordUtil.getSignature(rawSignature, PaymentMomo.MomoSecretKey);
                        var momoPaymentRequest = new MomoPaymentRequest
                        {
                            orderInfo = PaymentMomo.orderInfo,
                            partnerCode = PaymentMomo.MomoPartnerCode,
                            redirectUrl = request.UrlDirect,
                            ipnUrl = request.UrlIpn,
                            amount = checkOrder.TotalAmount,
                            orderId = request.InvoiceId,
                            requestId = request.InvoiceId,
                            requestType = PaymentMomo.requestType,
                            extraData = "",
                            partnerName = PaymentMomo.partnerName,
                            storeId = "VegaCity",
                            orderGroupId = "",
                            autoCapture = PaymentMomo.autoCapture,
                            lang = PaymentMomo.lang,
                            signature = signature,
                            orderExpireTime = PaymentMomo.orderExpireTime
                        };
                        //call momo api
                        var response = await CallApiUtils.CallApiEndpoint("https://test-payment.momo.vn/v2/gateway/api/create", momoPaymentRequest);
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            return new ResponseAPI
                            {
                                StatusCode = HttpStatusCodes.InternalServerError,
                                MessageResponse = PaymentMessage.MomoPaymentFail
                            };
                        }
                        var momoPaymentResponse = await CallApiUtils.GenerateObjectFromResponse<MomoPaymentResponse>(response);
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = PaymentMessage.MomoPaymentSuccess,
                            Data = momoPaymentResponse
                        };
                    }

                }
                catch (Exception ex)
                {
                    return new ResponseAPI
                    {
                        MessageResponse = PaymentMessage.MomoPaymentFail,
                        StatusCode = HttpStatusCodes.InternalServerError,
                        Data = ex.Message
                    };
                }
            } 
            else
            {
                //else
                var rawSignature = "accessKey=" + PaymentMomo.MomoAccessKey + "&amount=" + checkOrder.TotalAmount
                            + "&extraData=" + "&ipnUrl=" + PaymentMomo.ipnUrl + checkOrder.Id + "&orderId=" + request.InvoiceId
                            + "&orderInfo=" + PaymentMomo.orderInfo + "&partnerCode=" + PaymentMomo.MomoPartnerCode
                            + "&redirectUrl=" + PaymentMomo.redirectUrl + "&requestId=" + request.InvoiceId
                            + "&requestType=" + PaymentMomo.requestType;
                //create signature with sha256 and sercetkey
                string signature = PasswordUtil.getSignature(rawSignature, PaymentMomo.MomoSecretKey);
                var momoPaymentRequest = new MomoPaymentRequest
                {
                    orderInfo = PaymentMomo.orderInfo,
                    partnerCode = PaymentMomo.MomoPartnerCode,
                    redirectUrl = PaymentMomo.redirectUrl,
                    ipnUrl = PaymentMomo.ipnUrl+ checkOrder.Id,
                    amount = checkOrder.TotalAmount,
                    orderId = request.InvoiceId,
                    requestId = request.InvoiceId,
                    requestType = PaymentMomo.requestType,
                    extraData = "",
                    partnerName = PaymentMomo.partnerName,
                    storeId = checkOrder.StoreId.ToString(),
                    orderGroupId = "",
                    autoCapture = PaymentMomo.autoCapture,
                    lang = PaymentMomo.lang,
                    signature = signature,
                    orderExpireTime = PaymentMomo.orderExpireTime
                };
                //call momo api
                var response = await CallApiUtils.CallApiEndpoint("https://test-payment.momo.vn/v2/gateway/api/create", momoPaymentRequest);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.InternalServerError,
                        MessageResponse = PaymentMessage.MomoPaymentFail
                    };
                }
                var momoPaymentResponse = await CallApiUtils.GenerateObjectFromResponse<MomoPaymentResponse>(response);
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = PaymentMessage.MomoPaymentSuccess,
                    Data = momoPaymentResponse
                };
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = PaymentMessage.MomoPaymentFail
            };
        }
        public async Task<ResponseAPI> UpdateOrderPaidForCashier(IPNMomoRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending,
                 include: detail => detail.Include(a => a.OrderDetails)
                                          .Include(u => u.User).ThenInclude(w => w.Wallets)
                                          .Include(o => o.Payments));
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                (predicate: x => x.UserId == order.UserId)
                ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            //GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            //{
            //    StartDate = TimeUtils.GetCurrentSEATime(),
            //};
            List<Guid> res = new List<Guid>();
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    //productJson = item.ProductJson.Replace("[","").Replace("]","");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {

                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        //if (etagType != null)
                        //{
                        //    //generate etag
                        //    var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                        //    res = response.Data;
                        //}
                        //else if (package != null)
                        //{
                        //    package.PackageETagTypeMappings.ToList().ForEach(async x =>
                        //    {
                        //        var response = await _service.GenerateEtag(x.QuantityEtagType, x.EtagTypeId, reqGenerate);
                        //        res = response.Data;
                        //    });
                        //}
                    }
                }
            }
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
                Description = "Add" + order.SaleType + " Balance By " + order.Payments.SingleOrDefault().Name + " to cashier web: " + order.User.FullName,
            };
            await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

            order.User.Wallets.SingleOrDefault().BalanceHistory += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().Balance += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(order.User.Wallets.SingleOrDefault());

            //session update
            sessionUser.TotalQuantityOrder += 1;
            sessionUser.TotalCashReceive += order.TotalAmount;
            sessionUser.TotalFinalAmountOrder += order.TotalAmount;
            _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PaymentMomo.ipnUrl + order.Id 
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError
                };
        }
        public async Task<ResponseAPI> UpdateOrderPaidForChargingMoney(IPNMomoRequest req)
        {
            try
            {
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.PackageOrder).ThenInclude(r => r.Wallets)
                                       .Include(s => s.PromotionOrders).ThenInclude(a => a.Promotion)
                                       .Include(a => a.Payments)
                                       .Include(o => o.OrderDetails));
                var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);

                if (order.SaleType == SaleType.PackageItemCharge)
                {
                    if (order.PackageOrder == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
                }
                if (order.Payments.SingleOrDefault().Name != PaymentTypeEnum.Momo.GetDescriptionFromEnum())
                    throw new BadHttpRequestException("Payment is not Momo", HttpStatusCodes.BadRequest);
                //bonus here
                int PromotionAmount = 0; //not use
                if (order.PromotionOrders.Count == 1)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
                    //payment
                    order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                    order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                    //quantity promotion -
                    //
                    //order.PromotionOrders.SingleOrDefault().Promotion.Quantity -= 1;
                    //_unitOfWork.GetRepository<Promotion>().UpdateAsync(order.PromotionOrders.SingleOrDefault().Promotion);
                    //session update
                    //
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());

                    

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.SingleOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    //walletAdmin.BalanceHistory -= order.TotalAmount;
                    //walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    //_unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentZaloPay.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
                else
                {

                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());

                    

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentMomo.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
            }catch (Exception ex)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = ex.Message
                };
            }
        }
        public async Task<ResponseAPI> VnPayment(PaymentRequest req, HttpContext context)
        {
            var orderExisted = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending);
            if (orderExisted == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = OrderMessage.NotFoundOrder
                };
            }

            if (req.Key != null && PaymentTypeHelper.allowedPaymentTypes.Contains(req.Key.Split('_')[0]) && req.Key.Split("_")[1] == orderExisted.InvoiceId)
            {
                try
                {
                   
                        var tickCharge = TimeUtils.GetCurrentSEATime().ToString();
                        var vnpayCharge = new VnPayLibrary();
                        vnpayCharge.AddRequestData("vnp_Version", VnPayConfig.Version);
                        vnpayCharge.AddRequestData("vnp_Command", VnPayConfig.Command);
                        vnpayCharge.AddRequestData("vnp_TmnCode", VnPayConfig.TmnCode);
                        vnpayCharge.AddRequestData("vnp_Amount", (orderExisted.TotalAmount*100 ).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 
                        // vnpayCharge.AddRequestData("vnp_CreateDate", TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()));
                        vnpayCharge.AddRequestData("vnp_CreateDate", TimeUtils.GetCurrentSEATime().AddHours(7).ToString("yyyyMMddHHmmss"));
                        vnpayCharge.AddRequestData("vnp_CurrCode", VnPayConfig.CurrCode);
                        vnpayCharge.AddRequestData("vnp_IpAddr", VnPayUtils.GetIpAddress(context));
                        vnpayCharge.AddRequestData("vnp_Locale", VnPayConfig.Locale);
                        vnpayCharge.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng (InvoiceId):" + req.InvoiceId);
                        vnpayCharge.AddRequestData("vnp_OrderType", "other"); //default value: other
                        vnpayCharge.AddRequestData("vnp_ReturnUrl", req.UrlDirect);
                        vnpayCharge.AddRequestData("vnp_TxnRef", tickCharge); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                        var paymentUrlChargeMoney = vnpayCharge.CreateRequestUrl(VnPayConfig.BaseUrl, VnPayConfig.HashSecret);
                        //
                        if (paymentUrlChargeMoney == null)
                        {
                            return new ResponseAPI
                            {
                                StatusCode = HttpStatusCodes.InternalServerError,
                                MessageResponse = PaymentMessage.vnPayFailed
                            };
                        }

                        return new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = PaymentMessage.VnPaySuccess,
                            Data = new VnPaymentResponse()
                            {
                                Success = true,
                                PaymentMethod = "VnPay",
                                OrderDescription = "Thanh toán VnPay cho đơn hàng :" + req.InvoiceId,
                                OrderId = req.InvoiceId,
                                Amount = orderExisted.TotalAmount,
                                VnPayResponse = paymentUrlChargeMoney,
                                CrDate = TimeUtils.GetCurrentSEATime()

                            }
                        };
                    
                }
                catch (Exception ex)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = PaymentMessage.vnPayFailed + ex.Message,
                        StatusCode = HttpStatusCodes.InternalServerError
                    };
                }
            }
            var tick = TimeUtils.GetCurrentSEATime().ToString();
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayConfig.Version);
            vnpay.AddRequestData("vnp_Command", VnPayConfig.Command);
            vnpay.AddRequestData("vnp_TmnCode", VnPayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", (orderExisted.TotalAmount *100 ).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_CreateDate", TimeUtils.GetCurrentSEATime().AddHours(7).ToString("yyyyMMddHHmmss"));
            //vnpay.AddRequestData("vnp_CreateDate", TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()));
            vnpay.AddRequestData("vnp_CurrCode", VnPayConfig.CurrCode);
            vnpay.AddRequestData("vnp_IpAddr", VnPayUtils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", VnPayConfig.Locale);

            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng (InvoiceId):" + req.InvoiceId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", VnPayConfig.PaymentBackReturnUrl);

            vnpay.AddRequestData("vnp_TxnRef", tick); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            var paymentUrl = vnpay.CreateRequestUrl(VnPayConfig.BaseUrl, VnPayConfig.HashSecret);
            //
            if (paymentUrl == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = PaymentMessage.vnPayFailed
                };
            }

            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = PaymentMessage.VnPaySuccess,
                Data = new VnPaymentResponse()
                {
                    Success = true,
                    PaymentMethod = "VnPay",
                    OrderDescription = "Thanh toán VnPay cho đơn hàng :" + req.InvoiceId,
                    OrderId = req.InvoiceId,
                    Amount = orderExisted.TotalAmount,
                    VnPayResponse = paymentUrl,
                    CrDate = TimeUtils.GetCurrentSEATime()

                }
            };

        }
        public async Task<ResponseAPI> UpdateVnPayOrder(VnPayPaymentResponse req)
        {
            var invoiceId = req.vnp_OrderInfo.Split(":", 2);
            List<Guid> listEtagCreated = new List<Guid>();
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == invoiceId[1] && x.Status == OrderStatus.Pending,
                 include: detail => detail.Include(a => a.OrderDetails).Include(u => u.User).ThenInclude(w => w.Wallets));
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
            (predicate: x => x.UserId == order.UserId)
            ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            //GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            //{
            //    StartDate = TimeUtils.GetCurrentSEATime(),
            //};
            
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    //productJson = item.ProductJson.Replace("[","").Replace("]","");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {
                        
                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        //if (etagType != null)
                        //{
                        //    //generate etag
                        //    var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                        //    listEtagCreated = response.Data;
                        //}
                        //else if (package != null)
                        //{
                        //    foreach(var itemm in package.PackageETagTypeMappings)
                        //    {
                        //        var response = await _service.GenerateEtag(itemm.QuantityEtagType, itemm.EtagTypeId, reqGenerate);
                        //        listEtagCreated = response.Data;
                        //    }
                        //}
                    }
                }
            }
            string data = EnCodeBase64.EncodeBase64<List<Guid>>(listEtagCreated);
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
                Description = "Add" + order.SaleType + " Balance By " + order.Payments.SingleOrDefault().Name +  " to cashier web: " + order.User.FullName,
            };
            await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

            order.User.Wallets.SingleOrDefault().BalanceHistory += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().Balance += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(order.User.Wallets.SingleOrDefault());

            //session update
            sessionUser.TotalQuantityOrder += 1;
            sessionUser.TotalCashReceive += order.TotalAmount;
            sessionUser.TotalFinalAmountOrder += order.TotalAmount;
            _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);

            return await _unitOfWork.CommitAsync() > 0
            ? new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.NoContent,
                MessageResponse = PaymentMomo.ipnUrl + order.Id 
            }
            : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.InternalServerError
            };
        }
        public async Task<ResponseAPI> UpdateOrderPaidForChargingMoney(VnPayPaymentResponse req) //done to fix time
        {
            try
            {
                string invoiceId = req.vnp_OrderInfo.Split(":", 2)[1];
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.PackageOrder).ThenInclude(r => r.Wallets)
                                       .Include(s => s.PromotionOrders).ThenInclude(a => a.Promotion)
                                       .Include(a => a.Payments)
                                       .Include(o => o.OrderDetails));
                var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);

                if (order.SaleType == SaleType.PackageItemCharge)
                {
                    if (order.PackageOrder == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
                }
                if (order.Payments.SingleOrDefault().Name != PaymentTypeEnum.VnPay.GetDescriptionFromEnum())
                    throw new BadHttpRequestException("Payment is not VnPay", HttpStatusCodes.BadRequest);
                //bonus here
                int PromotionAmount = 0; //not use
                if (order.PromotionOrders.Count == 1)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
                    //payment
                    order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                    order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                    //quantity promotion -
                    //
                    //order.PromotionOrders.SingleOrDefault().Promotion.Quantity -= 1;
                    //_unitOfWork.GetRepository<Promotion>().UpdateAsync(order.PromotionOrders.SingleOrDefault().Promotion);
                    //session update
                    //
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());



                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    //walletAdmin.BalanceHistory -= order.TotalAmount;
                    //walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    //_unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentZaloPay.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
                else
                {

                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentMomo.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
            }
            catch (Exception ex)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = ex.Message
                };
            }
        }
        public async Task<ResponseAPI> PayOSPayment(PaymentRequest req)
        {
            var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(predicate: x => x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending,
                     include: y => y.Include( a => a.PackageOrder));
            if (checkOrder == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            List<PayOSItems> items = new List<PayOSItems>(); //xiu xai
            //foreach (var orderDetail in checkOrder.OrderDetails)
            //{
            //    //if (!string.IsNullOrEmpty(orderDetail.ProductJson))
            //    //{
            //    //    //var products = JsonConvert.DeserializeObject<List<ProductFromPos>>(orderDetail.ProductJson);
            //    //    //var dynamicProducts = JsonConvert.DeserializeObject<List<dynamic>>(orderDetail.ProductJson);

            //    //    for (int i = 0; i < products.Count; i++)
            //    //    {
            //    //        var product = products[i];
            //    //        var dynamicProduct = dynamicProducts[i];
            //    //        int quantity = dynamicProduct.Quantity != null ? (int)dynamicProduct.Quantity : (orderDetail.Quantity ?? 1);

            //    //        items.Add(new PayOSItems
            //    //        {
            //    //            name = product.Name,
            //    //            quantity = quantity,
            //    //            price = product.Price   
            //    //        });
            //    //    }
            //    //}
            //}
            List<ItemData> itemDataList = items.Select(item => new ItemData(
             item.name,   // Mapping from PayOSItems model to ItemData of pay os's model
             item.quantity,
             item.price
             )).ToList();
            string InnvoiceIdInt = req.InvoiceId.Substring(3);

            if (req.Key != null && PaymentTypeHelper.allowedPaymentTypes.Contains(req.Key.Split('_')[0]) && req.Key.Split("_")[1] == checkOrder.InvoiceId)
            {

                var paymentDataChargeMoney = new PaymentData(
                     orderCode: Int64.Parse(InnvoiceIdInt),  // Bạn có thể tạo mã đơn hàng tại đây
                     amount: checkOrder.TotalAmount,
                     description: "đơn :" + req.InvoiceId,
                     items: itemDataList,
                     cancelUrl: "http://yourdomain.com/payment/cancel",  // URL khi thanh toán bị hủy
                                                                         //returnUrl: PayOSConfiguration.ReturnUrlCharge,
                      returnUrl: req.UrlDirect,
                     // URL khi thanh toán thành công
                     buyerName: checkOrder.PackageOrder.CusName.ToString().Trim(),//customerInfoEtag.EtagDetail.FullName,
                     buyerEmail: checkOrder.PackageOrder.CusEmail.ToString().Trim(), // very require email here!
                     buyerPhone: checkOrder.PackageOrder.PhoneNumber.ToString().Trim(),//customerInfoEtag.EtagDetail.PhoneNumber,
                     buyerAddress: "VegaCity",
                     expiredAt: (int)DateTime.UtcNow.AddMinutes(30).Subtract(new DateTime(1970, 1, 1)).TotalSeconds
                );

                // Gọi hàm createPaymentLink từ PayOS với đối tượng PaymentData
                var paymentUrlChargeMoney = await _payOs.createPaymentLink(paymentDataChargeMoney);

                //return Ok(new { Url = paymentUrl });
                return new ResponseAPI
                {
                    MessageResponse = PaymentMessage.PayOSPaymentSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = paymentUrlChargeMoney
                };
            }
            //if key null

                //here
                try
                {
                    var paymentData = new PaymentData(
                        orderCode: Int64.Parse(InnvoiceIdInt),  // Bạn có thể tạo mã đơn hàng tại đây
                        amount: checkOrder.TotalAmount,
                        description: "đơn hàng :" + req.InvoiceId,
                        items: itemDataList,
                        cancelUrl: "http://yourdomain.com/payment/cancel",  // URL khi thanh toán bị hủy
                        returnUrl: req.UrlDirect,  // URL khi thanh toán thành công
                        buyerName: null,//customerInfo.FullName.ToString(),//customerInfo.FullName.ToString(),
                        //buyerEmail: customerInfo.Email.ToString(), // very require email here!
                        buyerEmail: null,
                        buyerPhone: null,//customerInfo.PhoneNumber.ToString(),//,
                        buyerAddress: null,// customerInfo.Email.ToString(),
                        expiredAt: (int)DateTime.UtcNow.AddMinutes(30).Subtract(new DateTime(1970, 1, 1)).TotalSeconds
                    );

                    // Gọi hàm createPaymentLink từ PayOS với đối tượng PaymentData
                    var paymentUrl = await _payOs.createPaymentLink(paymentData);

                    //return Ok(new { Url = paymentUrl });
                    return new ResponseAPI
                    {
                        MessageResponse = PaymentMessage.PayOSPaymentSuccess,
                        StatusCode = HttpStatusCodes.OK,
                        Data = paymentUrl
                    };
                }
                catch (Exception ex)
                {
                    //string error = ErrorUtil.GetErrorString("Exception", ex.Message);
                    //return StatusCode(StatusCodes.Status500InternalServerError, error);
                    return new ResponseAPI
                    {
                        MessageResponse = PaymentMessage.PayOSPaymentFail + ex.Message,
                        StatusCode = HttpStatusCodes.BadRequest,
                        Data = null
                    };
                }          
        }
        public async Task<ResponseAPI> UpdatePayOSOrder(string code, string id, string status, string orderCode)
        {
            var invoiceId = orderCode.ToString();

            // Fetch the order and check if it's still pending
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                 (predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Pending,
                  include: detail => detail.Include(a => a.OrderDetails).Include(u => u.User).ThenInclude(w => w.Wallets)
                                           .Include(x => x.PackageOrder).ThenInclude(r => r.Wallets)
                                           .Include(s => s.PromotionOrders).ThenInclude(a => a.Promotion)
                                           .Include(a => a.Payments));
            var orderCompleted = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Completed);
            if (orderCompleted != null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PayOSConfiguration.ipnUrl + orderCompleted.Id // URL for client-side redirection
                };
            }
            // Update the order to 'Completed'
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
            (predicate: x => x.UserId == order.UserId)
            ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            //GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            //{
            //    StartDate = TimeUtils.GetCurrentSEATime(),
            //};
            List<Guid> res = new List<Guid>();
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    //productJson = item.ProductJson.Replace("[", "").Replace("]", "");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {

                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        //if (etagType != null)
                        //{
                        //    //generate etag
                        //    var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                        //    res = response.Data;
                        //}
                        //else if (package != null)
                        //{
                        //    //package.PackageETagTypeMappings.ToList().ForEach(async x =>
                        //    //{
                        //    //    var response = await _service.GenerateEtag(x.QuantityEtagType, x.EtagTypeId, reqGenerate);
                        //    //    res = response.Data;
                        //    //});
                        //    foreach (var mapping in package.PackageETagTypeMappings)
                        //    {
                        //        var response = await _service.GenerateEtag(mapping.QuantityEtagType, mapping.EtagTypeId, reqGenerate);
                        //        res = response.Data;
                        //    }
                        //}
                        //return new ResponseAPI
                        //{
                        //    MessageResponse = "Some Thing goes wrong, pakcage or etag type",
                        //    StatusCode = HttpStatusCodes.BadRequest
                        //};
                    }
                }
            }
            //transaction cashier payos
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
                Description = "Add" + order.SaleType + " Balance By " + order.Payments.SingleOrDefault().Name + " to cashier web: " + order.User.FullName,
            };
            await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

            order.User.Wallets.SingleOrDefault().BalanceHistory += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().Balance += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(order.User.Wallets.SingleOrDefault());

            //session update
            sessionUser.TotalQuantityOrder += 1;
            sessionUser.TotalCashReceive += order.TotalAmount;
            sessionUser.TotalFinalAmountOrder += order.TotalAmount;
            _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);
            // Commit the transaction
            var commitResult = await _unitOfWork.CommitAsync();

            return commitResult > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PayOSConfiguration.ipnUrl + order.Id // URL for client-side redirection
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = PaymentMessage.PayOSPaymentFail
                };
        }
        public async Task<ResponseAPI> UpdateOrderPaidOSForChargingMoney(string code, string id, string status, string orderCode)
        {
            try
            {
                var invoiceId = "VGC" + orderCode.ToString();
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.PackageOrder).ThenInclude(r => r.Wallets)
                                       .Include(s => s.PromotionOrders).ThenInclude(a => a.Promotion)
                                       .Include(a => a.Payments)
                                       .Include(o => o.OrderDetails));
                                       
                var orderCompleted = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Completed);
                if (orderCompleted != null)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.NoContent,
                        MessageResponse = PayOSConfiguration.ipnUrl + orderCompleted.Id // URL for client-side redirection
                    };
                }
                var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);

                if (order.SaleType == SaleType.PackageItemCharge)
                {
                    if (order.PackageOrder == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
                }
                if (order.Payments.SingleOrDefault().Name != PaymentTypeEnum.PayOS.GetDescriptionFromEnum())
                    throw new BadHttpRequestException("Payment is not PayOS", HttpStatusCodes.BadRequest);
                //bonus here
                int PromotionAmount = 0; //not use
                if (order.PromotionOrders.Count == 1)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
                    //payment
                    order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                    order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                    //quantity promotion -
                    //
                    //order.PromotionOrders.SingleOrDefault().Promotion.Quantity -= 1;
                    //_unitOfWork.GetRepository<Promotion>().UpdateAsync(order.PromotionOrders.SingleOrDefault().Promotion);
                    //session update
                    //
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());



                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentZaloPay.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
                else
                {

                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());



                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentMomo.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
            }
            catch (Exception ex)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = ex.Message
                };
            }
        }
        public async Task<ResponseAPI> ZaloPayPayment(PaymentRequest req)
        {
            var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                             (predicate: x => x.InvoiceId == req.InvoiceId
                                           && x.Status    == OrderStatus.Pending);
            if (checkOrder == null)
            {
                return new ResponseAPI
                {
                    StatusCode      = HttpStatusCodes.NotFound,
                    MessageResponse = PaymentMessage.OrderNotFound
                };
            }
            var embed_data = new {
                redirecturl = req.UrlDirect ?? PaymentZaloPay.redirectUrl,
            };
            var items = new List<Object>();
            //if (req.Key != null && PaymentTypeHelper.allowedPaymentTypes.Contains(req.Key.Split('_')[0]) && req.Key.Split("_")[1] == checkOrder.InvoiceId)
            if (req.Key != null && PaymentTypeHelper.allowedPaymentTypes.Contains(req.Key.Split('_')[0]))
            {
                var zaloReq = new ZaloPayRequest()
                {
                    app_id = PaymentZaloPay.app_id,
                    app_trans_id = TimeUtils.GetCurrentSEATime().ToString("yyMMdd") + "_" + req.InvoiceId,
                    app_user = PaymentZaloPay.app_user,
                    app_time = long.Parse(TimeUtils.GetTimeStamp().ToString()),
                    item = JsonConvert.SerializeObject(items),
                    embed_data = JsonConvert.SerializeObject(embed_data),
                    amount = checkOrder.TotalAmount,
                    description = "Thanh toán cho đơn hàng (InvoiceId):" + req.InvoiceId,
                    bank_code = "",
                };
                var data = zaloReq.app_id + "|" + zaloReq.app_trans_id + "|" + zaloReq.app_user + "|" + zaloReq.amount + "|"
                + zaloReq.app_time + "|" + zaloReq.embed_data + "|" + zaloReq.item;
                zaloReq.mac = PasswordUtil.getSignature(data, PaymentZaloPay.key1);

                var response = await CallApiUtils.CallApiEndpoint("https://sb-openapi.zalopay.vn/v2/create", zaloReq);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.InternalServerError,
                        MessageResponse = PaymentMessage.ZaloPayPaymentFail
                    };
                }
                var ZaloPayResponse = await CallApiUtils.GenerateObjectFromResponse<ZaloPayPaymentResponse>(response);
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = PaymentMessage.ZaloPayPaymentSuccess,
                    Data = ZaloPayResponse
                };
            }
            else
            {
                var zaloReq = new ZaloPayRequest()
                {
                    app_id = PaymentZaloPay.app_id,
                    app_trans_id = TimeUtils.GetCurrentSEATime().ToString("yyMMdd") + "_" + req.InvoiceId,
                    app_user = PaymentZaloPay.app_user,
                    app_time = long.Parse(TimeUtils.GetTimeStamp().ToString()),
                    item = JsonConvert.SerializeObject(items),
                    embed_data = JsonConvert.SerializeObject(embed_data),
                    amount = checkOrder.TotalAmount,
                    description = "Thanh toán cho đơn hàng (InvoiceId):" + req.InvoiceId,
                    bank_code = "",
                };
                var data = zaloReq.app_id + "|" + zaloReq.app_trans_id + "|" + zaloReq.app_user + "|" + zaloReq.amount + "|"
                + zaloReq.app_time + "|" + zaloReq.embed_data + "|" + zaloReq.item;
                zaloReq.mac = PasswordUtil.getSignature(data, PaymentZaloPay.key1);

                var response = await CallApiUtils.CallApiEndpoint("https://sb-openapi.zalopay.vn/v2/create", zaloReq);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.InternalServerError,
                        MessageResponse = PaymentMessage.ZaloPayPaymentFail
                    };
                }
                var ZaloPayResponse = await CallApiUtils.GenerateObjectFromResponse<ZaloPayPaymentResponse>(response);
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = PaymentMessage.MomoPaymentSuccess,
                    Data = ZaloPayResponse
                };
            }
        }
        public async Task<ResponseAPI> UpdateOrderPaid(IPNZaloPayRequest req)
        {
            string InvoiceId = req.apptransid.Split("_")[1];
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
               (predicate: x => x.InvoiceId == InvoiceId && x.Status == OrderStatus.Pending,
                include: detail => detail.Include(a => a.OrderDetails).Include(u => u.User).ThenInclude(w => w.Wallets).Include(z => z.Payments));
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
            (predicate: x => x.UserId == order.UserId)
            ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);
            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            //GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            //{
            //    StartDate = TimeUtils.GetCurrentSEATime(),
            //};
            List<Guid> res = new List<Guid>();
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    //productJson = item.ProductJson.Replace("[", "").Replace("]", "");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {

                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                       
                    }
                }
            }
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
                Description = "Add" + order.SaleType + " Balance By " + order.Payments.SingleOrDefault().Name + " to cashier web: " + order.User.FullName,
            };
            await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

            order.User.Wallets.SingleOrDefault().BalanceHistory += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().Balance += Int32.Parse(order.TotalAmount.ToString());
            order.User.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(order.User.Wallets.SingleOrDefault());

            //session update
            sessionUser.TotalQuantityOrder += 1;
            sessionUser.TotalCashReceive += order.TotalAmount;
            sessionUser.TotalFinalAmountOrder += order.TotalAmount;
            _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionUser);


            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PaymentZaloPay.ipnUrl + order.Id
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "https://vegacity.id.vn/order-status?status=failure"
                };
        }
        public async Task<ResponseAPI> UpdateOrderPaidForChargingMoney(IPNZaloPayRequest req)
        {
            try
            {
                string InvoiceId = req.apptransid.Split("_")[1];
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.InvoiceId == InvoiceId && x.Status == OrderStatus.Pending,
                include: order => order.Include(a => a.User).ThenInclude(b => b.Wallets)
                                       .Include(x => x.PackageOrder).ThenInclude(r => r.Wallets)
                                       .Include(s => s.PromotionOrders).ThenInclude(a => a.Promotion)
                                       .Include(a => a.Payments)
                                       .Include(o => o.OrderDetails));
                var sessionUser = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync
                    (predicate: x => x.UserId == order.UserId)
                    ?? throw new BadHttpRequestException("User session not found", HttpStatusCodes.NotFound);

                if (order.SaleType == SaleType.PackageItemCharge)
                {
                    if (order.PackageOrder == null) throw new BadHttpRequestException("Package item or sale type not found", HttpStatusCodes.NotFound);
                }
                if (order.Payments.SingleOrDefault().Name != PaymentTypeEnum.ZaloPay.GetDescriptionFromEnum())
                    throw new BadHttpRequestException("Payment is not ZaloPay", HttpStatusCodes.BadRequest);
                //bonus here
                int PromotionAmount = 0; //not use
                if (order.PromotionOrders.Count == 1)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
                    //payment
                    order.Payments.SingleOrDefault().Status = PaymentStatus.Completed;
                    order.Payments.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Payment>().UpdateAsync(order.Payments.SingleOrDefault());
                    //quantity promotion -
                    //
                    //order.PromotionOrders.SingleOrDefault().Promotion.Quantity -= 1;
                    //_unitOfWork.GetRepository<Promotion>().UpdateAsync(order.PromotionOrders.SingleOrDefault().Promotion);
                    //session update
                    //
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentZaloPay.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
                else
                {

                    order.Status = OrderStatus.Completed;
                    order.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                    //order.OrderDetails.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime(); //may add new Status for OrderDetail Below
                    //_unitOfWork.GetRepository<OrderDetail>().UpdateAsync(order.OrderDetails.SingleOrDefault());
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
                    var transactionCharge = await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().SingleOrDefaultAsync
                        (predicate: x => x.OrderId == order.Id)
                        ?? throw new BadHttpRequestException("Transaction charge not found", HttpStatusCodes.NotFound);
                    //create deposit
                    var deposit = new CustomerMoneyTransfer()
                    {
                        Id = Guid.NewGuid(),
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Amount = order.TotalAmount + order.PromotionOrders.SingleOrDefault().DiscountAmount,
                        MarketZoneId = order.User.MarketZoneId,
                        PackageOrderId = order.PackageOrderId,
                        IsIncrease = true,
                        Status = TransactionStatus.Success.GetDescriptionFromEnum(),
                        TransactionId = transactionCharge.Id,
                    };
                    await _unitOfWork.GetRepository<CustomerMoneyTransfer>().InsertAsync(deposit);
                    //update wallet package item
                    order.PackageOrder.Wallets.SingleOrDefault().Balance += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().BalanceHistory += order.TotalAmount;
                    order.PackageOrder.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(order.PackageOrder.Wallets.SingleOrDefault());

                    transactionCharge.Status = TransactionStatus.Success.GetDescriptionFromEnum();
                    transactionCharge.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().UpdateAsync(transactionCharge);

                    //bill cashier receive money from packageItem
                    var transactionCashierBalance = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionCashierBalance);

                    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                    if (marketZone == null) throw new BadHttpRequestException("Market zone not found", HttpStatusCodes.NotFound);
                    var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                        (predicate: x => x.Email == marketZone.Email && x.MarketZoneId == marketZone.Id, include: z => z.Include(w => w.Wallets));
                    if (admin == null) throw new BadHttpRequestException("Admin not found", HttpStatusCodes.NotFound);
                    var walletAdmin = admin.Wallets.FirstOrDefault();
                    //bill admin refund money to packageItem
                    var transactionAdminBalanceHistory = new VegaCityApp.Domain.Models.Transaction()
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
                    await _unitOfWork.GetRepository<VegaCityApp.Domain.Models.Transaction>().InsertAsync(transactionAdminBalanceHistory);


                    walletAdmin.BalanceHistory -= order.TotalAmount;
                    walletAdmin.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(walletAdmin);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.NoContent,
                            MessageResponse = PaymentMomo.ipnUrl + order.Id
                        }
                        : new ResponseAPI()
                        {
                            StatusCode = HttpStatusCodes.InternalServerError
                        };
                }
            }
            catch (Exception ex)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = ex.Message
                };
            }
        }
    }
}

﻿using AutoMapper;
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
using VegaCityApp.API.Payload.Request.Etag;

namespace VegaCityApp.API.Services.Implement
{
    public class PaymentService : BaseService<PaymentService>, IPaymentService
    {
        private readonly PayOS _payOs;
        private readonly IEtagService _service;
        public PaymentService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PaymentService> logger, PayOS payOs,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor, IEtagService service) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _payOs = payOs;
            _service = service;
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
                 include: detail => detail.Include(a => a.OrderDetails));
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            {
                StartDate = TimeUtils.GetCurrentSEATime(),
            };
            List<Guid> res = new List<Guid>();
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    productJson = item.ProductJson.Replace("[","").Replace("]","");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {
                        var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag, include: mapping => mapping.Include(z => z.PackageETagTypeMappings));
                        if (etagType != null)
                        {
                            //generate etag
                            var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                            res = response.Data;
                        }
                        else if (package != null)
                        {
                            package.PackageETagTypeMappings.ToList().ForEach(async x =>
                            {
                                var response = await _service.GenerateEtag(x.QuantityEtagType, x.EtagTypeId, reqGenerate);
                                res = response.Data;
                            });
                        }
                    }
                }
            }
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
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                    (predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending,
                     include: z => z.Include(a => a.User).ThenInclude(b => b.Wallets)); //..
                order.Status = OrderStatus.Completed;
                order.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                    (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet));
                //update wallet admin
                var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(
                    predicate: x => x.Id == order.User.MarketZoneId);//..
                var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                    predicate: x => x.Email == marketZone.Email, include: wallet => wallet.Include(z => z.Wallets));//..
                foreach(var item in admin.Wallets)
                {
                    item.BalanceHistory -= Int32.Parse(req.amount.ToString());
                    item.UpsDate = TimeUtils.GetCurrentSEATime();
                }
                foreach (var item in order.User.Wallets)
                {
                    item.UpsDate = TimeUtils.GetCurrentSEATime();
                    item.Balance += Int32.Parse(req.amount.ToString());
                }
                _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
                _unitOfWork.GetRepository<Wallet>().UpdateRange(order.User.Wallets);
                //..
                //update wallet
                etag.Wallet.Balance += Int32.Parse(req.amount.ToString());
                etag.Wallet.BalanceHistory += Int32.Parse(req.amount.ToString());
                etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);

                //new transaction here
                var newAdminTransaction = new VegaCityApp.Domain.Models.Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = Int32.Parse(req.amount.ToString()),
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
                    Amount = Int32.Parse(req.amount.ToString()),
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
                    PaymentType = "Momo",
                    Name = "Nạp tiền vào ETag với số tiền: " + req.amount,
                    IsIncrease = true, // Xác định rằng đây là nạp tiền
                    Amount = Int32.Parse(req.amount.ToString()),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    WalletId = etag.Wallet.Id,
                    EtagId = etag.Id,
                    OrderId = order.Id,
                };
                await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
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
                 include: detail => detail.Include(a => a.OrderDetails));
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            {
                StartDate = TimeUtils.GetCurrentSEATime(),
            };
            
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    productJson = item.ProductJson.Replace("[","").Replace("]","");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {
                        var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag, include: mapping => mapping.Include(z => z.PackageETagTypeMappings));
                        if (etagType != null)
                        {
                            //generate etag
                            var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                            listEtagCreated = response.Data;
                        }
                        else if (package != null)
                        {
                            foreach(var itemm in package.PackageETagTypeMappings)
                            {
                                var response = await _service.GenerateEtag(itemm.QuantityEtagType, itemm.EtagTypeId, reqGenerate);
                                listEtagCreated = response.Data;
                            }
                        }
                    }
                }
            }
            string data = EnCodeBase64.EncodeBase64<List<Guid>>(listEtagCreated);
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
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
                var orderInvoiceId = req.vnp_OrderInfo.Split(":", 2)[1];
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                    (predicate: x => x.InvoiceId == orderInvoiceId && x.Status == OrderStatus.Pending
                    , include: z => z.Include(a => a.User).ThenInclude(b => b.Wallets));//  find user and wallet vnpay
                int trimmedAmount = req.vnp_Amount / 100; //remove exceed 00 o day
                order.Status = OrderStatus.Completed;
                order.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);

                var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                    (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet).Include(b => b.EtagType));

                //bonus 
                decimal? bonusRate = etag.EtagType.BonusRate;
                decimal bonus = (bonusRate.HasValue ? bonusRate.Value : 0) * trimmedAmount; // Tính toán bonus chính xác

                //admin wallet part vnpay
                var marketzone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
                var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Email == marketzone.Email, include: wallet => wallet.Include(z => z.Wallets));

                foreach (var item in admin.Wallets)
                {
                    item.BalanceHistory -= trimmedAmount;
                    item.UpsDate = TimeUtils.GetCurrentSEATime();
                }

                foreach (var item in order.User.Wallets)
                {
                    // Chuyển đổi bonus từ decimal về int và tính toán
                    item.Balance += trimmedAmount + (int)bonus;
                    item.UpsDate = TimeUtils.GetCurrentSEATime();
                }

                _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
                _unitOfWork.GetRepository<Wallet>().UpdateRange(order.User.Wallets);

                //update wallet
                etag.Wallet.Balance += trimmedAmount + (int)bonus;
                etag.Wallet.BalanceHistory += trimmedAmount + (int)bonus;
                etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
                //transactions here 
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
                    PaymentType = "VnPay",
                    Name = "Nạp tiền vào ETag với số tiền: " + order.TotalAmount,
                    IsIncrease = true, // Xác định rằng đây là nạp tiền
                    Amount = trimmedAmount,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    WalletId = etag.Wallet.Id,
                    EtagId = etag.Id,
                    OrderId = order.Id,
                };

                await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);

                return await _unitOfWork.CommitAsync() > 0
                    ? new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.NoContent,
                        MessageResponse = VnPayConfig.ipnUrl + order.Id
                    }
                    : new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.InternalServerError
                    };
            }
            catch (Exception ex)
            {
                return new ResponseAPI()
                {
                    MessageResponse = ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError
                };
            }
        }
        public async Task<ResponseAPI> PayOSPayment(PaymentRequest req)
        {
            var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(predicate: x => x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending);
            if (checkOrder == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundOrder,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            List<PayOSItems> items = new List<PayOSItems>(); //xiu xai
            foreach (var orderDetail in checkOrder.OrderDetails)
            {
                if (!string.IsNullOrEmpty(orderDetail.ProductJson))
                {
                    var products = JsonConvert.DeserializeObject<List<ProductFromPos>>(orderDetail.ProductJson);
                    var dynamicProducts = JsonConvert.DeserializeObject<List<dynamic>>(orderDetail.ProductJson);

                    for (int i = 0; i < products.Count; i++)
                    {
                        var product = products[i];
                        var dynamicProduct = dynamicProducts[i];
                        int quantity = dynamicProduct.Quantity != null ? (int)dynamicProduct.Quantity : (orderDetail.Quantity ?? 1);

                        items.Add(new PayOSItems
                        {
                            name = product.Name,
                            quantity = quantity,
                            price = product.Price
                        });
                    }
                }
            }
            List<ItemData> itemDataList = items.Select(item => new ItemData(
             item.name,   // Mapping from PayOSItems model to ItemData of pay os's model
             item.quantity,
             item.price
             )).ToList();
            if(req.Key != null && PaymentTypeHelper.allowedPaymentTypes.Contains(req.Key.Split('_')[0]) && req.Key.Split("_")[1] == checkOrder.InvoiceId)
            {
                var customerInfoEtag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == checkOrder.EtagId,
                include: etag => etag.Include(o => o.EtagDetails));
                var paymentDataChargeMoney = new PaymentData(
                    orderCode: Int64.Parse(checkOrder.InvoiceId.ToString()),  // Bạn có thể tạo mã đơn hàng tại đây
                    amount: checkOrder.TotalAmount,
                    description: "đơn hàng :" + req.InvoiceId,
                    items: itemDataList,
                    cancelUrl: "http://yourdomain.com/payment/cancel",  // URL khi thanh toán bị hủy
                    //returnUrl: PayOSConfiguration.ReturnUrlCharge,
                     returnUrl: PayOSConfiguration.ReturnUrlCharge,
                    // URL khi thanh toán thành công
                    buyerName: "Nguyen Van A",//customerInfoEtag.EtagDetails.SingleOrDefault().FullName.ToString(),
                    buyerEmail: "", // very require email here!
                    buyerPhone: "0909998888",//customerInfoEtag.EtagDetails.SingleOrDefault().PhoneNumber.ToString(),
                    buyerAddress: "",
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
                var customerInfo = JsonConvert.DeserializeObject<VegaCityApp.API.Payload.Request.Payment.CustomerInfo>(checkOrder.CustomerInfo);//xiu xai
                //here
                try
                {
                    var paymentData = new PaymentData(
                        orderCode: Int64.Parse(checkOrder.InvoiceId.ToString()),  // Bạn có thể tạo mã đơn hàng tại đây
                        amount: checkOrder.TotalAmount,
                        description: "đơn hàng :" + req.InvoiceId,
                        items: itemDataList,
                        cancelUrl: "http://yourdomain.com/payment/cancel",  // URL khi thanh toán bị hủy
                        returnUrl: PayOSConfiguration.ReturnUrl,  // URL khi thanh toán thành công
                        buyerName: customerInfo.FullName.ToString(),//customerInfo.FullName.ToString(),
                        //buyerEmail: customerInfo.Email.ToString(), // very require email here!
                        buyerEmail: "",
                        buyerPhone: customerInfo.PhoneNumber.ToString(),//,
                        buyerAddress: "",// customerInfo.Email.ToString(),
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
                  include: detail => detail.Include(a => a.OrderDetails));
            // Update the order to 'Completed'
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            {
                StartDate = TimeUtils.GetCurrentSEATime(),
            };
            List<Guid> res = new List<Guid>();
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    productJson = item.ProductJson.Replace("[", "").Replace("]", "");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {
                        var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag, include: mapping => mapping.Include(z => z.PackageETagTypeMappings));
                        if (etagType != null)
                        {
                            //generate etag
                            var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                            res = response.Data;
                        }
                        else if (package != null)
                        {
                            package.PackageETagTypeMappings.ToList().ForEach(async x =>
                            {
                                var response = await _service.GenerateEtag(x.QuantityEtagType, x.EtagTypeId, reqGenerate);
                                res = response.Data;
                            });
                        }
                    }
                }
            }


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
            //var orderInvoiceId = req.vnp_OrderInfo.Split(":", 2)[1];
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == orderCode && x.Status == OrderStatus.Pending,
                include: z => z.Include(a => a.User).ThenInclude(b => b.Wallets)); //
            var orderCompleted = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
               (predicate: x => x.InvoiceId == orderCode && x.Status == OrderStatus.Completed);                                                                     //from here
            if (order == null || order.Status == OrderStatus.Completed)
            {
                // If the order doesn't exist or is already processed, return not found
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    //MessageResponse = "https://vegacity.id.vn/user/order-status?status=failure"
                    MessageResponse = PayOSConfiguration.ipnUrl + orderCompleted.Id

                };
            }
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet).Include(b => b.EtagType));
            //bonus 
            decimal? bonusRate = etag.EtagType.BonusRate;
            decimal bonus = (bonusRate.HasValue ? bonusRate.Value : 0) * order.TotalAmount;
            //admin wallet stuff
            var marketzone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == order.User.MarketZoneId);
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Email == marketzone.Email, include: wallet => wallet.Include(z => z.Wallets)); //
            foreach (var item in admin.Wallets)
            {
                item.BalanceHistory -= Int32.Parse((order.TotalAmount.ToString()));
                item.UpsDate = TimeUtils.GetCurrentSEATime();
            }
            foreach (var item in order.User.Wallets)
            {
                item.Balance += order.TotalAmount + (int)bonus;
                item.UpsDate = TimeUtils.GetCurrentSEATime();
            }
            _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
            _unitOfWork.GetRepository<Wallet>().UpdateRange(order.User.Wallets);
            //..
            
            //update wallet
            etag.Wallet.Balance += order.TotalAmount + (int)bonus;
            etag.Wallet.BalanceHistory += order.TotalAmount + (int)bonus;
            etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
            //create deposite
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
            var newDeposit = new Deposit
            {
                Id = Guid.NewGuid(), // Tạo ID mới
                PaymentType = "PayOS",
                Name = "Nạp tiền vào ETag với số tiền: " + order.TotalAmount,
                IsIncrease = true, // Xác định rằng đây là nạp tiền
                Amount = Int32.Parse(order.TotalAmount.ToString()),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                WalletId = etag.Wallet.Id,
                EtagId = etag.Id,
                OrderId = order.Id,
            };
            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
            return await _unitOfWork.CommitAsync() > 0
            ? new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.NoContent,
                MessageResponse = PayOSConfiguration.ipnUrl + order.Id
            }
            : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.InternalServerError
            };
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
            if (req.Key != null && PaymentTypeHelper.allowedPaymentTypes.Contains(req.Key.Split('_')[0]) && req.Key.Split("_")[1] == checkOrder.InvoiceId)
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
                include: detail => detail.Include(a => a.OrderDetails));
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

            //check id? etagtype or package
            string productJson = null;
            OrderProductFromCashierRequest productData = null;
            GenerateEtagRequest reqGenerate = new GenerateEtagRequest()
            {
                StartDate = TimeUtils.GetCurrentSEATime(),
            };
            List<Guid> res = new List<Guid>();
            if (order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    productJson = item.ProductJson.Replace("[", "").Replace("]", "");
                    productData = JsonConvert.DeserializeObject<OrderProductFromCashierRequest>(productJson);
                    if (productData != null)
                    {
                        var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag);
                        var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                            (predicate: x => x.Id == Guid.Parse(productData.Id) && !x.Deflag, include: mapping => mapping.Include(z => z.PackageETagTypeMappings));
                        if (etagType != null)
                        {
                            //generate etag
                            var response = await _service.GenerateEtag(productData.Quantity, etagType.Id, reqGenerate);
                            res = response.Data;
                        }
                        else if (package != null)
                        {
                            package.PackageETagTypeMappings.ToList().ForEach(async x =>
                            {
                                var response = await _service.GenerateEtag(x.QuantityEtagType, x.EtagTypeId, reqGenerate);
                                res = response.Data;
                            });
                        }
                    }
                }
            }

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
                var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                    (predicate: x => x.InvoiceId == InvoiceId && x.Status == OrderStatus.Pending,
                     include: z => z.Include(a => a.User).ThenInclude(b => b.Wallets)); //..
                order.Status = OrderStatus.Completed;
                order.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                    (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet));
                //update wallet admin
                var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(
                    predicate: x => x.Id == order.User.MarketZoneId);//..
                var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                    predicate: x => x.Email == marketZone.Email, include: wallet => wallet.Include(z => z.Wallets));//..
                foreach (var item in admin.Wallets)
                {
                    item.BalanceHistory -= Int32.Parse(req.amount.ToString());
                    item.UpsDate = TimeUtils.GetCurrentSEATime();
                }
                foreach (var item in order.User.Wallets)
                {
                    item.UpsDate = TimeUtils.GetCurrentSEATime();
                    item.Balance += Int32.Parse(req.amount.ToString());
                }
                _unitOfWork.GetRepository<Wallet>().UpdateRange(admin.Wallets);
                _unitOfWork.GetRepository<Wallet>().UpdateRange(order.User.Wallets);
                //..
                //update wallet
                etag.Wallet.Balance += Int32.Parse(req.amount.ToString());
                etag.Wallet.BalanceHistory += Int32.Parse(req.amount.ToString());
                etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
                //transactions here 
                var newAdminTransaction = new VegaCityApp.Domain.Models.Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = Int32.Parse(req.amount.ToString()),
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
                    Amount = Int32.Parse(req.amount.ToString()),
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
                    PaymentType = "ZaloPay",
                    Name = "Nạp tiền vào ETag với số tiền: " + req.amount,
                    IsIncrease = true, // Xác định rằng đây là nạp tiền
                    Amount = Int32.Parse(req.amount.ToString()),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    WalletId = etag.Wallet.Id,
                    EtagId = etag.Id,
                    OrderId = order.Id,
                };
                await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
                return await _unitOfWork.CommitAsync() > 0
                    ? new ResponseAPI()
                    {
                        StatusCode      = HttpStatusCodes.NoContent,
                        MessageResponse = PaymentZaloPay.ipnUrl + order.Id
                    }
                    : new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.InternalServerError
                    };
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

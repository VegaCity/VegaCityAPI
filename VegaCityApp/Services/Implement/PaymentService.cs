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
                                        (predicate: x => x.InvoiceId == request.InvoiceId && x.Status == OrderStatus.Pending);
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
                    if (request.Key.Split("_")[0] == "Momo")
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
        public async Task<ResponseAPI> UpdateOrderPaid(IPNMomoRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
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
        public async Task<ResponseAPI> UpdateOrderPaidForChargingMoney(IPNMomoRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet));
            //update wallet
            etag.Wallet.Balance += Int32.Parse(req.amount.ToString());
            etag.Wallet.BalanceHistory += Int32.Parse(req.amount.ToString());
            etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
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

            if (req.Key != null && req.Key.Split('_')[0] == "vnpay")
            {
                try
                {
                   
                        var tickCharge = TimeUtils.GetCurrentSEATime().ToString();
                        var vnpayCharge = new VnPayLibrary();
                        vnpayCharge.AddRequestData("vnp_Version", VnPayConfig.Version);
                        vnpayCharge.AddRequestData("vnp_Command", VnPayConfig.Command);
                        vnpayCharge.AddRequestData("vnp_TmnCode", VnPayConfig.TmnCode);
                        vnpayCharge.AddRequestData("vnp_Amount", (orderExisted.TotalAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 
                        vnpayCharge.AddRequestData("vnp_CreateDate", TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()));
                        vnpayCharge.AddRequestData("vnp_CurrCode", VnPayConfig.CurrCode);
                        vnpayCharge.AddRequestData("vnp_IpAddr", VnPayUtils.GetIpAddress(context));
                        vnpayCharge.AddRequestData("vnp_Locale", VnPayConfig.Locale);
                        vnpayCharge.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng (InvoiceId):" + req.InvoiceId);
                        vnpayCharge.AddRequestData("vnp_OrderType", "other"); //default value: other
                        vnpayCharge.AddRequestData("vnp_ReturnUrl", VnPayConfig.VnPaymentBackReturnUrlChargeMoney);
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
            vnpay.AddRequestData("vnp_Amount", (orderExisted.TotalAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

            vnpay.AddRequestData("vnp_CreateDate", TimeUtils.GetTimestamp(TimeUtils.GetCurrentSEATime()));
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
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == invoiceId[1] && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
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
        public async Task<ResponseAPI> UpdateOrderPaidForChargingMoney(VnPayPaymentResponse req)
        {
            var orderInvoiceId = req.vnp_OrderInfo.Split(":", 2)[1];
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == orderInvoiceId && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet));
            //update wallet
            etag.Wallet.Balance += Int32.Parse(req.vnp_Amount.ToString());
            etag.Wallet.BalanceHistory += Int32.Parse(req.vnp_Amount.ToString());
            etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
            //create deposite
            var newDeposit = new Deposit
            {
                Id = Guid.NewGuid(), // Tạo ID mới
                PaymentType = "VnPay",
                Name = "Nạp tiền vào ETag với số tiền: " + req.vnp_Amount,
                IsIncrease = true, // Xác định rằng đây là nạp tiền
                Amount = Int32.Parse(req.vnp_Amount.ToString()),
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
            if (checkOrder.CustomerInfo != null) //package , item sell without customer infor from etag
            {
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
                        buyerName: customerInfo.FullName.ToString(),
                        //buyerEmail: customerInfo.Email.ToString(), // very require email here!
                        buyerEmail:"",
                        buyerPhone: customerInfo.PhoneNumber.ToString(),
                        buyerAddress:"",// customerInfo.Email.ToString(),
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
            if(req.Key != null && req.Key.Split('_')[0] == "payos")
            {
                var customerInfoEtag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == checkOrder.EtagId);
                var paymentDataChargeMoney = new PaymentData(
                    orderCode: Int64.Parse(checkOrder.InvoiceId.ToString()),  // Bạn có thể tạo mã đơn hàng tại đây
                    amount: checkOrder.TotalAmount,
                    description: "đơn hàng :" + req.InvoiceId,
                    items: itemDataList,
                    cancelUrl: "http://yourdomain.com/payment/cancel",  // URL khi thanh toán bị hủy
                    returnUrl: PayOSConfiguration.ReturnUrlCharge,  // URL khi thanh toán thành công
                    buyerName: customerInfoEtag.FullName.ToString(),
                    buyerEmail: "", // very require email here!
                    buyerPhone: customerInfoEtag.PhoneNumber.ToString(),
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
            return new ResponseAPI
            {
                MessageResponse = PaymentMessage.PayOSPaymentFail,
                StatusCode = HttpStatusCodes.BadRequest,
            };

        }
        public async Task<ResponseAPI> UpdatePayOSOrder(string code, string id, string status, string orderCode)
        {
            var invoiceId = orderCode.ToString();

            // Fetch the order and check if it's still pending
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
               predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Pending);
            if (order == null || order.Status == OrderStatus.Completed)
            {
                // If the order doesn't exist or is already processed, return not found
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = "https://vegacity.id.vn/user/order-status?status=failure"
                };
            }
            // Update the order to 'Completed'
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);

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
                (predicate: x => x.InvoiceId == orderCode && x.Status == OrderStatus.Pending);
            if (order == null || order.Status == OrderStatus.Completed)
            {
                // If the order doesn't exist or is already processed, return not found
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = "https://vegacity.id.vn/user/order-status?status=failure"
                };
            }
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet));
            //update wallet
            etag.Wallet.Balance += Int32.Parse(order.TotalAmount.ToString());
            etag.Wallet.BalanceHistory += Int32.Parse(order.TotalAmount.ToString());
            etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
            //create deposite
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

    }


}

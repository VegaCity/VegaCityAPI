using AutoMapper;
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
            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == req.StoreId && !x.Deflag && x.Status ==(int) StoreStatusEnum.Opened);
            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagId && !x.Deflag);
            string json = JsonConvert.SerializeObject(req.ProductData);
            var newOrder = new Order()
            {
                Id = Guid.NewGuid(),
                PaymentType = req.PaymentType,
                StoreId = store.Id,
                EtagId = etag != null? etag.Id : null,
                Name = "Order From Pos: " + TimeUtils.GetCurrentSEATime(),
                TotalAmount = req.TotalAmount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = OrderStatus.Pending,
                InvoiceId = req.InvoiceId,
                Vatrate = (double)EnvironmentVariableConstant.VATRate / 100,
                ProductJson = json//
            };
            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);

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

        public async Task<IPaginate<GetOrderResponse>> SearchAllOrders(int size, int page)
        {
            var orders = await _unitOfWork.GetRepository<Order>().GetPagingListAsync(
                selector: x => new GetOrderResponse()
                {
                    Id = x.Id,
                    PaymentType = x.PaymentType,
                    Name = x.Name,
                    TotalAmount = x.TotalAmount,
                    CrDate = x.CrDate,
                    Status = x.Status,
                    InvoiceId = x.InvoiceId,
                    Vatrate = x.Vatrate,
                    ProductJson = x.ProductJson,
                    StoreId = x.StoreId,
                    EtagId = x.EtagId,
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name));
            return orders;
        }


        public async Task<ResponseAPI> UpdateOrder(string InvoiceId, UpdateOrderRequest req)
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

            if (order.Status == OrderStatus.Completed)
            {
                return new ResponseAPI()
                {
                    MessageResponse = OrderMessage.OrderCompleted,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            order.ProductJson = JsonConvert.SerializeObject(req.NewProducts);
            order.TotalAmount = req.TotalAmount;
            order.PaymentType = req.PaymentType;
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync(predicate: x => x.Id == req.EtagId && !x.Deflag);
            order.EtagId = etag != null ? etag.Id : null;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
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

        public async Task<ResponseAPI> SearchOrder(Guid OrderId)
        {
            var orderExist = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.Id == OrderId && x.Status != OrderStatus.Canceled,
                include: order => order.Include(o => o.Etag)
                    .Include(o => o.Store)
                    .Include(o => o.Deposits));
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
                Data = orderExist
            };
        }
    }

    
}

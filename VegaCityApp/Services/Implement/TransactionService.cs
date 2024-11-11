using AutoMapper;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.API.Payload.Response.TransactionResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class TransactionService : BaseService<TransactionService>, ITransactionService
    {
        public TransactionService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<TransactionService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> DeleteTransaction(Guid id)
        {
            var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (transaction == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Transaction not found"
                };
            }
            _unitOfWork.GetRepository<Transaction>().DeleteAsync(transaction);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI() {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Delete transaction successfully"
            };
        }

        public async Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllTransaction(int size, int page)
        {
            try
            {
                IPaginate<TransactionResponse> data = await _unitOfWork.GetRepository<Transaction>().GetPagingListAsync(
                               selector: x => new TransactionResponse()
                               {
                                   Id = x.Id,
                                   Amount = x.Amount,
                                   Description = x.Description,
                                   CrDate = x.CrDate,
                                   Currency = x.Currency,
                                   Status = x.Status,
                                   IsIncrease = x.IsIncrease,
                                   StoreId = x.StoreId,
                                   Type = x.Type,
                                   WalletId = x.WalletId
                               },
                                page: page,
                                size: size,
                                orderBy: x => x.OrderByDescending(z => z.CrDate));
                return new ResponseAPI<IEnumerable<TransactionResponse>>
                {
                    MessageResponse = "Get Transactions success !!",
                    StatusCode = HttpStatusCodes.OK,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<TransactionResponse>>
                {
                    MessageResponse = "Get Transactions fail" + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

        public async Task<ResponseAPI> GetTransactionById(Guid id)
        {
            var transaction = await _unitOfWork.GetRepository<Transaction>().SingleOrDefaultAsync(
                predicate: x => x.Id == id
                );
            if (transaction == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Transaction not found"
                };
            }
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Get transaction successfully",
            };
        }
    }
}

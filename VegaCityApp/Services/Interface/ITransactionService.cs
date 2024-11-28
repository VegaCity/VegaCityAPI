using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.TransactionResponse;

namespace VegaCityApp.API.Services.Interface
{
    public interface ITransactionService
    {
        Task<ResponseAPI> DeleteTransaction(Guid id);
        Task<ResponseAPI> GetTransactionById(Guid id);
        Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllTransaction(int size, int page);
        Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllTransactionByStoreId(Guid storeId, string type, int size, int page);
        Task<ResponseAPI<IEnumerable<StoreMoneyTransferRes>>> GetAllStoreMoneyTransfer(Guid storeId, int size, int page);
        Task<ResponseAPI<IEnumerable<CustomerMoneyTransferRes>>> GetAllCustomerMoneyTransfer(Guid PackageOrderId, int size, int page);
        Task CheckTransactionPending();
    }
}

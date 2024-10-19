using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.TransactionResponse;

namespace VegaCityApp.API.Services.Interface
{
    public interface ITransactionService
    {
        Task<ResponseAPI> DeleteTransaction(Guid id);
        Task<ResponseAPI> GetTransactionById(Guid id);
        Task<ResponseAPI<IEnumerable<TransactionResponse>>> GetAllTransaction(int size, int page);
    }
}

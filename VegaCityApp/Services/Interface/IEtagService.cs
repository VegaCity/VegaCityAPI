using VegaCityApp.API.Payload.Request.Etag;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IEtagService
    {
        Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreateEtag(EtagRequest req);
        Task<ResponseAPI> UpdateEtagType(Guid etagTypeId,UpdateEtagTypeRequest req);
        Task<ResponseAPI> DeleteEtagType(Guid etagTypeId);
        Task<ResponseAPI> SearchEtagType(Guid etagTypeId);
        Task<ResponseAPI<List<Guid>>> GenerateEtag(int quantity, Guid etagTypeId, GenerateEtagRequest req);
        Task<ResponseAPI> UpdateEtag(Guid etagId, UpdateEtagRequest req);
        Task<ResponseAPI> DeleteEtag(Guid etagId);
        Task<ResponseAPI> SearchEtag(Guid? etagId, string? etagCode);
        Task<ResponseAPI<IEnumerable<EtagResponse>>> SearchAllEtag(int size, int page);
        Task<ResponseAPI<IEnumerable<EtagTypeResponse>>> SearchAllEtagType(int size, int page);
        Task<ResponseAPI> AddEtagTypeToPackage(Guid etagId, Guid packageId, int quantityEtagType);
        Task<ResponseAPI> RemoveEtagTypeFromPackage(Guid etagId, Guid packageId);
        Task<ResponseAPI> ActivateEtag(Guid etagId, ActivateEtagRequest req);
        Task<ResponseAPI> PrepareChargeMoneyEtag(ChargeMoneyEtagRequest req);
        Task CheckEtagExpire();
        Task<ResponseAPI> EtagPayment( string etagCode, int amount);
    }
}

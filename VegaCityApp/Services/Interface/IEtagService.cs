﻿using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IEtagService
    {
        Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreateEtag(EtagRequest req);
        Task<ResponseAPI> UpdateEtagType(UpdateEtagTypeRequest req);
        Task<ResponseAPI> DeleteEtagType(Guid etagTypeId);
        Task<ResponseAPI> SearchEtagType(Guid etagTypeId);
        Task<IPaginate<EtagTypeResponse>> SearchAllEtagType(int size, int page);
    }
}

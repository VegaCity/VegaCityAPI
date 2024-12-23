﻿using VegaCityApp.API.Payload.Request.Zone;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.GetZoneResponse;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IZoneService
    {
        Task<ResponseAPI> CreateZone(CreateZoneRequest req);
        Task<ResponseAPI> UpdateZone(Guid ZoneId, UpdateZoneRequest req);
        Task<ResponseAPI<IEnumerable<GetZoneResponse>>> SearchZones(int size, int page);
        Task<ResponseAPI<Zone>> SearchZone(Guid ZoneId);
        Task<ResponseAPI> DeleteZone(Guid ZoneId);
    }
}

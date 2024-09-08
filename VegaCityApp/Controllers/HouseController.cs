﻿using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.House;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.HouseResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.Domain.Paginate;
using static VegaCityApp.API.Constants.ApiEndPointConstant;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Controllers
{
    [ApiController]
    public class HouseController : BaseController<HouseController>
    {
        private readonly IHouseService _service;

        public HouseController(ILogger<HouseController> logger, IHouseService service) : base(logger)
        {
            _service = service;
        }
        [HttpPost(HouseEndpoint.CreateHouse)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.Created)]
        public async Task<IActionResult> CreateHouse([FromBody] CreateHouseRequest req)
        {
            var result = await _service.CreateHouse(req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch(HouseEndpoint.UpdateHouse)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateHouse(Guid houseId, [FromBody] UpdateHouseRequest req)
        {
            var result = await _service.UpdateHouse(houseId, req);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet(HouseEndpoint.GetListHouse)]
        [ProducesResponseType(typeof(IPaginate<GetHouseResponse>), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchAllHouse(int size, int page)
        {
            var result = await _service.SearchAllHouse(size, page);
            return Ok(result);
        }
        [HttpGet(HouseEndpoint.GetHouse)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> SearchHouse(Guid id)
        {
            var result = await _service.SearchHouse(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete(HouseEndpoint.DeleteHouse)]
        [ProducesResponseType(typeof(ResponseAPI), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteHouse(Guid id)
        {
            var result = await _service.DeleteHouse(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
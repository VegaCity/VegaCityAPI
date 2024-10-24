﻿using VegaCityApp.API.Payload.Request.WalletType;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IWalletTypeService
    {
        Task<ResponseAPI> CreateWalletType(WalletTypeRequest walletTypeRequest);
        Task<ResponseAPI> UpdateWalletType(Guid Id, UpDateWalletTypeRequest walletTypeRequest);
        Task<ResponseAPI> DeleteWalletType(Guid id);
        Task<ResponseAPI> GetWalletTypeById(Guid id);
        Task<ResponseAPI<IEnumerable<WalletTypeResponse>>> GetAllWalletType(int size, int page);
        Task<ResponseAPI> AddServiceStoreToWalletType(Guid id, Guid serviceStoreId);
        Task<ResponseAPI> RemoveServiceStoreToWalletType(Guid id, Guid serviceStoreId);
        Task CheckExpireWallet();
        Task EndDayCheckWalletCashier(Guid apiKey);
        Task<ResponseAPI> RequestWithdrawMoneyWallet(Guid id, WithdrawMoneyRequest request);
        Task<ResponseAPI> WithdrawMoneyWallet(Guid id, Transaction transaction);
    }
}

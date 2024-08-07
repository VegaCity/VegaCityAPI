using AutoMapper;
using Pos_System.API.Constants;
using System;
using System.Net;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Payload.Request;
using VegaCityApp.Payload.Response;
using VegaCityApp.Repository.Interfaces;
using VegaCityApp.Service.Interface;
using static Pos_System.API.Constants.MessageConstant;

namespace VegaCityApp.Service.Implement
{
    public class AccountService : BaseService<AccountService>, IAccountService
    {
        public AccountService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<AccountService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        //not done yet
        public async Task<CreateAccountResponse> CreateAccount(CreateAccountRequest req)
        {
            var response = new CreateAccountResponse();
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = req.FullName,
                    PhoneNumber = req.PhoneNumber,
                    Email = req.Email,
                    Birthday = req.Birthday,
                    Gender = req.Gender,
                    Cccd = req.Cccd,
                    ImageUrl = req.ImageUrl,
                    MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                };
                var account = new Account
                {
                    Id = Guid.NewGuid(),
                    Email = req.Email,
                    Password = PasswordUtil.HashPassword(req.Password),
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime(),
                    Status = "ACTIVE",
                    
                };
                //await _unitOfWork.GetRepository<Account>().AddAsync(account);
                //await _unitOfWork.SaveChangesAsync();
                response.StatusCode = 200;
                response.MessageResponse = "Create account successfully";
                response.UserId = account.Id;
                response.AccountId = account.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create account failed");
                response.StatusCode = 500;
                response.MessageResponse = "Create account failed";
            }
            return response;
        }   

        public async Task<CreateWalletTypeResponse> CreateWalletType(WalletTypeRequest req)
        {
            var walletType = new WalletType()
            {
                Id = Guid.NewGuid(),
                BonusRate = req.BonusRate,
                CrDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
                Name = req.WalletTypeName,
                UpsDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<WalletType>().InsertAsync(walletType);
            var response = new CreateWalletTypeResponse()
            {
                Message = WalletTypeMessage.CreateSuccessFully,
                StatusCode = HttpStatusCodes.Created,
                WalletTypeId = walletType.Id,
            };
            int check = await _unitOfWork.CommitAsync();

            return check > 0 ? response : new CreateWalletTypeResponse()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                Message = WalletTypeMessage.CreateFail
            };
        }
        public async Task<CreateWalletTypeResponse> DeleteWalletType(Guid WalleTypeId)
        {
            var wallet = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(predicate: x => x.Id == WalleTypeId);
            if(wallet == null)
            {
                return new CreateWalletTypeResponse()
                {
                    Message = WalletTypeMessage.NotFoundWalletType,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            wallet.Deflag = true;
            _unitOfWork.GetRepository<WalletType>().UpdateAsync(wallet);
            await _unitOfWork.CommitAsync();
            return new CreateWalletTypeResponse()
            {
                Message = WalletTypeMessage.DeleteWalletTypeSuccessfully,
                StatusCode = HttpStatusCodes.OK,
                WalletTypeId = WalleTypeId
            };
        }
    }
}

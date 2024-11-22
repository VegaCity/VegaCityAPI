using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class MarketZoneService : BaseService<MarketZoneService>, IMarketZoneService
    {
        private readonly IAccountService _account;

        public MarketZoneService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<MarketZoneService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAccountService account) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _account = account;
        }
        public async Task<ResponseAPI> UpdateMarketZone(MarketZoneRequest request)
        {
            var marketZone = await GetMarketZone(request.Id);
            marketZone.Data.Name = request.Name ?? marketZone.Data.Name;
            marketZone.Data.Address = request.Address ?? marketZone.Data.Address;
            marketZone.Data.Location = request.Location ?? marketZone.Data.Location;
            marketZone.Data.ImageUrl = request.ImageUrl ?? marketZone.Data.ImageUrl;
            marketZone.Data.Description = request.Description ?? marketZone.Data.Description;
            marketZone.Data.ShortName = request.ShortName ?? marketZone.Data.ShortName;
            _unitOfWork.GetRepository<MarketZone>().UpdateAsync(marketZone.Data);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Update market zone successfully",
                    Data = marketZone.Data.Id
                }
                : new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "Update market zone failed"
                };
        }
        public async Task<ResponseAPI> DeleteMarketZone(Guid id)
        {
            var marketZone = await GetMarketZone(id);
            marketZone.Data.Deflag = true;
            _unitOfWork.GetRepository<MarketZone>().UpdateAsync(marketZone.Data);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Delete market zone successfully",
                    Data = marketZone.Data.Id
                }
                : new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "Delete market zone failed"
                };
        }
        public async Task<ResponseAPI<MarketZone>> GetMarketZone(Guid id)
        {
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == id && !x.Deflag)
                ?? throw new BadHttpRequestException("Market zone not found");
            return new ResponseAPI<MarketZone>
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Get market zone successfully",
                Data = marketZone
            };
        }
        public async Task<ResponseAPI<IEnumerable<GetMarketZoneResponse>>> SearchAllOrders(int size, int page)
        {
            try
            {
                IPaginate<GetMarketZoneResponse> data = await _unitOfWork.GetRepository<MarketZone>().GetPagingListAsync(
                selector: x => new GetMarketZoneResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    Location = x.Location,
                    ShortName = x.ShortName
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name));
                return new ResponseAPI<IEnumerable<GetMarketZoneResponse>>
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Get Paging List MarketZones Successfully",
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    },
                    Data = data.Items,
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<GetMarketZoneResponse>>
                {
                    MessageResponse = "Get Paging List MarketZones Fail: " + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> CreateMarketZone(MarketZoneRequest request)
        {
            if (!ValidationUtils.IsPhoneNumber(request.PhoneNumber)) throw new BadHttpRequestException("Invalid phone number");
            if (!ValidationUtils.IsEmail(request.Email)) throw new BadHttpRequestException("Invalid email");
            bool marketZoneMap = await CreateMarketZoneFirst(request);
            if (!marketZoneMap) throw new BadHttpRequestException("Create market zone failed");
            //create admin marketZone
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Name == RoleEnum.Admin.GetDescriptionFromEnum())
                ?? throw new BadHttpRequestException("Role not found");
            var user = await CreateAdmin(request.Email, request.PhoneNumber, role.Id, request.Id, request.CccdPassport, request.Address)
                ?? throw new BadHttpRequestException("Create admin failed");
            var userExit = await _account.SearchUser(user.Id);
            //create admin reftoken
            bool refToken = await CreateRefToken(userExit.Data);
            if (!refToken) throw new BadHttpRequestException("Create ref token failed");

            //create wallet admin
            var walletType = await CreateWalletTypeAdminFirst(WalletTypeEnum.UserWallet.GetDescriptionFromEnum(), request.Id);
            var wallet = await CreateWallet(user.Id, walletType.Id);
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = "Create market zone successfully",
                Data = marketZoneMap
            };
        }
        private async Task<bool> CreateWallet(Guid userId, Guid walletTypeId)
        {
            var walletExit = await _unitOfWork.GetRepository<Wallet>().SingleOrDefaultAsync
                (predicate: x => x.UserId == userId && x.WalletTypeId == walletTypeId);
            if (walletExit != null) return true;
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                BalanceHistory = 0,
                WalletTypeId = walletTypeId,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false
            };
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(wallet);
            return await _unitOfWork.CommitAsync() > 0;
        }
        private async Task<bool> CreateRefToken(User user)
        {
            var refreshTokenExit = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync
                (predicate: x => x.UserId == user.Id && x.Name == RoleEnum.Admin.GetDescriptionFromEnum());
            if (refreshTokenExit != null) return true;
            Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("MarketZoneId", user.MarketZoneId);
            var refreshToken = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(2)),
                CrDate = TimeUtils.GetCurrentSEATime(),
                Name = RoleEnum.Admin.GetDescriptionFromEnum(),
                UpsDate = TimeUtils.GetCurrentSEATime()
            };
            await _unitOfWork.GetRepository<UserRefreshToken>().InsertAsync(refreshToken);
            return await _unitOfWork.CommitAsync() > 0;
        }
        private async Task<User> CreateAdmin(string email, string phone, Guid roleId, Guid apiKey, string CccdPassport, string address)
        {
            var userExit = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.RoleId == roleId && x.MarketZoneId == apiKey, include: y => y.Include(z => z.Role));
            if (userExit != null) return userExit;
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PhoneNumber = phone,
                FullName = "Administator",
                RoleId = roleId,
                Gender = (int)GenderEnum.Other,
                MarketZoneId = apiKey,
                Password = PasswordUtil.HashPassword(EnvironmentVariableConstant.DefaultPassword),
                CccdPassport = CccdPassport,
                IsChange = true,
                Status = (int)UserStatusEnum.Active,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Address = address
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(user);
            await _unitOfWork.CommitAsync();
            return user;
        }
        private async Task<bool> CreateMarketZoneFirst(MarketZoneRequest request)
        {
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == request.Id);
            if (marketZone != null) return true;
            var marketZoneMap = _mapper.Map<MarketZone>(request);
            marketZoneMap.Deflag = false;
            marketZoneMap.CrDate = TimeUtils.GetCurrentSEATime();
            marketZoneMap.UpsDate = TimeUtils.GetCurrentSEATime();
            await _unitOfWork.GetRepository<MarketZone>().InsertAsync(marketZoneMap);

            return await _unitOfWork.CommitAsync() > 0;
        }
        private async Task<WalletType> CreateWalletTypeAdminFirst(string name, Guid apiKey)
        {
            var checkWalletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync
                (predicate: x => x.Name == WalletTypeEnum.UserWallet.GetDescriptionFromEnum() && x.MarketZoneId == apiKey);
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == apiKey)
                ?? throw new BadHttpRequestException("Market zone not found");
            if (checkWalletType != null) return checkWalletType;
            var walletType = new WalletType
            {
                Id = Guid.NewGuid(),
                Name = name,
                Deflag = false,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                MarketZoneId = apiKey
            };
            await _unitOfWork.GetRepository<WalletType>().InsertAsync(walletType);
            await _unitOfWork.CommitAsync();
            return walletType;
        }
        public async Task<ResponseAPI> CreateRole(string name)
        {
            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Deflag = false
            };
            await _unitOfWork.GetRepository<Role>().InsertAsync(newRole);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Created,
                    MessageResponse = "Create role successfully",
                    Data = newRole.Id
                }
                : new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "Create role failed"
                };
        }
        public async Task<ResponseAPI> DeleteRole(Guid id)
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (role == null) throw new BadHttpRequestException("Role not found");
            role.Deflag = true;
            _unitOfWork.GetRepository<Role>().UpdateAsync(role);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Delete role successfully",
                    Data = role.Id
                }
                : new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "Delete role failed"
                };
        }
        public async Task<ResponseAPI> UpdateRole(Guid id, string name)
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (role == null) throw new BadHttpRequestException("Role not found");
            role.Name = name.Trim();
            _unitOfWork.GetRepository<Role>().UpdateAsync(role);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Update role successfully",
                    Data = role.Id
                }
                : new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "Update role failed"
                };
        }
        public async Task<ResponseAPI<IEnumerable<Object>>> GetListRole(int size, int page)
        {
            try
            {
                IPaginate<Object> data = (IPaginate<Object>)await _unitOfWork.GetRepository<Role>().GetPagingListAsync(
                selector: x => new { name = x.Name },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name)

                );
                return new ResponseAPI<IEnumerable<Object>>
                {
                    MessageResponse = "Get List Role Success",
                    StatusCode = HttpStatusCodes.OK,
                    Data = data.Items,
                    MetaData = new MetaData
                    {
                        Size = data.Size,
                        Page = data.Page,
                        Total = data.Total,
                        TotalPage = data.TotalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<Object>>
                {
                    MessageResponse = "Get Roles fail: " + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI> CreateMarketZoneConfig(Guid apiKey, double storeTransferRate, double withrawRate)
        {
            var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == apiKey);
            if (marketZone == null) throw new BadHttpRequestException("Market zone not found");
            var marketZoneConfig = new MarketZoneConfig
            {
                Id = Guid.NewGuid(),
                MarketZoneId = apiKey,
                StoreStranferRate = storeTransferRate,
                WithdrawRate = withrawRate
            };
            await _unitOfWork.GetRepository<MarketZoneConfig>().InsertAsync(marketZoneConfig);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Created,
                    MessageResponse = "Create market zone config successfully",
                    Data = marketZoneConfig.Id
                }
                : new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = "Create market zone config failed"
                };
        }
    }
}

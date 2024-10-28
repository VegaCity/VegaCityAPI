using AutoMapper;
using Google.Apis.Auth.OAuth2.Requests;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services;
using VegaCityApp.API.Services.Implement;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.Service.Implement
{
    public class AccountService : BaseService<AccountService>, IAccountService
    {
        private readonly IUtilService _utilService;

        public AccountService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<AccountService> logger,
            IMapper mapper, IUtilService utilService,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _utilService = utilService;
        }

        #region Private Method
        private async Task<User> CreateUserRegister(RegisterRequest req, Guid apiKey)
        {
            var role = await _unitOfWork.GetRepository<Role>()
                .SingleOrDefaultAsync(predicate: r => r.Name == req.RoleName.Trim().Replace(" ", string.Empty));
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName.Trim(),
                PhoneNumber = req.PhoneNumber.Trim(),
                CccdPassport = req.CccdPassport.Trim(),
                Address = req.Address.Trim(),
                Email = req.Email.Trim(),
                Description = req.Description.Trim(),
                MarketZoneId = apiKey,
                RoleId = role != null ? role.Id : Guid.Parse(EnvironmentVariableConstant.StoreId),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Gender = (int)GenderEnum.Other,
                Status = role != null && role.Name == RoleEnum.Store.GetDescriptionFromEnum()? (int)UserStatusEnum.PendingVerify: (int) UserStatusEnum.Active,
                Password = role != null && role.Name == RoleEnum.Store.GetDescriptionFromEnum()? null : PasswordUtil.GenerateCharacter(10),
                IsChange = false,
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            await _unitOfWork.CommitAsync();
            newUser.Role = role;
            return  newUser;
        }
        private async Task<bool> CreateUserWallet(Guid userId)
        {
            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                BalanceHistory = 0,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false,
                WalletTypeId = Guid.Parse(EnvironmentVariableConstant.UserWallet)
            };
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
            return await _unitOfWork.CommitAsync() > 0;
        }
        private async Task<Guid> UpdateUserApproving(User user, Guid storeId)
        {
            user.Status = (int)UserStatusEnum.Active;
            user.IsChange = false;
            user.Password = PasswordUtil.GenerateCharacter(10);
            user.StoreId = storeId;
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return user.Id;
        }
        private async Task<bool> DeleteRefreshToken(string token)
        {
            var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                               predicate: x => x.Token == token);
            if (refreshToken == null)
            {
                return false;
            }
            _unitOfWork.GetRepository<UserRefreshToken>().DeleteAsync(refreshToken);
            return await _unitOfWork.CommitAsync() > 0;
        }
        #endregion
        public async Task<LoginResponse> Login(LoginRequest req)
        {
            Tuple<string, Guid> guidClaim = null;
            if (!ValidationUtils.IsEmail(req.Email.Trim())) throw new BadHttpRequestException(UserMessage.InvalidEmail);
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Email == req.Email,
                include: User => User.Include(y => y.Role)) ?? throw new BadHttpRequestException(UserMessage.UserNotFound);
            switch (user.Status)
            {
                case (int)UserStatusEnum.Active:
                    if (user.Password == PasswordUtil.HashPassword(req.Password))
                    {
                        //generate Access Token
                        guidClaim = new Tuple<string, Guid>("MarketZoneId", user.MarketZoneId);
                        var token = JwtUtil.GenerateJwtToken(user, guidClaim);
                        //check refresh token
                        var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                                                        predicate: x => x.UserId == user.Id && x.Name == user.Role.Name) ?? throw new BadHttpRequestException(UserMessage.SessionExpired);
                        var tokenRefresh = "";
                        //check expire date
                        var exDay = JwtUtil.GetExpireDate(refreshToken.Token);
                        if(TimeUtils.GetCurrentSEATime() > exDay) throw new BadHttpRequestException(UserMessage.SessionExpired);
                        else
                        {
                            //deccode token
                            refreshToken.Token = JwtUtil.GenerateRefreshToken(user, guidClaim, exDay);
                            refreshToken.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<UserRefreshToken>().UpdateAsync(refreshToken);
                            tokenRefresh = refreshToken.Token;
                        }
                        return await _unitOfWork.CommitAsync() > 0? new LoginResponse
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = UserMessage.LoginSuccessfully,
                            Data = new Data
                            {
                                UserId = user.Id,
                                Email = user.Email,
                                RoleName = user.Role.Name,
                                RoleId = user.Role.Id,
                                Tokens = new Tokens
                                {
                                    AccessToken = token,
                                    RefreshToken = tokenRefresh
                                }
                            }
                        }: throw new BadHttpRequestException(UserMessage.SaveRefreshTokenFail);
                    }
                    throw new BadHttpRequestException(UserMessage.WrongPassword);
                case (int)UserStatusEnum.PendingVerify:
                    throw new BadHttpRequestException(UserMessage.PendingVerify);
                case (int)UserStatusEnum.Disable:
                    throw new BadHttpRequestException(UserMessage.UserDisable);
                case (int)UserStatusEnum.Ban:
                    throw new BadHttpRequestException(UserMessage.UserBan);
            }

            throw new BadHttpRequestException(UserMessage.LoginFail);

        }
        public async Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req)
        {
            Tuple<string, Guid> guidClaim = null;
            var user = await _utilService.GetUser(req.Email, req.apiKey) ?? throw new BadHttpRequestException(UserMessage.UserNotFound);
            var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                               predicate: x => x.UserId == user.Id && x.Token == req.RefreshToken);
 
            if(refreshToken == null)
            {
                var check = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                                                  predicate: x => x.Name == user.Role.Name && x.UserId == user.Id);
                if(check != null) throw new BadHttpRequestException(UserMessage.UserHadToken);
                guidClaim = new Tuple<string, Guid>("MarketZoneId", user.MarketZoneId); 
                var newToken = new UserRefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = user.Role.Name == RoleEnum.Admin.GetDescriptionFromEnum() 
                    ? JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(2)) 
                    : JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(5)),
                    Name = user.Role.Name,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<UserRefreshToken>().InsertAsync(newToken);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.RefreshTokenSuccessfully,
                    Data = new
                    {
                        RefreshToken = newToken.Token
                    }
                };
            }
            else
            {
                //delete refresh token
                await DeleteRefreshToken(req.RefreshToken);
                guidClaim = new Tuple<string, Guid>("MarketZoneId", user.MarketZoneId); 
                var newRefreshToken = new UserRefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = user.Role.Name == RoleEnum.Admin.GetDescriptionFromEnum() 
                    ? JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(2)) 
                    : JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(5)),
                    Name = user.Role.Name,
                    CrDate = TimeUtils.GetCurrentSEATime(),
                    UpsDate = TimeUtils.GetCurrentSEATime()
                };
                await _unitOfWork.GetRepository<UserRefreshToken>().InsertAsync(newRefreshToken);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.RefreshTokenSuccessfully,
                    Data = new
                    {
                        RefreshToken = newRefreshToken.Token
                    }
                };
            }
        }
        public async Task<ResponseAPI> GetRefreshTokenByEmail(string email, GetApiKey req)
        {
            //check email valid format
            if (!ValidationUtils.IsEmail(email.Trim())) throw new BadHttpRequestException(UserMessage.InvalidEmail);
            var user = await _utilService.GetUser(email, req.apiKey) ?? throw new BadHttpRequestException(UserMessage.UserNotFound);
            var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                predicate: x => x.UserId == user.Id) ?? throw new BadHttpRequestException(UserMessage.RefreshTokenNotFound);
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = UserMessage.GetRefreshTokenSuccessfully,
                Data = new
                {
                    UserEmail = user.Email,
                    RefreshToken = refreshToken.Token
                }
            };
        }
        public async Task<ResponseAPI> Register(RegisterRequest req)
        {
            //check form Email, PhoneNumber, CCCD
            if (!ValidationUtils.IsEmail(req.Email.Trim())) throw new BadHttpRequestException(UserMessage.InvalidEmail);

            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber.Trim())) throw new BadHttpRequestException(UserMessage.InvalidPhoneNumber);

            if (!ValidationUtils.IsCCCD(req.CccdPassport.Trim())) throw new BadHttpRequestException(UserMessage.InvalidCCCD);

            //check if email is already exist
            var emailExist = await _utilService.GetUser(req.Email, req.apiKey);
            if (emailExist != null) throw new BadHttpRequestException(UserMessage.EmailExist);
            var phoneNumberExist = await _utilService.GetUserPhone(req.PhoneNumber, req.apiKey);
            if (phoneNumberExist != null) throw new BadHttpRequestException(UserMessage.PhoneNumberExist);
            var cccdExist = await _utilService.GetUserCCCDPassport(req.CccdPassport, req.apiKey);
            if (cccdExist != null) throw new BadHttpRequestException(UserMessage.CCCDExist);

            //create new user
            var newUser = await CreateUserRegister(req, req.apiKey);
            //create refesh token
            var refresh = new ReFreshTokenRequest
            {
                apiKey = req.apiKey,
                Email = newUser.Email,
                RefreshToken = null
            };
            var token = await RefreshToken(refresh);
            if (newUser.Id != Guid.Empty)
            {
                //create wallet
                var result = await CreateUserWallet(newUser.Id);
                if (!result) throw new BadHttpRequestException(UserMessage.CreateWalletFail);
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Created,
                    MessageResponse = UserMessage.CreateSuccessfully,
                    Data = new
                    {
                        UserId = newUser.Id,
                        token.Data
                    }
                };
            }
            throw new BadHttpRequestException(UserMessage.CreateUserFail);
        }
        public async Task<ResponseAPI> AdminCreateUser(RegisterRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            #region validate form
            if (!ValidationUtils.IsEmail(req.Email.Trim())) throw new BadHttpRequestException(UserMessage.InvalidEmail);

            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber.Trim())) throw new BadHttpRequestException(UserMessage.InvalidPhoneNumber);

            if (!ValidationUtils.IsCCCD(req.CccdPassport.Trim())) throw new BadHttpRequestException(UserMessage.InvalidCCCD);
            #endregion
            #region check exist
            var emailExist = await _utilService.GetUser(req.Email, apiKey);
            if (emailExist != null) throw new BadHttpRequestException(UserMessage.EmailExist);
            var phoneNumberExist = await _utilService.GetUserPhone(req.PhoneNumber, apiKey);
            if (phoneNumberExist != null) throw new BadHttpRequestException(UserMessage.PhoneNumberExist);
            var cccdExist = await _utilService.GetUserCCCDPassport(req.CccdPassport, apiKey);
            if (cccdExist != null) throw new BadHttpRequestException(UserMessage.CCCDExist);
            #endregion
            #region create new user
            var newUser = await CreateUserRegister(req, apiKey);
            #endregion
            #region create refesh token
            var refresh = new ReFreshTokenRequest
            {
                apiKey = apiKey,
                Email = newUser.Email,
                RefreshToken = null
            };
            var token = await RefreshToken(refresh);
            #endregion
            #region create wallet
            var result = await CreateUserWallet(newUser.Id);
            if (!result) throw new BadHttpRequestException(UserMessage.CreateWalletFail);
            #endregion
            #region send mail
            if (newUser != null)
            {
                try
                {
                    var subject = UserMessage.YourPasswordToChange;
                    var body = "Your Your Password To Change. Your password is: " + newUser.Password;
                    await MailUtil.SendMailAsync(newUser.Email, subject, body);
                }catch(Exception ex)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.SendMailFail
                    };
                }
            }
            #endregion
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = UserMessage.CreateSuccessfully,
                Data = new
                {
                    UserId = newUser.Id,
                    RefreshToken = token.Data
                }
            };
        }
        public async Task<ResponseAPI> ApproveUser(Guid userId, ApproveRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            string roleName = GetRoleFromJwt();
            var house = await _utilService.GetHouse(req.LocationHouse, req.AddressHouse) ?? throw new BadHttpRequestException(UserMessage.HouseNotFound);
            if (house.IsRent) throw new BadHttpRequestException(UserMessage.HouseIsRent);

            var user = await SearchUser(userId) ?? throw new BadHttpRequestException(UserMessage.UserNotFound);
            if(user.MarketZoneId != apiKey) throw new BadHttpRequestException(UserMessage.UserNotFound);
            if (user.Status == (int) UserStatusEnum.Active)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.Approved,
                    Data = new
                    {
                        UserId = user.Id
                    }
                };
            }
            if (roleName != RoleEnum.Admin.GetDescriptionFromEnum()) throw new BadHttpRequestException(UserMessage.RoleNotAllow);
            if (req.ApprovalStatus.Trim().Equals(ApproveStatus.REJECT))
            {
                user.Status = (int)UserStatusEnum.Disable;
                _unitOfWork.GetRepository<User>().UpdateAsync(user);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Accepted,
                    MessageResponse = UserMessage.ApproveReject,
                    Data = new
                    {
                        UserId = user.Id
                    }
                };
            }
            else if (req.ApprovalStatus.Trim().Equals(ApproveStatus.APPROVED))
            {
                #region check phone, email valid format
                if (!ValidationUtils.IsEmail(req.StoreEmail)) throw new BadHttpRequestException(UserMessage.InvalidEmail);
                if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber)) throw new BadHttpRequestException(UserMessage.InvalidPhoneNumber);
                #endregion
                if (user.RoleId == Guid.Parse(EnvironmentVariableConstant.StoreId))
                {
                    #region create store
                    var newStore = new Store
                    {
                        Id = Guid.NewGuid(),
                        Name = req.StoreName.Trim(),
                        Address = req.StoreAddress.Trim(),
                        PhoneNumber = req.PhoneNumber.Trim(),
                        Email = req.StoreEmail.Trim(),
                        Status = (int)StoreStatusEnum.Closed,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        MarketZoneId = apiKey,
                        Deflag = false,
                        HouseId = house.Id
                    };
                    await _unitOfWork.GetRepository<Store>().InsertAsync(newStore);
                    var wallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Balance = 0,
                        BalanceHistory = 0,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Deflag = false,
                        StartDate = TimeUtils.GetCurrentSEATime(),
                        StoreId = newStore.Id,
                        WalletTypeId = Guid.Parse(EnvironmentVariableConstant.StoreWallet)
                    };
                    await _unitOfWork.GetRepository<Wallet>().InsertAsync(wallet);
                    await _unitOfWork.CommitAsync();
                    #endregion
                    //update user
                    house.IsRent = true;
                    house.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<House>().UpdateAsync(house);
                    var result = await UpdateUserApproving(user, newStore.Id);
                    await _unitOfWork.CommitAsync();
                    if (result != Guid.Empty)
                    {
                        #region send mail
                        if (user != null)
                        {
                            try
                            {
                                var subject = UserMessage.ApproveSuccessfully;
                                var body = "Your account has been approved. Your password is: " + user.Password;
                                await MailUtil.SendMailAsync(user.Email, subject, body);
                            }
                            catch (Exception ex)
                            {
                                return new ResponseAPI
                                {
                                    StatusCode = HttpStatusCodes.BadRequest,
                                    MessageResponse = UserMessage.SendMailFail
                                };

                            }

                            return new ResponseAPI
                            {
                                StatusCode = HttpStatusCodes.Created,
                                MessageResponse = UserMessage.ApproveSuccessfully,
                                Data = new
                                {
                                    UserId = result,
                                    StoreId = newStore.Id
                                }
                            };
                        }
                        #endregion
                    }
                }
            }
            throw new BadHttpRequestException(UserMessage.ApproveFail);
        }
        //after register, admin will approve user
        public async Task<ResponseAPI> ChangePassword(ChangePasswordRequest req)
        {
            if (!ValidationUtils.IsEmail(req.Email.Trim())) throw new BadHttpRequestException(UserMessage.InvalidEmail);
            var user = await _utilService.GetUser(req.Email.Trim(), req.apiKey) ?? throw new BadHttpRequestException(UserMessage.UserNotFound);
            if (user.IsChange == false)
            {
                if(RoleHelper.allowedRoles.Contains(user.Role.Name))
                {
                    if (user.Password == req.OldPassword.Trim())
                    {
                        user.Password = PasswordUtil.HashPassword(req.NewPassword);
                        user.IsChange = true;
                        _unitOfWork.GetRepository<User>().UpdateAsync(user);
                        await _unitOfWork.CommitAsync();
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = UserMessage.ChangePasswordSuccessfully,
                            Data = new
                            {
                                UserId = user.Id
                            }
                        };
                    }
                    else throw new BadHttpRequestException(UserMessage.OldPasswordNotDuplicate);
                }
            }
            else
            {
                if(user.Password == PasswordUtil.HashPassword(req.OldPassword.Trim()))
                {
                    user.Password = PasswordUtil.HashPassword(req.NewPassword.Trim());
                    _unitOfWork.GetRepository<User>().UpdateAsync(user);
                    await _unitOfWork.CommitAsync();
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = UserMessage.ChangePasswordSuccessfully,
                        Data = new
                        {
                            UserId = user.Id
                        }
                    };
                }
            }
            throw new BadHttpRequestException(UserMessage.PasswordIsNotChanged);
        }
        public async Task<ResponseAPI<IEnumerable<GetUserResponse>>> SearchAllUser(int size, int page)
        {
            try
            {
                Guid apiKey = GetMarketZoneIdFromJwt();
                IPaginate<GetUserResponse> data = await _unitOfWork.GetRepository<User>().GetPagingListAsync(
                selector: x => new GetUserResponse()
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    Birthday = x.Birthday,
                    Description = x.Description,
                    CccdPassport = x.CccdPassport,
                    Gender = x.Gender,
                    ImageUrl = x.ImageUrl,
                    StoreId = x.StoreId,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    RoleId = x.RoleId,
                    Status = x.Status
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.FullName),
                predicate: x => //x.Status == (int)UserStatusEnum.Active || x.Status == (int)UserStatusEnum.PendingVerify &&
                                x.MarketZoneId == apiKey);

                return new ResponseAPI<IEnumerable<GetUserResponse>>
                {
                    MessageResponse = UserMessage.GetListSuccess,
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
                throw new Exception(UserMessage.GetAllUserFail + " " +  ex.Message);
            }
        }

        public async Task<User> SearchUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Id == UserId 
                && (x.Status ==(int) UserStatusEnum.Active || x.Status ==(int)UserStatusEnum.PendingVerify),
                include: user => user
                        .Include(y => y.Wallets)
                        .Include(y => y.Store)
                        .Include(y => y.Role)
            );
            return user;
        }
        public async Task<ResponseAPI<User>> UpdateUser(Guid userId, UpdateUserAccountRequest req)
        {
            if(!ValidationUtils.IsPhoneNumber(req.PhoneNumber.Trim())) throw new BadHttpRequestException(UserMessage.InvalidPhoneNumber);
            
            var user = await SearchUser(userId);

            if (user == null) throw new BadHttpRequestException(UserMessage.UserNotFound);
            else if(user.Status != (int)UserStatusEnum.Active) throw new BadHttpRequestException(UserMessage.UserDisable);
            
            user.FullName = req.FullName != null ? req.FullName.Trim() : user.FullName;
            user.PhoneNumber = req.PhoneNumber != null ? req.PhoneNumber.Trim() : user.PhoneNumber;
            user.Birthday = req.Birthday ?? user.Birthday;
            user.Gender = req.Gender ?? user.Gender;
            user.ImageUrl = req.ImageUrl != null ? req.ImageUrl.Trim() : user.ImageUrl;
            user.Address = req.Address != null ? req.Address.Trim() : user.Address;
            user.Description = req.Description != null ? req.Description.Trim() : user.Description;

            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI<User>()
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.UpdateUserSuccessfully,
                    Data = user
                }
                : throw new BadHttpRequestException(UserMessage.FailedToUpdate);
        }
        public async Task<ResponseAPI> DeleteUser(Guid UserId)
        {
            var user = await SearchUser(UserId);
            if (user == null) throw new BadHttpRequestException(UserMessage.UserNotFound);
            else if (user.Status != (int)UserStatusEnum.Active) throw new BadHttpRequestException(UserMessage.UserDisable);
            switch (user.Status)
            {
                case (int)UserStatusEnum.Active:
                    user.Status = (int)UserStatusEnum.Disable;
                    user.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<User>().UpdateAsync(user);
                    return await _unitOfWork.CommitAsync() > 0
                        ? new ResponseAPI()
                        {
                            MessageResponse = UserMessage.DeleteUserSuccess,
                            StatusCode = HttpStatusCodes.OK
                        }
                        : throw new BadHttpRequestException(UserMessage.DeleteUserFail);
                case (int)UserStatusEnum.Ban:
                    throw new BadHttpRequestException(UserMessage.UserBan);
                case (int)UserStatusEnum.Disable:
                    throw new BadHttpRequestException(UserMessage.UserDisable);
            }
            throw new BadHttpRequestException(UserMessage.DeleteUserFail);
        }
        public async Task<ResponseAPI<Wallet>> GetAdminWallet()
        {
            var currentMarketZoneId = GetMarketZoneIdFromJwt();
            var marketzone = await _utilService.GetMarketZone(currentMarketZoneId) ?? throw new BadHttpRequestException("Market Zone is not found");
            if (string.IsNullOrEmpty(marketzone.Email)) throw new BadHttpRequestException("Market Zone email is not found");
            var admin = await _utilService.GetUser(marketzone.Email, marketzone.Id) ?? throw new BadHttpRequestException("Admin is not found");
            if (admin.Status != (int)UserStatusEnum.Active) throw new BadHttpRequestException("Admin is not active");
            if (!admin.Wallets.Any()) throw new BadHttpRequestException("Admin wallet is not found");
            Wallet? walletAd = admin.Wallets.SingleOrDefault() ?? throw new BadHttpRequestException("Admin wallet is not found");
            walletAd.User = null;
            return new ResponseAPI<Wallet>()
            {
                MessageResponse = UserMessage.GetWalletSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = walletAd
            };
        }

        public async Task<ResponseAPI> GetChartByDuration(AdminChartDurationRequest req)
        {
            string roleCurrent = GetRoleFromJwt() ?? throw new BadHttpRequestException("Role is not found");
            if (req.Days == 0) throw new BadHttpRequestException(TransactionMessage.DayNull);
            if (!DateTime.TryParse(req.StartDate + " 00:00:00.000Z", out DateTime startDate))
                throw new BadHttpRequestException("Invalid start date format.");
            DateTime endDate = startDate.AddDays(req.Days ?? throw new BadHttpRequestException(TransactionMessage.DayNull));
            var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.Status == OrderStatus.Completed, null, include: etag => etag.Include(y => y.Etag));
            var etags = await _unitOfWork.GetRepository<Etag>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate, null, null);
            var transactions = await _unitOfWork.GetRepository<Transaction>()
                                       .GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate
                                                       && x.Amount > 0
                                                       && x.Type != "WithdrawMoney",
                                                       null, null);
            var deposits = await _unitOfWork.GetRepository<Deposit>()
                                      .GetListAsync(x => x.CrDate >= startDate
                                                      && x.CrDate <= endDate
                                                      && x.Amount > 0,
                                                      null, null);
            var packages = await _unitOfWork.GetRepository<Package>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate,
                                                        null, null);
            if (transactions == null || !transactions.Any())
            throw new BadHttpRequestException("Transactions not found");

            if (roleCurrent == RoleEnum.Admin.GetDescriptionFromEnum())
            {
                var groupedStaticsAdmin = transactions
               .GroupBy(t => t.CrDate.ToString("MMM")) // Group by month name (e.g., "Oct")
               .Select(g => new
               {
                   Name = g.Key, // Month name
                   TotalTransactions = transactions.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   TotalTransactionsAmount = g.Sum(t => t.Amount),
                   //  EtagCount = etags.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   EtagCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key && o.EtagId != null),
                   OrderCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key),

               }).ToList();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Get Admin's Dashboard Successfully!",
                    Data = groupedStaticsAdmin
                };
            }
            else if (roleCurrent == "CashierWeb" || roleCurrent == "CashierApp")
            {
                var groupedStaticsCashier = deposits
              .GroupBy(t => t.CrDate.ToString("MMM")) // Group by month name (e.g., "Oct")
              .Select(g => new
              {
                  Name = g.Key, // Month name
                  TotalTransactions = deposits.Count(o => o.CrDate.ToString("MMM") == g.Key),
                  TotalTransactionsAmount = g.Sum(t => t.Amount),
                  EtagCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key && o.EtagId != null),
                  OrderCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key), //package 
                  PackageCount = packages.Count(o => o.CrDate.ToString("MMM") == g.Key)
              }).ToList();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Get Cashier's Dashboard Successfully!",
                    Data = groupedStaticsCashier
                };
            }
            // case store 
            var storeOrders = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.StoreId != null && x.Status == OrderStatus.Completed,
                                                        null, null);
            var groupedStaticsStore = orders
             .GroupBy(t => t.CrDate.ToString("MMM")) // Group by month name (e.g., "Oct")
             .Select(g => new
             {
                 Name = g.Key, // Month name
                 OrderCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key), //package 
                 TotalAmountFromOrder = g.Sum(t => t.TotalAmount),
                 //PackageCount = packages.Count(o => o.CrDate.ToString("MMM") == g.Key)
             }).ToList();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Get Store's Dashboard Successfully!",
                Data = groupedStaticsStore
            };
        }

    }
}

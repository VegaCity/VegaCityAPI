using AutoMapper;
using Google.Apis.Auth.OAuth2.Requests;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Admin;
using VegaCityApp.API.Payload.Request.Auth;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Services;
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
        public AccountService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<AccountService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
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
            if (!ValidationUtils.IsEmail(req.Email))
            {
                return new LoginResponse
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail
                };
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Email == req.Email,
                include: User => User.Include(y => y.Role));
            if (user == null)
            {
                return new LoginResponse
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound
                };
            }
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
                                                        predicate: x => x.UserId == user.Id && x.Name == user.Role.Name);
                        if (refreshToken == null)
                        {
                            return new LoginResponse
                            {
                                StatusCode = HttpStatusCodes.Unauthorized,
                                MessageResponse = UserMessage.SessionExpired
                            };
                        }
                        var tokenRefresh = "";
                        //check expire date
                        var exDay = JwtUtil.GetExpireDate(refreshToken.Token);
                        if(TimeUtils.GetCurrentSEATime() > exDay)
                        {
                            return new LoginResponse
                            {
                                StatusCode = HttpStatusCodes.Unauthorized,
                                MessageResponse = UserMessage.SessionExpired
                            };
                        }
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
                        }: new LoginResponse
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = UserMessage.SaveRefreshTokenFail
                        };
                    }
                    return new LoginResponse
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.WrongPassword
                    };
                case (int)UserStatusEnum.PendingVerify:
                    return new LoginResponse
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.PendingVerify
                    };
                case (int)UserStatusEnum.Disable:
                    return new LoginResponse
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserDisable
                    };
                case (int)UserStatusEnum.Ban:
                    return new LoginResponse
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserBan
                    };
            }

            return new LoginResponse
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.LoginFail
            };

        }
        public async Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req)
        {
            Tuple<string, Guid> guidClaim = null;
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                               predicate: x => x.Email == req.Email && x.MarketZoneId == req.apiKey,
                                              include: User => User.Include(y => y.Role));
            var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                               predicate: x => x.UserId == user.Id && x.Token == req.RefreshToken);
            if(user == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound
                };
            }
            if(refreshToken == null)
            {
                var check = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                                                  predicate: x => x.Name == user.Role.Name && x.UserId == user.Id);
                if(check != null)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserHadToken
                    };
                }
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
            if (!ValidationUtils.IsEmail(email))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail
                };
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Email == email && x.MarketZoneId == req.apiKey);
            if (user == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound
                };
            }
            var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                predicate: x => x.UserId == user.Id);
            if (refreshToken == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.RefreshTokenNotFound
                };
            }
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
            if (!ValidationUtils.IsEmail(req.Email))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail
                };
            }

            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidPhoneNumber
                };
            }

            if (!ValidationUtils.IsCCCD(req.CccdPassport))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidCCCD
                };
            }

            //check if email is already exist
            var emailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email.Trim() && x.MarketZoneId == req.apiKey);
            if (emailExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EmailExist
                };
            }
            var phoneNumberExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.PhoneNumber == req.PhoneNumber.Trim() && x.MarketZoneId == req.apiKey);
            if (phoneNumberExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.PhoneNumberExist
                };
            }
            var cccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.CccdPassport == req.CccdPassport.Trim() && x.MarketZoneId == req.apiKey);
            if (cccdExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.CCCDExist
                };
            }

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
                if (!result)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.CreateWalletFail
                    };
                }
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
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.CreateUserFail
            };
        }
        public async Task<ResponseAPI> AdminCreateUser(RegisterRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            #region validate form
            if (!ValidationUtils.IsEmail(req.Email))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail,
                };
            }

            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidPhoneNumber,
                };
            }

            if (!ValidationUtils.IsCCCD(req.CccdPassport))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidCCCD,
                };
            }
            #endregion
            #region check exist
            var emailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email && x.MarketZoneId == apiKey);
            if (emailExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EmailExist
                };
            }
            var phoneNumberExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.PhoneNumber == req.PhoneNumber && x.MarketZoneId == apiKey);
            if (phoneNumberExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.PhoneNumberExist
                };
            }
            var cccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.CccdPassport == req.CccdPassport && x.MarketZoneId == apiKey);
            if (cccdExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.CCCDExist
                };
            }
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
            if (!result)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.CreateWalletFail
                };
            }
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
            var house = await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync(
                predicate: x => x.Location == req.LocationHouse.Trim() && x.Address == req.AdressHouse.Trim());
            if (house != null)
            {
                if (house.IsRent)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.HouseIsRent
                    };
                }
            }
            else
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.HouseNotFound
                };
            }
            var user = await _unitOfWork.GetRepository<User>()
            .SingleOrDefaultAsync(predicate: x => x.Id == userId && x.MarketZoneId == apiKey,
                                  include: role => role.Include(z => z.Role));
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
            if (roleName != RoleEnum.Admin.GetDescriptionFromEnum())
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.RoleNotAllow
                };
            }
            if (req.ApprovalStatus.Trim().Equals(ApproveStatus.REJECT))
            {
                if (user == null)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.NotFound,
                        MessageResponse = UserMessage.UserNotFound
                    };
                }

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
                if (!ValidationUtils.IsEmail(req.StoreEmail))
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.InvalidEmail
                    };
                }

                if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.InvalidPhoneNumber
                    };
                }
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
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.ApproveFail
            };
        }
        //after register, admin will approve user
        public async Task<ResponseAPI> ChangePassword(ChangePasswordRequest req)
        {
            if (!ValidationUtils.IsEmail(req.Email.Trim()))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail
                };
            }

            var user = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(predicate: x => x.Email == req.Email.Trim() && x.MarketZoneId == req.apiKey, include: user => user.Include(x => x.Role));
            if (user == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound
                };
            }

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
                    else
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = UserMessage.OldPasswordNotDuplicate
                        };
                    }
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
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.PasswordIsNotChanged
            };
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
                predicate: x => x.Status == (int)UserStatusEnum.Active || x.Status == (int)UserStatusEnum.PendingVerify && x.MarketZoneId == apiKey);

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
                return new ResponseAPI<IEnumerable<GetUserResponse>>
                {
                    MessageResponse = UserMessage.GetAllUserFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData=null
                };
            }
        }

        public async Task<ResponseAPI> SearchUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Id == UserId 
                && (x.Status ==(int) UserStatusEnum.Active || x.Status ==(int)UserStatusEnum.PendingVerify),
                include: user => user
                        .Include(y => y.Wallets)
                        .Include(y => y.Store)
                        .Include(y => y.Role)
            );
            if (user == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = UserMessage.NotFoundUser,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            return new ResponseAPI()
            {
                MessageResponse = UserMessage.GetUserSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    user
                }
            };
        }
        public async Task<ResponseAPI> UpdateUser(Guid userId, UpdateUserAccountRequest req)
        {
            if(!ValidationUtils.IsPhoneNumber(req.PhoneNumber.Trim()))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidPhoneNumber
                };
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                    (predicate: x => x.Id == userId && x.Status == (int)UserStatusEnum.Active);
            if (user == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.NotFoundUser
                };
            }
            user.FullName = req.FullName != null ? req.FullName.Trim() : user.FullName;
            user.PhoneNumber = req.PhoneNumber != null ? req.PhoneNumber.Trim() : user.PhoneNumber;
            user.Birthday = req.Birthday ?? user.Birthday;
            user.Gender = req.Gender ?? user.Gender;
            user.ImageUrl = req.ImageUrl != null ? req.ImageUrl.Trim() : user.ImageUrl;
            user.Address = req.Address != null ? req.Address.Trim() : user.Address;
            user.Description = req.Description != null ? req.Description.Trim() : user.Description;
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.UpdateUserSuccessfully
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.FailedToUpdate
                };
        }
        public async Task<ResponseAPI> DeleteUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Id == UserId && x.Status ==(int) UserStatusEnum.Active);
            if (user == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.UserMessage.NotFoundUser
                };
            }
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
                        : new ResponseAPI()
                        {
                            MessageResponse = UserMessage.DeleteUserFail,
                            StatusCode = HttpStatusCodes.BadRequest
                        };
                case (int)UserStatusEnum.Ban:
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserBan
                    };
                case (int)UserStatusEnum.Disable:
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserDisable
                    };
            }
            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.DeleteUserFail
            };
        }
        public async Task<ResponseAPI> GetAdminWallet()
        {
            var currentMarketZoneId = GetMarketZoneIdFromJwt();
            var marketzone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == currentMarketZoneId);
            if(marketzone == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = "Not Found MarketZone!!",
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            var admin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Email == marketzone.Email, include: wallet => wallet.Include(z => z.Wallets));
            if (admin == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = UserMessage.NotFoundUserWallet,
                    StatusCode = HttpStatusCodes.NotFound,
                };
            }
            Wallet walletAd = admin.Wallets.SingleOrDefault();
            walletAd.User = null;
            return new ResponseAPI()
            {
                MessageResponse = UserMessage.GetWalletSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = walletAd
            };
        }
    }
}

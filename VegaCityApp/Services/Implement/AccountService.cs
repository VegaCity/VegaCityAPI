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
using VegaCityApp.API.Payload.Response.UserResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Payload.Request.Store;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

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
                RoleId = role.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Gender = (int)GenderEnum.Other,
                Status = role != null && role.Name == RoleEnum.Store.GetDescriptionFromEnum()? (int)UserStatusEnum.PendingVerify: (int) UserStatusEnum.Active,
                Password = role != null && role.Name == RoleEnum.Store.GetDescriptionFromEnum()? null : PasswordUtil.GenerateCharacter(10),
                IsChange = false,
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            await _unitOfWork.CommitAsync();
            return  newUser;
        }
        private async Task<bool> CreateUserWallet(Guid userId)
        {
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(
                predicate: x => x.Name == WalletTypeEnum.UserWallet.GetDescriptionFromEnum());
            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                BalanceHistory = 0,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false,
                WalletTypeId = walletType.Id
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
            var mapping = new UserStoreMapping
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                StoreId = storeId
            };
            await _unitOfWork.GetRepository<UserStoreMapping>().InsertAsync(mapping);
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
                    if(user.Role.Name == RoleEnum.Store.GetDescriptionFromEnum())
                    {
                        //check mapping store
                        var store = await _unitOfWork.GetRepository<UserStoreMapping>().SingleOrDefaultAsync(
                            predicate: x => x.UserId == user.Id) 
                            ?? throw new BadHttpRequestException("Store Not Mapping with account", HttpStatusCodes.BadRequest);
                    }
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

        } //get ready !!
        public async Task<ResponseAPI<UserSession>> CreateUserSession(Guid userId, SessionRequest req) //get ready !!
        {
            if (req.EndDate < req.StartDate)
            {
                return new ResponseAPI<UserSession>
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EndDateInvalid
                };
            }
            
            var user = await SearchUser(userId);
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Id == req.ZoneId && !x.Deflag)
                ?? throw new BadHttpRequestException("Zone not found");
            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ZoneId = req.ZoneId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                TotalCashReceive = 0,
                Status = SessionStatusEnum.Active.GetDescriptionFromEnum(),
                TotalFinalAmountOrder = 0,
                TotalQuantityOrder = 0,
                TotalWithrawCash = 0
            };
            await _unitOfWork.GetRepository<UserSession>().InsertAsync(session);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI<UserSession>
            {
                StatusCode = HttpStatusCodes.Created,
                MessageResponse = UserMessage.CreateSessionSuccessfully,
                Data = session
            };
        }
        public async Task<ResponseAPI<UserSession>> GetUserSessionById(Guid sessionId)
        {
            var session = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.Id == sessionId,
                include: session => session.Include(x => x.User).Include(x => x.Zone))
                ?? throw new BadHttpRequestException("Session not found", HttpStatusCodes.NotFound);
            return new ResponseAPI<UserSession>
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = UserMessage.GetSessionSuccessfully,
                Data = session
            };
        }
        public async Task<ResponseAPI<IEnumerable<GetUserSessions>>> GetAllUserSessions(int page, int size)
        {
            try {
                var sessions = await _unitOfWork.GetRepository<UserSession>().GetPagingListAsync(
                    selector: x => new GetUserSessions
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        TotalCashReceive = x.TotalCashReceive,
                        TotalFinalAmountOrder = x.TotalFinalAmountOrder,
                        TotalQuantityOrder = x.TotalQuantityOrder,
                        TotalWithrawCash = x.TotalWithrawCash,
                        ZoneId = x.ZoneId,
                        Status = x.Status
                    },
                    page: page,
                    size: size,
                    orderBy: x => x.OrderByDescending(z => z.StartDate),
                    predicate: x => x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum());
                return new ResponseAPI<IEnumerable<GetUserSessions>>
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.GetAllSessionSuccessfully,
                    MetaData = new MetaData
                    {
                        Page = page,
                        Size = size,
                        Total = sessions.Total,
                        TotalPage = sessions.TotalPages
                    },
                    Data = sessions.Items
                };
            } 
            catch (Exception ex)
            {
                return new ResponseAPI<IEnumerable<GetUserSessions>>
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = UserMessage.GetAllSessionFail + ex.Message,
                    Data = null
                };
            }
        }
        public async Task<ResponseAPI> DeleteSession(Guid sessionId)
        {
            var sessionExsit = await GetUserSessionById(sessionId);
            sessionExsit.Data.Status = SessionStatusEnum.Canceled.GetDescriptionFromEnum();
            _unitOfWork.GetRepository<UserSession>().UpdateAsync(sessionExsit.Data);
            await _unitOfWork.CommitAsync();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = UserMessage.DeleteSessionSuccessfully
            };
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
                throw new BadHttpRequestException(UserMessage.InvalidEmail);

            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                throw new BadHttpRequestException(UserMessage.InvalidPhoneNumber);

            if (!ValidationUtils.IsCCCD(req.CccdPassport))
                throw new BadHttpRequestException(UserMessage.InvalidCCCD);

            //check if email is already exist
            var emailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email.Trim() && x.MarketZoneId == req.apiKey);
            if (emailExist != null)
                throw new BadHttpRequestException(UserMessage.EmailExist, HttpStatusCodes.BadRequest);
            var phoneNumberExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.PhoneNumber == req.PhoneNumber.Trim() && x.MarketZoneId == req.apiKey);
            if (phoneNumberExist != null)
                throw new BadHttpRequestException(UserMessage.PhoneNumberExist, HttpStatusCodes.BadRequest);
            var cccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.CccdPassport == req.CccdPassport.Trim() && x.MarketZoneId == req.apiKey);
            if (cccdExist != null)
                throw new BadHttpRequestException(UserMessage.CCCDExist, HttpStatusCodes.BadRequest);

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
                    throw new BadHttpRequestException(UserMessage.CreateWalletFail); 
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
        public async Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req)
        {
            Tuple<string, Guid> guidClaim = null;
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                               predicate: x => x.Email == req.Email && x.MarketZoneId == req.apiKey,
                                              include: User => User.Include(y => y.Role));
            var refreshToken = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                               predicate: x => x.UserId == user.Id && x.Token == req.RefreshToken);
            if (user == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound
                };
            }
            if (refreshToken == null)
            {
                var check = await _unitOfWork.GetRepository<UserRefreshToken>().SingleOrDefaultAsync(
                                                  predicate: x => x.Name == user.Role.Name && x.UserId == user.Id);
                if (check != null)
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
        //get ready !!
        public async Task<ResponseAPI> ApproveUser(Guid userId, ApproveRequest req)
        {
            Guid apiKey = GetMarketZoneIdFromJwt();
            string roleName = GetRoleFromJwt();
            if (roleName != RoleEnum.Admin.GetDescriptionFromEnum()) throw new BadHttpRequestException("You are not allowed to access this function");
            var user = await SearchUser(userId);
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Location == req.LocationZone && !x.Deflag)
                ?? throw new BadHttpRequestException("Zone not found");
            if (user.Data.Status == (int) UserStatusEnum.Active)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = UserMessage.Approved,
                    Data = new
                    {
                        UserId = user.Data.Id
                    }
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

                user.Data.Status = (int)UserStatusEnum.Disable;
                _unitOfWork.GetRepository<User>().UpdateAsync(user.Data);
                await _unitOfWork.CommitAsync();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.Accepted,
                    MessageResponse = UserMessage.ApproveReject,
                    Data = new
                    {
                        UserId = user.Data.Id
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
                if (user.Data.Role.Name == RoleEnum.Store.GetDescriptionFromEnum())
                {
                    #region create store
                    var newStore = new Store
                    {
                        Id = Guid.NewGuid(),
                        StoreType = req.StoreType,
                        Name = req.StoreName.Trim(),
                        Address = req.StoreAddress.Trim(),
                        PhoneNumber = req.PhoneNumber.Trim(),
                        Email = req.StoreEmail.Trim(),
                        Status = (int)StoreStatusEnum.Closed,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        MarketZoneId = apiKey,
                        Deflag = false,
                        ZoneId = zone.Id
                    };
                    await _unitOfWork.GetRepository<Store>().InsertAsync(newStore);
                    var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(
                        predicate: x => x.Name == WalletTypeEnum.StoreWallet.GetDescriptionFromEnum());
                    var wallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Data.Id,
                        Balance = 0,
                        BalanceHistory = 0,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Deflag = false,
                        StartDate = TimeUtils.GetCurrentSEATime(),
                        StoreId = newStore.Id,
                        WalletTypeId = walletType.Id
                    };
                    await _unitOfWork.GetRepository<Wallet>().InsertAsync(wallet);
                    await _unitOfWork.CommitAsync();
                    #endregion
                    //update user
                    var result = await UpdateUserApproving(user.Data, newStore.Id);
                    await _unitOfWork.CommitAsync();
                    if (result != Guid.Empty)
                    {
                        #region send mail
                        if (user != null)
                        {
                            try
                            {
                                var subject = UserMessage.ApproveSuccessfully;
                                var body = "Your account has been approved. Your password is: " + user.Data.Password + "\nPlease access this website to change password: http://localhost:3000/change-password";
                                await MailUtil.SendMailAsync(user.Data.Email, subject, body);
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
                return new ResponseAPI<IEnumerable<GetUserResponse>>
                {
                    MessageResponse = UserMessage.GetAllUserFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData=null
                };
            }
        }

        public async Task<ResponseAPI<User>> SearchUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Id == UserId 
                && (x.Status ==(int) UserStatusEnum.Active || x.Status ==(int)UserStatusEnum.PendingVerify),
                include: user => user
                        .Include(y => y.Wallets)
                        .Include(y => y.UserStoreMappings).ThenInclude(y => y.Store)
                        .Include(y => y.Role)
            ) ?? throw new BadHttpRequestException(UserMessage.NotFoundUser);
            return new ResponseAPI<User>
            {
                MessageResponse = UserMessage.GetUserSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = user
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

        public async Task<ResponseAPI> GetChartByDuration(AdminChartDurationRequest req)
        {
            string roleCurrent = GetRoleFromJwt();
            if (roleCurrent == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound,
                };
            }

            if (req.Days == 0)
            {
                return new ResponseAPI
                {
                    MessageResponse = TransactionMessage.DayNull,
                    StatusCode = HttpStatusCodes.BadRequest,
                };
            }

            if (!DateTime.TryParse(req.StartDate + " 00:00:00.000Z", out DateTime startDate))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = "Invalid start date format.",
                };
            }

            DateTime? endDate = startDate.AddDays(((int)req.Days));
            var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.Status == OrderStatus.Completed, null,null);
            //var etags = await _unitOfWork.GetRepository<Etag>().GetListAsync(x => x.CrDate >= startDate
            //                                           && x.CrDate <= endDate, null, null);
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
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = "Transaction not found"
                };
            }

            if (roleCurrent == "Admin")
            {
                var groupedStaticsAdmin = transactions
               .GroupBy(t => t.CrDate.ToString("MMM")) // Group by month name (e.g., "Oct")
               .Select(g => new
               {
                   Name = g.Key, // Month name
                   TotalTransactions = transactions.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   TotalTransactionsAmount = g.Sum(t => t.Amount),
                   //  EtagCount = etags.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   EtagCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key ),
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
                  EtagCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key),
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
        public async Task<string> ReAssignEmail(Guid userId, ReAssignEmail req)
        {
            Guid marketZoneId = GetMarketZoneIdFromJwt();
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == userId 
            && x.Status ==(int) UserStatusEnum.PendingVerify && x.MarketZoneId == marketZoneId);
            if (user == null)
            {
                return UserMessage.UserNotFound;
            }
            user.Email = req.Email;
            user.Password = PasswordUtil.GenerateCharacter(10);
            user.IsChange = false;
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            //send mail
            try
            {
                var subject = UserMessage.YourPasswordToChange;
                var body = "Your Your Password To Change. Your password is: " + user.Password;
                await MailUtil.SendMailAsync(user.Email, subject, body);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return UserMessage.SendMailFail;
            }
            return UserMessage.ReAssignEmailSuccess;
        }

        public async Task AddRole()
        {
            var role = await _unitOfWork.GetRepository<Role>().GetListAsync();
            if (role.Count == 0)
            {
                role = new List<Role>
                {
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = RoleEnum.Admin.GetDescriptionFromEnum(),
                        Deflag = false
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = RoleEnum.CashierWeb.GetDescriptionFromEnum(),
                        Deflag = false
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = RoleEnum.CashierApp.GetDescriptionFromEnum(),
                        Deflag = false
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = RoleEnum.Store.GetDescriptionFromEnum(),
                        Deflag = false
                    }
                };
                await _unitOfWork.GetRepository<Role>().InsertRangeAsync(role);
                await _unitOfWork.CommitAsync();
            }
            else
            {
                return;
            }
        }


        public async Task<ResponseAPI<IEnumerable<GetStoreResponse>>> GetAllClosingRequest(Guid apiKey, int size, int page)
        {
            try
            {
                IPaginate<GetStoreResponse> data = await _unitOfWork.GetRepository<Store>().GetPagingListAsync(

                selector: x => new GetStoreResponse()
                {
                    Id = x.Id,
                    StoreType = x.StoreType,
                    Name = x.Name,
                    Address = x.Address,
                    Description = x.Description,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    PhoneNumber = x.PhoneNumber,
                    MarketZoneId = x.MarketZoneId,
                    ShortName = x.ShortName,
                    Email = x.Email,
                    Status = x.Status,

                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => !x.Deflag && x.MarketZoneId == apiKey && x.Status == (int)StoreStatusEnum.Blocked
                );
                return new ResponseAPI<IEnumerable<GetStoreResponse>>
                {
                    MessageResponse = StoreMessage.GetListStoreSuccess,
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
                return new ResponseAPI<IEnumerable<GetStoreResponse>>
                {
                    MessageResponse = StoreMessage.GetListStoreFailed + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }

        public async Task<ResponseAPI> SearchStoreClosing(Guid StoreId)
        {
            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.Id == StoreId && !x.Deflag && x.Status == (int)StoreStatusEnum.Blocked,
                include: z => z.Include(s => s.Wallets)
                               .Include(a => a.StoreServices)
                               .Include(a => a.Menus).ThenInclude(a => a.Products).ThenInclude(o => o.ProductCategory)
            );
            if (store == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = StoreMessage.NotFoundStore,
                    StatusCode = HttpStatusCodes.NotFound
                };
            }
            string storeType = null;
            string storeStatus = null;
            if (store.Status == (int)StoreStatusEnum.Blocked)
            {
                storeStatus = StoreStatusEnum.Blocked.GetDescriptionFromEnum();
            }
            //check storetype enum and parse to string
            if (!StoreTypeHelper.allowedStoreTypes.Contains((int)store.StoreType))
            {
                throw new BadHttpRequestException(StoreMessage.InvalidStoreType, HttpStatusCodes.BadRequest);
            }
            else
            {
                if (store.StoreType == (int)StoreTypeEnum.Service)
                {
                    storeType = StoreTypeEnum.Service.GetDescriptionFromEnum();
                }
                else if (store.StoreType == (int)StoreTypeEnum.Food)
                {
                    storeType = StoreTypeEnum.Food.GetDescriptionFromEnum();
                }
                else if (store.StoreType == (int)StoreTypeEnum.Other)
                {
                    storeType = StoreTypeEnum.Other.GetDescriptionFromEnum();
                }
                else if (store.StoreType == (int)StoreTypeEnum.Clothing)
                {
                    storeType = StoreTypeEnum.Clothing.GetDescriptionFromEnum();
                }
            }
            return new ResponseAPI()
            {
                MessageResponse = StoreMessage.GetStoreSuccess,
                StatusCode = HttpStatusCodes.OK,
                Data = new
                {
                    storeType,
                    storeStatus,
                    store
                }
            };
        }


        public async Task<ResponseAPI> ResolveClosingStore(GetWalletStoreRequest req)
        {
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                throw new BadHttpRequestException(PackageItemMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);
            if (req.StoreName == null)
            {
                throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);
            }
            var searchName = NormalizeString(req.StoreName);
            var stores = await _unitOfWork.GetRepository<Store>().GetListAsync(predicate: x => x.PhoneNumber == req.PhoneNumber && x.Status == (int)StoreStatusEnum.Blocked
                                                                               , include: z => z.Include(s => s.Wallets)
                                                                                                .Include(a => a.StoreServices)
                                                                                                .Include(a => a.Menus).ThenInclude(a => a.Products));
            var storeTrack = stores.SingleOrDefault(x => NormalizeString(x.Name) == searchName || NormalizeString(x.ShortName) == searchName);
            if (storeTrack == null)
            {
                throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);
            }
            //if (storeTrack.Wallets.SingleOrDefault().Balance <= 50000)
            //{
            //    throw new BadHttpRequestException(StoreMessage.MustGreaterThan50K, HttpStatusCodes.BadRequest);
            //}
            
            if (req.Status == "APPROVED")
            {
                storeTrack.Wallets.SingleOrDefault().Deflag = false;
                storeTrack.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Wallet>().UpdateAsync(storeTrack.Wallets.SingleOrDefault());

                storeTrack.Status = (int)StoreStatusEnum.Closed;
                storeTrack.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Store>().UpdateAsync(storeTrack);
            }
            else if(req.Status != null)
            {
                if (req.Status != "REJECTED")
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.InvalidTypeOfStatus
                    };
                }
                if (req.Status == "REJECTED")
                {
                    if (storeTrack.StoreType == StoreTypeHelper.allowedStoreTypes[2])
                    {
                        foreach (var service in storeTrack.StoreServices)
                        {
                            service.Deflag = false;
                            service.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<VegaCityApp.Domain.Models.StoreService>().UpdateAsync(service);
                        }
                    }
                    else
                    {
                        var processedCategories = new HashSet<Guid>();
                        foreach (var menu in storeTrack.Menus)
                        {
                            menu.Deflag = false;
                            menu.UpsDate = TimeUtils.GetCurrentSEATime();
                            _unitOfWork.GetRepository<Menu>().UpdateAsync(menu);

                            foreach (var product in menu.Products)
                            {
                                product.Status = "Active";
                                product.UpsDate = TimeUtils.GetCurrentSEATime();
                                _unitOfWork.GetRepository<Product>().UpdateAsync(product);

                                if (!processedCategories.Contains(product.ProductCategoryId))
                                {
                                    var productCategory = await _unitOfWork.GetRepository<ProductCategory>()
                                                          .SingleOrDefaultAsync(predicate: c => c.Id == product.ProductCategoryId);

                                    if (productCategory != null && productCategory.Deflag)
                                    {
                                        productCategory.Deflag = false;
                                        productCategory.UpsDate = TimeUtils.GetCurrentSEATime();
                                        _unitOfWork.GetRepository<ProductCategory>().UpdateAsync(productCategory);

                                        // Add to processedCategories to avoid re-processing
                                        processedCategories.Add(product.ProductCategoryId);
                                    }
                                }
                            }
                        }
                    }
                    storeTrack.Wallets.SingleOrDefault().Deflag = false;
                    storeTrack.Wallets.SingleOrDefault().UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Wallet>().UpdateAsync(storeTrack.Wallets.SingleOrDefault());

                    storeTrack.Status = (int)StoreStatusEnum.Opened;
                    storeTrack.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<Store>().UpdateAsync(storeTrack);

                }
               
            }
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                #region send mail
                try
                {
                    //var admin = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(EnvironmentVariableConstant.marketZoneId));
                    var subject = UserMessage.ResolvedMessage;
                    var body = "We have process your closing request and decide the Status will be: " + req.Status;
                    await MailUtil.SendMailAsync(storeTrack.Email, subject, body);
                }
                catch (Exception ex)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.SendMailFail
                    };
                }
                #endregion
                return new ResponseAPI()
                {

                    MessageResponse = UserMessage.ApproveSubmitted + " " + req.Status,
                    StatusCode = HttpStatusCodes.OK,

                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = UserMessage.ApproveFailedSubmitted,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }

        private static string NormalizeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Chuẩn hóa chuỗi để loại bỏ dấu
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Chuẩn hóa chuỗi, chuyển thành chữ thường và loại bỏ khoảng trắng
            var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
            result = Regex.Replace(result, @"\s+", ""); // Loại bỏ tất cả khoảng trắng

            return result;
        }

    }
}

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
using VegaCityApp.API.Services.Interface;

namespace VegaCityApp.Service.Implement
{
    public class AccountService : BaseService<AccountService>, IAccountService
    {
        private readonly IStoreService _storeService;
        private readonly IUtilService _util;

        public AccountService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<AccountService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IStoreService storeService,
            IUtilService util) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
            _storeService = storeService;
            _util = util;
        }

        #region Private Method
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
        private async Task<User> CreateUserRegister(RegisterRequest req, Guid apiKey)
        {
            var role = await _unitOfWork.GetRepository<Role>()
                .SingleOrDefaultAsync(predicate: r => r.Name == req.RoleName.Trim().Replace(" ", string.Empty));
            if (req.RegisterStoreType != null)
            {
                if (!StoreTypeHelper.allowedStoreTypes.Contains((int)req.RegisterStoreType))
                {
                    throw new BadHttpRequestException("Invalid Store Type", HttpStatusCodes.BadRequest);
                }
            }
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
                Status = role != null && role.Name == RoleEnum.Store.GetDescriptionFromEnum() ? (int)UserStatusEnum.PendingVerify : (int)UserStatusEnum.Active,
                Password = role != null && role.Name == RoleEnum.Store.GetDescriptionFromEnum() ? null : PasswordUtil.GenerateCharacter(10),
                IsChange = false,
                RegisterStoreType = req.RegisterStoreType != null ? req.RegisterStoreType : null
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            await _unitOfWork.CommitAsync();
            return newUser;
        }
        private async Task<bool> CreateUserWallet(Guid userId, Guid apiKey)
        {
            var walletType = await _unitOfWork.GetRepository<WalletType>().SingleOrDefaultAsync(
                predicate: x => x.Name == WalletTypeEnum.UserWallet.GetDescriptionFromEnum() && x.MarketZoneId == apiKey);
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Id == userId);
            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = user.FullName,
                BalanceStart = 0,
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
            //update wallet user
            user.Wallets.SingleOrDefault().StoreId = storeId;
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(user.Wallets.SingleOrDefault());
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
        #region Auth
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
                predicate: x => x.Email == req.Email && x.Status == (int)UserStatusEnum.Active,
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
                    if (user.Role.Name == RoleEnum.Store.GetDescriptionFromEnum())
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
                        if (TimeUtils.GetCurrentSEATime() > exDay)
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
                        return await _unitOfWork.CommitAsync() > 0 ? new LoginResponse
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
                        } : new LoginResponse
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
                x.Email == req.Email.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Active);
            if (emailExist != null)
                throw new BadHttpRequestException(UserMessage.EmailExist, HttpStatusCodes.BadRequest);
            var phoneNumberExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.PhoneNumber == req.PhoneNumber.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Active);
            if (phoneNumberExist != null)
                throw new BadHttpRequestException(UserMessage.PhoneNumberExist, HttpStatusCodes.BadRequest);
            var cccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.CccdPassport == req.CccdPassport.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Active);
            if (cccdExist != null)
                throw new BadHttpRequestException(UserMessage.CCCDExist, HttpStatusCodes.BadRequest);
            //check ban user
            var banEmailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Ban);
            if (banEmailExist != null)
                throw new BadHttpRequestException(UserMessage.UserBan, HttpStatusCodes.BadRequest);
            var banCccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.CccdPassport == req.CccdPassport.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Ban);
            if (banCccdExist != null)
                throw new BadHttpRequestException(UserMessage.UserBan, HttpStatusCodes.BadRequest);
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
                var result = await CreateUserWallet(newUser.Id, req.apiKey);
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
                predicate: x => x.Email == email && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Active);
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
                .SingleOrDefaultAsync(predicate: x => x.Email == req.Email.Trim()
                                              && x.MarketZoneId == req.apiKey
                                              && x.Status == (int)UserStatusEnum.Active, include: user => user.Include(x => x.Role));
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
                if (RoleHelper.allowedRoles.Contains(user.Role.Name))
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
                if (user.Password == PasswordUtil.HashPassword(req.OldPassword.Trim()))
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
        public async Task<string> ReAssignEmail(Guid userId, ReAssignEmail req)
        {
            Guid marketZoneId = GetMarketZoneIdFromJwt();
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == userId
            && x.Status == (int)UserStatusEnum.PendingVerify && x.MarketZoneId == marketZoneId);
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
                var body = $"<div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>" +
                                               $"<h1 style='color: #007bff;'>Welcome to our Vega City!</h1>" +
                                               $"<p>Thanks for signing up our services.</p>" +
                                               $"<p><strong>This is your password to change: {user.Password}</strong></p>" +
                                               $"<p>Please access this website to change password: <a href='https://vegacity.id.vn/change-password'>Link Access !!</a></p>" +
                                           $"</div>"; ;
                await MailUtil.SendMailAsync(user.Email, subject, body);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return UserMessage.SendMailFail;
            }
            return UserMessage.ReAssignEmailSuccess;
        }
        #endregion
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
            var checkSessionExsit = await _unitOfWork.GetRepository<UserSession>().SingleOrDefaultAsync(
                predicate: x => x.UserId == userId && x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum());
            if (checkSessionExsit != null) throw new BadHttpRequestException("Session for this user is already exist", HttpStatusCodes.BadRequest);
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
            try
            {
                var sessions = await _unitOfWork.GetRepository<UserSession>().GetPagingListAsync(
                    selector: x => new GetUserSessions
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        UserName = x.User.FullName,
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
                    predicate: x => x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()
                                 && x.User.MarketZoneId == GetMarketZoneIdFromJwt(),
                    include: x => x.Include(a => a.User));
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
        public async Task<ResponseAPI> AdminCreateUser(RegisterRequest req)
        {
            if (req.RoleName == RoleEnum.Admin.GetDescriptionFromEnum())
            {
                throw new BadHttpRequestException("You are not allowed to create Role Admin", HttpStatusCodes.BadRequest);
            }
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
            //check ban user
            var banEmailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Ban);
            if (banEmailExist != null)
                throw new BadHttpRequestException(UserMessage.UserBan, HttpStatusCodes.BadRequest);
            var banCccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.CccdPassport == req.CccdPassport.Trim() && x.MarketZoneId == req.apiKey && x.Status == (int)UserStatusEnum.Ban);
            if (banCccdExist != null)
                throw new BadHttpRequestException(UserMessage.UserBan, HttpStatusCodes.BadRequest);
            #endregion
            //check session user
            await _util.CheckUserSession(GetUserIdFromJwt());
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
            var result = await CreateUserWallet(newUser.Id, apiKey);
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
                    var body = $"<div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>" +
                                               $"<h1 style='color: #007bff;'>Welcome to our Vega City!</h1>" +
                                               $"<p>Thanks for signing up our services.</p>" +
                                               $"<p><strong>This is your code to verify: {newUser.Password}</strong></p>" +
                                               $"<p>Please access this website to change password: <a href='https://vegacity.id.vn/change-password'>Link Access !!</a></p>" +
                                           $"</div>";
                    await MailUtil.SendMailAsync(newUser.Email, subject, body);
                }
                catch (Exception ex)
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
            //check session
            await _util.CheckUserSession(GetUserIdFromJwt());
            var zone = await _unitOfWork.GetRepository<Zone>().SingleOrDefaultAsync(predicate: x => x.Location == req.LocationZone && !x.Deflag)
                ?? throw new BadHttpRequestException("Zone not found");
            if (user.Data.Status == (int)UserStatusEnum.Active)
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
                    await _unitOfWork.CommitAsync();
                    #endregion
                    //update user
                    var result = await UpdateUserApproving(user.Data, newStore.Id);
                    if (result != Guid.Empty)
                    {
                        #region send mail
                        if (user != null)
                        {
                            try
                            {
                                var subject = UserMessage.ApproveSuccessfully;
                                //var body = $"Your account has been approved. Your password is: " + user.Data.Password + "\nPlease access this website to change password: http://localhost:3000/change-password";
                                var body = $"<div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>" +
                                               $"<h1 style='color: #007bff;'>Welcome to our Vega City!</h1>" +
                                               $"<p>Thanks for signing up our services.</p>" +
                                               $"<p><strong>This is your code to verify: {user.Data.Password}</strong></p>" +
                                               $"<p>Please access this website to change password: <a href='https://vegacity.id.vn/change-password'>Link Access !!</a></p>" +
                                           $"</div>";
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
                    Status = x.Status,
                    RegisterStoreType = x.RegisterStoreType == (int)StoreTypeEnum.Food ? "Store Product" : "Store Service",
                    RoleName = x.Role.Name
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.FullName),
                predicate: x => //x.Status == (int)UserStatusEnum.Active || x.Status == (int)UserStatusEnum.PendingVerify &&
                                x.MarketZoneId == apiKey,
                include: z => z.Include(z => z.Role));

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
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<IEnumerable<GetUserResponse>>> SearchAllUserNoSession(int size, int page)
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
                    Status = x.Status,
                    RegisterStoreType = x.RegisterStoreType == (int)StoreTypeEnum.Food ? "Store Product" : "Store Service",
                    RoleName = x.Role.Name
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.FullName),
                predicate: x => x.Status == (int)UserStatusEnum.Active
                             && x.MarketZoneId == apiKey
                             && x.UserSessions.Any(z => z.Status == SessionStatusEnum.Active.GetDescriptionFromEnum()) == false,
                //&& x.Role.Name != RoleEnum.Admin.GetDescriptionFromEnum(),
                include: z => z.Include(z => z.Role).Include(z => z.UserSessions));

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
                    MetaData = null
                };
            }
        }
        public async Task<ResponseAPI<User>> SearchUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Id == UserId
                && (x.Status == (int)UserStatusEnum.Active || x.Status == (int)UserStatusEnum.PendingVerify),
                include: user => user
                        .Include(y => y.Wallets).ThenInclude(a => a.WalletType)
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
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber.Trim()))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidPhoneNumber
                };
            }
            string role = GetRoleFromJwt();
            await _util.CheckUserSession(GetUserIdFromJwt());
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                    (predicate: x => x.Id == userId && x.Status == (int)UserStatusEnum.Active,
                    include: rf => rf.Include(z => z.UserRefreshTokens));
            if (user == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.NotFoundUser
                };
            }
            if (role == RoleEnum.Admin.GetDescriptionFromEnum())
            {
                if (req.Status == UserStatusEnum.Ban)
                {
                    _unitOfWork.GetRepository<UserRefreshToken>().DeleteRangeAsync(user.UserRefreshTokens);
                }
                user.Status = (int)(req.Status ?? (int)UserStatusEnum.Active);
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
            await _util.CheckUserSession(GetUserIdFromJwt());
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync
                (predicate: x => x.Id == UserId && x.Status == (int)UserStatusEnum.Active,
                 include: z => z.Include(a => a.UserRefreshTokens)
                                .Include(a => a.UserSessions)
                                .Include(a => a.UserStoreMappings)
                                .Include(a => a.Wallets));
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
                    _unitOfWork.GetRepository<UserRefreshToken>().DeleteRangeAsync(user.UserRefreshTokens);
                    if (user.UserSessions.Count > 0)
                    {
                        foreach (var session in user.UserSessions)
                        {
                            if (session.Status == SessionStatusEnum.Active.GetDescriptionFromEnum())
                            {
                                session.Status = SessionStatusEnum.Canceled.GetDescriptionFromEnum();
                                _unitOfWork.GetRepository<UserSession>().UpdateAsync(session);
                            }
                        }
                    }
                    //delete store include menu and product
                    if (user.StoreId != null)
                    {
                        await _storeService.DeleteStore((Guid)user.StoreId);
                        if (user.UserStoreMappings.Count > 0)
                        {
                            _unitOfWork.GetRepository<UserStoreMapping>().DeleteRangeAsync(user.UserStoreMappings);
                        }
                    }
                    if (user.Wallets.Count > 0)
                    {
                        user.Wallets.SingleOrDefault().Deflag = true;
                        _unitOfWork.GetRepository<Wallet>().UpdateRange(user.Wallets);
                    }
                    _unitOfWork.GetRepository<User>().UpdateAsync(user);
                    await _unitOfWork.CommitAsync();

                    return new ResponseAPI()
                    {
                        MessageResponse = UserMessage.DeleteUserSuccess,
                        StatusCode = HttpStatusCodes.OK,
                        Data = new
                        {
                            UserId = user.Id
                        }
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
            if (marketzone == null)
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
        //Dashboard
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
                                                       && x.CrDate <= endDate && x.Status == OrderStatus.Completed, null, null);
            var ordersCash = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.Status == OrderStatus.Completed
                                                       && x.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum()
                                                       , null, null);
            var etags = await _unitOfWork.GetRepository<PackageOrder>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.Status == PackageItemStatusEnum.Active.GetDescriptionFromEnum(), null, null);
            var transactions = await _unitOfWork.GetRepository<Transaction>()
                                       .GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate
                                                       && x.Status == TransactionStatus.Success
                                                       && x.Amount > 0
                                                       && x.Type != "WithdrawMoney",
                                                       null, null);

            var deposits = await _unitOfWork.GetRepository<CustomerMoneyTransfer>()
                                      .GetListAsync(x => x.CrDate >= startDate
                                                      && x.CrDate <= endDate
                                                      && x.Status == OrderStatus.Completed
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
                   EtagCount = etags.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   OrderCount = orders.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   OrderCash = ordersCash.Count(o => o.CrDate.ToString("MMM") == g.Key),
                   OtherOrder = orders.Count(o => o.CrDate.ToString("MMM") == g.Key) - ordersCash.Count(o => o.CrDate.ToString("MMM") == g.Key)

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
                var ordersCashier = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                      && x.CrDate <= endDate && x.Status == OrderStatus.Completed && x.UserId == GetUserIdFromJwt(), null, null);
                var ordersCashCashier = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                           && x.CrDate <= endDate && x.Status == OrderStatus.Completed
                                                           && x.UserId == GetUserIdFromJwt()
                                                           && x.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum()
                                                           , null, null);
                var transactionsCashier = await _unitOfWork.GetRepository<Transaction>()
                                      .GetListAsync(x => x.CrDate >= startDate
                                                      && x.CrDate <= endDate
                                                      && x.Status == TransactionStatus.Success
                                                      && x.Amount > 0
                                                      && x.Type != "WithdrawMoney"
                                                      && x.UserId == GetUserIdFromJwt(),
                                                      null, null);
                var groupedStaticsCashier = transactionsCashier
              .GroupBy(t => t.CrDate.ToString("MMM")) // Group by month name (e.g., "Oct")
              .Select(g => new
              {
                  Name = g.Key, // Month name
                  TotalTransactions = transactionsCashier.Count(o => o.CrDate.ToString("MMM") == g.Key),
                  TotalAmountFromTransaction = transactionsCashier
                                                .Where(o => o.CrDate.ToString("MMM") == g.Key)
                                                .Sum(o => o.Amount),
                  EtagCount = etags.Count(o => o.CrDate.ToString("MMM") == g.Key),
                  OrderCount = ordersCashier.Count(o => o.CrDate.ToString("MMM") == g.Key),
                  OrderCash = ordersCashCashier.Count(o => o.CrDate.ToString("MMM") == g.Key),
                  OtherOrder = ordersCashier.Count(o => o.CrDate.ToString("MMM") == g.Key) - ordersCashCashier.Count(o => o.CrDate.ToString("MMM") == g.Key)
                  //PackageCount = packages.Count(o => o.CrDate.ToString("MMM") == g.Key)
              }).ToList();
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Get Cashier's Dashboard Successfully!",
                    Data = groupedStaticsCashier
                };
            }
            // case store 
            var storeUser = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == GetUserIdFromJwt());
            var storeId = storeUser.StoreId;
            var storeOrders = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.StoreId == storeId && x.Status == OrderStatus.Completed,
                                                        null, null);
            var storeCashOrders = await _unitOfWork.GetRepository<Order>().GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate && x.StoreId == storeId && x.Status == OrderStatus.Completed && x.Payments.SingleOrDefault().Name == PaymentTypeEnum.Cash.GetDescriptionFromEnum(),
                                                        null, null);
            var transactionsStore = await _unitOfWork.GetRepository<Transaction>()
                                       .GetListAsync(x => x.CrDate >= startDate
                                                       && x.CrDate <= endDate
                                                       && x.Amount > 0
                                                       && x.Status == TransactionStatus.Success
                                                       && x.Type == TransactionType.SellingProduct
                                                       && x.UserId == storeUser.Id,
                                                       null, null);
            var menusStore = await _unitOfWork.GetRepository<Menu>().GetListAsync(x => x.StoreId == storeId && x.Deflag == false, null, null);
            //find products                                                                          
            var menuStore = menusStore.FirstOrDefault();
            var menuIds = menusStore.Select(m => m.Id).ToList();
            var productMappings = await _unitOfWork.GetRepository<MenuProductMapping>()
                .GetListAsync(x => menuIds.Contains(x.MenuId), null, null);
            var distinctProductIds = productMappings.Select(x => x.ProductId).Distinct().Count();


            var productsStore = await _unitOfWork.GetRepository<Product>().GetListAsync(x => x.MenuId == menuStore.Id
                                                                                         && x.Status == "Active", null, null);

            //var productsStore = await _unitOfWork.GetRepository<Product>().GetListAsync(x => x.CrDate >= startDate
            //                                                                               && x.CrDate <= endDate
            //                                                                               && x.
            //                                                                               && x.Status == "Active",null,null);
            var groupedStaticsStore = storeOrders
             .GroupBy(t => t.CrDate.ToString("MMM")) // Group by month name (e.g., "Oct")
             .Select(g => new
             {
                 Name = g.Key, // Month name
                 OrderCount = storeOrders.Count(o => o.CrDate.ToString("MMM") == g.Key), //package 
                 OrderCashCount = storeCashOrders.Count(o => o.CrDate.ToString("MMM") == g.Key),
                 OtherOrderCount = storeOrders.Count(o => o.CrDate.ToString("MMM") == g.Key) - storeCashOrders.Count(o => o.CrDate.ToString("MMM") == g.Key),
                 //TotalAmountFromOrder = g.Sum(t => t.TotalAmount),
                 TotalTransaction = transactionsStore.Count(o => o.CrDate.ToString("MMM") == g.Key),
                 TotalAmountFromTransaction = transactionsStore
                                                .Where(o => o.CrDate.ToString("MMM") == g.Key)
                                                .Sum(o => o.Amount),
                 TotalMenu = menusStore.Count(),
                 TotalProduct = distinctProductIds,
                 //PackageCount = packages.Count(o => o.CrDate.ToString("MMM") == g.Key)
             }).ToList();
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = "Get Store's Dashboard Successfully!",
                Data = groupedStaticsStore
            };
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
                predicate: x => !x.Deflag && x.MarketZoneId == apiKey && x.Status == (int)StoreStatusEnum.InActive
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
                predicate: x => x.Id == StoreId && !x.Deflag && x.Status == (int)StoreStatusEnum.InActive,
                include: z => z.Include(s => s.Wallets)
                               .Include(a => a.Menus).ThenInclude(a => a.MenuProductMappings).ThenInclude(o => o.Product).ThenInclude(a => a.ProductCategory)
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
            if (store.Status == (int)StoreStatusEnum.InActive)
            {
                storeStatus = StoreStatusEnum.InActive.GetDescriptionFromEnum();
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
            await _util.CheckUserSession(GetUserIdFromJwt());
            if (!ValidationUtils.IsPhoneNumber(req.PhoneNumber))
                throw new BadHttpRequestException(PackageItemMessage.PhoneNumberInvalid, HttpStatusCodes.BadRequest);
            if (req.StoreName == null)
                throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);
            var searchName = NormalizeString(req.StoreName);
            var stores = await _unitOfWork.GetRepository<Store>().GetListAsync(predicate: x => x.PhoneNumber == req.PhoneNumber
                                                                                            && x.Status == (int)StoreStatusEnum.InActive,
                                                                               include: z => z.Include(s => s.Wallets));
            var storeTrack = stores.SingleOrDefault(x => NormalizeString(x.Name) == searchName || NormalizeString(x.ShortName) == searchName)
                ?? throw new BadHttpRequestException(StoreMessage.NotFoundStore, HttpStatusCodes.NotFound);
            if (req.Status == "APPROVED")
            {
                storeTrack.Status = (int)StoreStatusEnum.Blocked;
                storeTrack.UpsDate = TimeUtils.GetCurrentSEATime();
                _unitOfWork.GetRepository<Store>().UpdateAsync(storeTrack);
            }
            else if (req.Status != null)
            {
                if (req.Status != "REJECTED")
                    throw new BadHttpRequestException(UserMessage.InvalidTypeOfStatus, HttpStatusCodes.BadRequest);
                if (req.Status == "REJECTED")
                {
                    var storeAccount = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.StoreId == storeTrack.Id);
                    storeAccount.Status = (int)UserStatusEnum.Active;
                    storeAccount.UpsDate = TimeUtils.GetCurrentSEATime();
                    _unitOfWork.GetRepository<User>().UpdateAsync(storeAccount);
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
                    var subject = UserMessage.ResolvedMessage;
                    var body = $"<div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>" +
                                               $"<h1 style='color: #007bff;'>Welcome to our Vega City!</h1>" +
                                               $"<p>Thanks for closing request.</p>" +
                                               $"<p><strong>We have process your closing request and decide the Status will be: {req.Status}</strong></p>" +

                                           $"</div>";
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
        public async Task CheckSession()
        {
            var userSessions = await _unitOfWork.GetRepository<UserSession>().GetListAsync
                (predicate: x => x.Status == SessionStatusEnum.Active.GetDescriptionFromEnum() && x.EndDate < TimeUtils.GetCurrentSEATime());
            foreach (var session in userSessions)
            {
                session.Status = SessionStatusEnum.Expired.GetDescriptionFromEnum();
                _unitOfWork.GetRepository<UserSession>().UpdateAsync(session);
            }
            await _unitOfWork.CommitAsync();
        }
    }
}

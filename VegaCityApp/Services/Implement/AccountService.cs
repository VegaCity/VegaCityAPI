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
        private async Task<User> CreateUserRegister(RegisterRequest req)
        {
            var role = await _unitOfWork.GetRepository<Role>()
                .SingleOrDefaultAsync(predicate: r => r.Name == req.RoleName.Replace(" ", string.Empty));
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                Cccd = req.CCCD,
                Address = req.Address,
                Email = req.Email,
                Description = req.Description,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.MarketZoneId),
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
                WalletType = (int)WalletTypeEnum.UserWallet,
                Balance = 0,
                BalanceHistory = 0,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false
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
        private async Task<Guid> UpdateOtherUserApproving(User user, Guid RoleId)
        {
            user.Status = (int)UserStatusEnum.Active;
            user.IsChange = false;
            if (RoleId == Guid.Parse(EnvironmentVariableConstant.CashierWebId))
            {
                user.Password = PasswordUtil.GenerateCharacter(10);

            }
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
        #region private variable
        private readonly string[] allowedRoles = {
            RoleEnum.CashierWeb.GetDescriptionFromEnum(),
            RoleEnum.Store.GetDescriptionFromEnum(),
            RoleEnum.Admin.GetDescriptionFromEnum(),
            RoleEnum.CashierApp.GetDescriptionFromEnum()
        };
        #endregion
        public async Task<ResponseAPI> Login(LoginRequest req)
        {
            Tuple<string, Guid> guidClaim = null;
            if (!ValidationUtils.IsEmail(req.Email))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail
                };
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Email == req.Email && x.MarketZoneId == Guid.Parse(EnvironmentVariableConstant.MarketZoneId),
                include: User => User.Include(y => y.Role));
            if (user == null)
            {
                return new ResponseAPI
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
                            return new ResponseAPI
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
                            return new ResponseAPI
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
                        return await _unitOfWork.CommitAsync() > 0? new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = UserMessage.LoginSuccessfully,
                            Data = new
                            {
                                UserId = user.Id,
                                AccessToken = token,
                                RefreshToken = tokenRefresh
                            }
                        }: new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.BadRequest,
                            MessageResponse = UserMessage.SaveRefreshTokenFail
                        };
                    }
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.WrongPassword
                    };
                case (int)UserStatusEnum.PendingVerify:
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.PendingVerify
                    };
                case (int)UserStatusEnum.Disable:
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserDisable
                    };
                case (int)UserStatusEnum.Ban:
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.BadRequest,
                        MessageResponse = UserMessage.UserBan
                    };
            }

            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.LoginFail
            };

        }
        public async Task<ResponseAPI> RefreshToken(ReFreshTokenRequest req)
        {
            Tuple<string, Guid> guidClaim = null;
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                               predicate: x => x.Email == req.Email,
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
                    Token = user.Role.Name == RoleEnum.Admin.GetDescriptionFromEnum() ? JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(1)) : JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(2)),
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
                    Token = user.Role.Name == RoleEnum.Admin.GetDescriptionFromEnum() ? JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(1)) : JwtUtil.GenerateRefreshToken(user, guidClaim, TimeUtils.GetCurrentSEATime().AddDays(2)),
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

            if (!ValidationUtils.IsCCCD(req.CCCD))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidCCCD
                };
            }

            //check if email is already exist
            var emailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email);
            if (emailExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EmailExist
                };
            }
            var phoneNumberExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.PhoneNumber == req.PhoneNumber);
            if (phoneNumberExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.PhoneNumberExist
                };
            }
            var cccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Cccd == req.CCCD);
            if (cccdExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.CCCDExist
                };
            }

            //create new user
            var newUser = await CreateUserRegister(req);
            //create refesh token
            var refresh = new ReFreshTokenRequest
            {
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

            if (!ValidationUtils.IsCCCD(req.CCCD))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidCCCD,
                };
            }

            var emailExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email);
            if (emailExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EmailExist
                };
            }
            var phoneNumberExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.PhoneNumber == req.PhoneNumber);
            if (phoneNumberExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.PhoneNumberExist
                };
            }
            var cccdExist = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Cccd == req.CCCD);
            if (cccdExist != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.CCCDExist
                };
            }

            var newUser = await CreateUserRegister(req);
            //create refesh token
            var refresh = new ReFreshTokenRequest
            {
                Email = newUser.Email,
                RefreshToken = null
            };
            var token = await RefreshToken(refresh);
            if (newUser.Id != Guid.Empty)
            {
                if (newUser.Role.Name == RoleEnum.Store.GetDescriptionFromEnum())
                {
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
                            RefreshToken = token.Data
                        }
                    };
                }
                else
                {
                    //send mail
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
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.CreateUserFail
            };
        }
        public async Task<ResponseAPI> ApproveUser(Guid userId, ApproveRequest req)
        {
            var user = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(predicate: x => x.Id == userId);
            if (user.Status == (int)UserStatusEnum.Active)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.Approved,
                    Data = new
                    {
                        UserId = user.Id
                    }
                };
            }
            if (req.ApprovalStatus.Equals(ApproveStatus.REJECT))
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
            else if(req.ApprovalStatus.Equals(ApproveStatus.APPROVED))
            {
                //check phone, email
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
                if(user.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierWebId))
                {
                    var result = await UpdateOtherUserApproving(user, user.RoleId);
                    if (result != Guid.Empty)
                    {

                        //send mail
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

                        }
                    }

                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.Created,
                            MessageResponse = UserMessage.ApproveSuccessfully,
                            Data = new
                            {
                                UserId = result,
                            }
                        };
                    }
                }
                if (user.RoleId == Guid.Parse(EnvironmentVariableConstant.StoreId))
                {
                    var newStore = new Store
                    {
                        Id = Guid.NewGuid(),
                        Name = req.StoreName,
                        Address = req.StoreAddress,
                        PhoneNumber = req.PhoneNumber,
                        Email = req.StoreEmail,
                        Status = (int)StoreStatusEnum.Closed,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        MarketZoneId = Guid.Parse(EnvironmentVariableConstant.MarketZoneId),
                        Deflag = false,
                    };
                    await _unitOfWork.GetRepository<Store>().InsertAsync(newStore);
                    //create store wallet
                    var storeWallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        WalletType = (int)WalletTypeEnum.StoreWallet,
                        Balance = 0,
                        BalanceHistory = 0,
                        CrDate = TimeUtils.GetCurrentSEATime(),
                        UpsDate = TimeUtils.GetCurrentSEATime(),
                        Deflag = false,
                        StoreId = newStore.Id
                    };
                    await _unitOfWork.GetRepository<Wallet>().InsertAsync(storeWallet);
                    await _unitOfWork.CommitAsync();
                    //update user
                    var result = await UpdateUserApproving(user, newStore.Id);
                    if (result != Guid.Empty)
                    {

                        //send mail
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
                }
    
            }

            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.ApproveFail
            };
        }
        public async Task<ResponseAPI> ChangePassword(ChangePasswordRequest req)
        {
            if (!ValidationUtils.IsEmail(req.Email))
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidEmail
                };
            }

            var user = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(predicate: x => x.Email == req.Email, include: user => user.Include(x => x.Role));
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
                if(allowedRoles.Contains(user.Role.Name))
                {
                    if (user.Password == req.OldPassword)
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
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.PasswordIsChanged
            };
        }
        public async Task<IPaginate<GetUserResponse>> SearchAllUser(int size, int page)
        {
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
                    Cccd = x.Cccd,
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
                predicate: x => x.Status == (int)UserStatusEnum.Active
            );
            return data;

           

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
                    MessageResponse = MessageConstant.UserMessage.NotFoundUser,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = UserMessage.GetListSuccess,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new
                {
                    user
                }
            };
        }
        public async Task<ResponseAPI> UpdateUser(Guid userId, UpdateUserAccountRequest req)
        {
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
            user.FullName = req.FullName;
            user.PhoneNumber = req.PhoneNumber;
            user.Birthday = req.Birthday;
            user.Gender = req.Gender;
            user.ImageUrl = req.ImageUrl;
            user.Address = req.Address;
            user.Description = req.Description;
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
    }
}

using AutoMapper;
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

        //not done yet
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
                    if (user.RoleId != Guid.Parse(EnvironmentVariableConstant.CashierAppId))
                        
                    {
                        if (user.Password == PasswordUtil.HashPassword(req.Password))
                        {
                            guidClaim = new Tuple<string, Guid>("MarketZoneId", (Guid)user.MarketZoneId);
                            //generate token
                            var token = JwtUtil.GenerateJwtToken(user, guidClaim);
                            return new ResponseAPI
                            {
                                StatusCode = HttpStatusCodes.OK,
                                MessageResponse = UserMessage.LoginSuccessfully,
                                Data = new
                                {
                                    UserId = user.Id,
                                    AccessToken = token
                                }
                            };
                        }
                    }
                    if (user.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierAppId) && user.PinCode == (req.Password))
                    {
                        guidClaim = new Tuple<string, Guid>("MarketZoneId", (Guid)user.MarketZoneId);
                        //generate token
                        var token = JwtUtil.GenerateJwtToken(user, guidClaim);
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.OK,
                            MessageResponse = UserMessage.LoginSuccessfully,
                            Data = new
                            {
                                UserId = user.Id,
                                AccessToken = token
                            }
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
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Email == req.Email
                && x.PhoneNumber == req.PhoneNumber && x.Cccd == req.CCCD);
            if (user != null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EmailExistOrPhoneOrCCCDExit
                };
            }

            //create new user
            var newUser = await CreateUserRegister(req);
            if (newUser != Guid.Empty)
            {
                //create wallet
                var result = await CreateUserWallet(newUser);
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
                        UserId = newUser
                    }
                };
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.CreateUserFail
            };
        }

        public async Task<ResponseAPI> AdminCreateUser(CreateUserRequest req)
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

            if (!ValidationUtils.IsCCCD(req.Cccd))
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.InvalidCCCD,
                };
            }

            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x =>
                x.Email == req.Email && x.PhoneNumber == req.PhoneNumber && x.Cccd == req.Cccd);
            if (user != null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = UserMessage.EmailExistOrPhoneOrCCCDExit
                };
            }

            var newUser = await CreateUserRole(req);
            if (newUser != Guid.Empty)
            {
                var result = await CreateUserWallet(newUser);
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
                        UserId = newUser
                        
                    }
                };
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.CreateUserFail
            };

        }

        private async Task<Guid> CreateUserRegister(RegisterRequest req)
        {
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
                RoleId = Guid.Parse(EnvironmentVariableConstant.StoreId),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Gender = (int)GenderEnum.Other,
                Status = (int)UserStatusEnum.PendingVerify
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            return await _unitOfWork.CommitAsync() > 0 ? newUser.Id : Guid.Empty;
        }

        private async Task<Guid> CreateUserRole(CreateUserRequest req)
        {;
            var role = await _unitOfWork.GetRepository<Role>()
                .SingleOrDefaultAsync(predicate: r => r.Name == req.RoleName.Replace(" ", string.Empty));
            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                Cccd = req.Cccd,
                Address = req.Address,
                Email = req.Email,
                Birthday = req.Birthday,
                Gender = (int)GenderEnum.Other,
                Description = req.Description,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.MarketZoneId),
                RoleId = role.Id,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Status = (int)UserStatusEnum.PendingVerify
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            return await _unitOfWork.CommitAsync() > 0 ? newUser.Id : Guid.Empty;
        }
     
        private async Task<bool> CreateUserWallet(Guid userId)
        {
            var newWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WalletType = (int) WalletTypeEnum.StoreWallet,
                Balance = 0,
                BalanceHistory = 0,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Deflag = false
            };
            await _unitOfWork.GetRepository<Wallet>().InsertAsync(newWallet);
            return await _unitOfWork.CommitAsync() > 0;
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
                            var subject = UserMessage.ApproveSuccessfully;
                            var body = "Your account has been approved. Your password is: " + user.Password;
                            await MailUtil.SendMailAsync(user.Email, subject, body);
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
                    await _unitOfWork.CommitAsync();
                    //update user
                    var result = await UpdateUserApproving(user, newStore.Id);
                    if (result != Guid.Empty)
                    {

                        //send mail
                        if (user != null)
                        {
                            var subject = UserMessage.ApproveSuccessfully;
                            var body = "Your account has been approved. Your password is: " + user.Password;
                            await MailUtil.SendMailAsync(user.Email, subject, body);
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

                else
                {
                    if (user.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierAppId))
                    {
                        var result = await UpdateOtherUserApproving(user, user.RoleId);
                        if (result != Guid.Empty)
                        {

                            //send mail
                            if (user != null)
                            {
                                var subject = UserMessage.ApproveSuccessfully;
                                var body = "Your account has been approved. Your SignIn Pin Code is: " + user.PinCode;
                                await MailUtil.SendMailAsync(user.Email, subject, body);
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

                }
    
            }

            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = UserMessage.ApproveFail
            };
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

            if (RoleId == Guid.Parse(EnvironmentVariableConstant.CashierAppId))
            {
                user.PinCode = PasswordUtil.GeneratePinCode();
            }
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return user.Id;
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
                .SingleOrDefaultAsync(predicate: x => x.Email == req.Email);
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
                if(user.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierWebId) || user.RoleId == Guid.Parse(EnvironmentVariableConstant.StoreId))
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
                }else if(user.RoleId == Guid.Parse(EnvironmentVariableConstant.CashierAppId))
                {
                    if (user.PinCode == req.OldPassword)
                    {
                        user.PinCode = PasswordUtil.HashPassword(req.NewPassword);
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
                predicate: x => x.Id == UserId,
                include: user => user
                        .Include(y => y.Wallets)
                        .Include(y => y.Store)
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
                    (predicate: x => x.Id == userId && x.Status.Equals(UserStatusEnum.Active));
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
                (predicate: x => x.Id == UserId && x.Status.Equals(UserStatusEnum.Active));
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

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.RoleResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.API.Services;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Payload.Request;
using VegaCityApp.Repository.Interfaces;
using VegaCityApp.Service.Interface;
using static VegaCityApp.API.Constants.MessageConstant;
using GetUserResponse = VegaCityApp.API.Payload.Response.GetUserResponse;

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
                predicate: x => x.Email == req.Email,
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
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
                RoleId = Guid.Parse(EnvironmentVariableConstant.StoreId),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                Gender = (int)GenderEnum.Other,
                Status = (int)UserStatusEnum.PendingVerify
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            await _unitOfWork.CommitAsync();
            return newUser.Id;
        }

        public async Task<ResponseAPI> ApproveUser(ApproveRequest req)
        {
            var user = await _unitOfWork.GetRepository<User>()
                .SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(req.UserId));
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

            if (req.ApprovalStatus == "REJECT")
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
            else if (req.ApprovalStatus == "APPROVED")
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

                //create store
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
                    MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
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
            else
            {
                if (user.Password == PasswordUtil.HashPassword(req.OldPassword))
                {
                    user.Password = PasswordUtil.HashPassword(req.NewPassword);
                    _unitOfWork.GetRepository<User>().UpdateAsync(user);
                    await _unitOfWork.CommitAsync();
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = UserMessage.OldPasswordNotDuplicate
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

        //nguyentppse161945

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
                    IsChange = x.IsChange,
                    Status = x.Status
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.FullName),
                predicate: x => x.Status != 2
            );
            return data;

           

        }


        public async Task<ResponseAPI> SearchUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: x => x.Id == UserId,
                include: user => user
                    .Include(y => y.Orders)
                    .Include(y => y.UserWallets)
                    .Include(y => y.Etags)
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
                    User = new
                    {
                        user.Id,
                        user.FullName,
                        user.PhoneNumber,
                        user.Birthday,
                        user.StoreId,
                        user.CrDate,
                        user.UpsDate,
                        user.Gender,
                        user.Cccd,
                        user.ImageUrl,
                        user.PinCode,
                        user.MarketZoneId,
                        user.Email,
                        user.Password,
                        user.RoleId,
                        user.Description,
                        user.IsChange,
                        user.Address,
                        user.Status
                    },
                    Orders = user.Orders.Select(o => new
                    {
                        o.Id,
                        o.Status,
                        o.TotalAmount
                    }),
                    UserWallets = user.UserWallets.Select(w => new
                    {
                        w.Id,
                        w.WalletType,
                        w.CrDate,
                        w.UpsDate,
                        w.Balance,
                        w.BalanceHistory,
                        w.Deflag,
                        w.EtagId
                    }),
                    Etags = user.Etags.Select(e => new
                    {
                        e.Id,
                        e.EtagType,
                    })
                }
            };
        }

        //public async Task<GetUserResponse> UpdateUserById(UpdateUserAccountRequest req, Guid userId)
        //{
        //    var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == userId);
        //    if (user == null)
        //    {
        //        return new GetUserResponse()
        //        {
        //            StatusCode = HttpStatusCodes.NotFound,
        //            MessageResponse = UserMessage.NotFoundUser
        //        };
        //    }

        //    user.FullName = req.FullName;
        //    user.PhoneNumber = req.PhoneNumber;
        //    user.Birthday = req.Birthday;
        //    user.Gender = req.Gender;
        //    user.ImageUrl = req.ImageUrl;
        //    user.Address = req.Address;
        //    user.Description = req.Description;
        //    _unitOfWork.GetRepository<User>().UpdateAsync(user);
        //    await _unitOfWork.CommitAsync();
        //    return new GetUserResponse()
        //    {
        //        StatusCode = HttpStatusCodes.OK,
        //        MessageResponse = UserMessage.UpdateUserSuccessfully,
        //        Email = user.Email,
        //        FullName = user.FullName,
        //        Address = user.Address,
        //        RoleId = user.RoleId,
        //        userWallets = new List<GetWalletResponse>(),
        //        PhoneNumber = user.PhoneNumber,
        //        Birthday = user.Birthday,
        //        Gender = user.Gender,
        //        ImageUrl = user.ImageUrl,
        //        Cccd = user.Cccd,
        //        MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),

        //    };

        //}
        public async Task<ResponseAPI> UpdateUser(UpdateUserAccountRequest req)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == req.UserId);
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
                    MessageResponse = MessageConstant.UserMessage.UpdateUserSuccessfully
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.BadRequest,
                    MessageResponse = MessageConstant.UserMessage.FailedToUpdate
                };
        }

        public async Task<ResponseAPI> DeleteUser(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == UserId);
            if (user == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.UserMessage.NotFoundUser
                };
            }

            user.Status = 1;
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = MessageConstant.UserMessage.DeleteUserSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = MessageConstant.UserMessage.DeleteUserFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }


    }
}

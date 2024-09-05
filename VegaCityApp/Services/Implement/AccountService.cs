using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.ETagResponse;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.API.Payload.Response.RoleResponse;
using VegaCityApp.API.Payload.Response.StoreResponse;
using VegaCityApp.API.Payload.Response.UserResponse;
using VegaCityApp.API.Payload.Response.WalletResponse;
using VegaCityApp.API.Services;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Payload.Request;
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

        public async Task<GetListUserResponse> GetUserList(GetListParameterRequest req)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var allUsers = await userRepo.GetListAsync();
            var response = new GetListUserResponse();
            try
            {
                IEnumerable<User> filteredUsers = allUsers;

                if (req != null)
                {
                    if (!string.IsNullOrEmpty(req.Search))
                    {
                        filteredUsers = filteredUsers
                            .Where(x => x.FullName.Contains(req.Search) || x.Address.Contains(req.Search));
                    }

                    if (req.Page.HasValue && req.PageSize.HasValue)
                    {
                        var skip = (req.Page.Value - 1) * req.PageSize.Value;
                        filteredUsers = filteredUsers.Skip(skip).Take(req.PageSize.Value);
                    }
                }

                if (!filteredUsers.Any())
                {
                    response.StatusCode = MessageConstant.HttpStatusCodes.NotFound;
                    response.MessageResponse = UserMessage.NotFoundUser;
                    return response;
                }

                response.StatusCode = MessageConstant.HttpStatusCodes.OK;
                response.MessageResponse = UserMessage.GetListSuccess;
                response.Users = filteredUsers.ToList();

            }
            catch (Exception ex)
            {
                response.StatusCode = MessageConstant.HttpStatusCodes.InternalServerError;
                response.MessageResponse = $"An error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<GetListUserResponse> GetListUserByUserRoleId(Guid RoleId)
        {
            var response = new GetListUserResponse();
            //get users with RoleId through <User>
            var result = await _unitOfWork.GetRepository<User>().GetListAsync(predicate: x => x.RoleId == RoleId);
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Id == RoleId);

            if (result != null && result.Any())
            {
                response.StatusCode = HttpStatusCodes.OK;
                response.MessageResponse = RoleMessage.GetListByRoleSuccessfully;
                response.Users = result.ToList();
            }
            else
            {
                response.StatusCode = 404;
                response.MessageResponse = RoleMessage.GetListByRoleNotFound;
                response.Users = new List<User>(); // Empty list instead of null
            }

            return response;
        } //assign role id to test in user

        public async Task<GetUserResponse> GetUserDetail(Guid UserId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == UserId);
            if (user == null)
            {
                return new GetUserResponse()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = UserMessage.UserNotFound
                };
            }

            var userRole = (await _unitOfWork.GetRepository<Role>()
                .SingleOrDefaultAsync(predicate: x => x.Id == user.RoleId))?.Name;
            var userStore = (await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == user.StoreId))?.Name;
            var userStoreShortName =
                (await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == user.StoreId))
                ?.ShortName;
            var userHouseId = (await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == user.StoreId))?.HouseId;
            var userHouseName =
                (await _unitOfWork.GetRepository<House>().SingleOrDefaultAsync(predicate: x => x.Id == userHouseId))
                ?.HouseName;
            var userMarketZone = (await _unitOfWork.GetRepository<MarketZone>()
                .SingleOrDefaultAsync(predicate: x => x.Id == user.MarketZoneId))?.Name;
            var userStoreType =
                (await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(predicate: x => x.Id == user.StoreId))
                ?.StoreType;
            var userWallets = await _unitOfWork.GetRepository<UserWallet>()
                .GetListAsync(predicate: x => x.UserId == UserId);
            var userETags = await _unitOfWork.GetRepository<Etag>().GetListAsync(predicate: x => x.UserId == UserId);
            var userOrders = await _unitOfWork.GetRepository<Order>().GetListAsync(predicate: x => x.UserId == UserId);
            var walletResponses = new List<GetWalletResponse>();
            var etagResponses = new List<GetETagResponse>();
            var orderResponses = new List<GetOrderResponse>();
            foreach (var wallet in userWallets)
            {
                walletResponses.Add(new GetWalletResponse()
                {
                    WalletId = wallet.Id,
                    Balance = wallet.Balance,
                    WalletType = wallet.WalletType,
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = walletMessage.FoundSuccess
                });
            }

            foreach (var etag in userETags)
            {
                etagResponses.Add(new GetETagResponse()
                {
                    EtagTypeId = etag.EtagTypeId,
                    MarketZoneId = etag.MarketZoneId,
                    Qrcode = etag.Qrcode,
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = EtagMessage.GetEtagsSuccess
                });
            }

            foreach (var order in userOrders)
            {
                orderResponses.Add(new GetOrderResponse()
                {
                    Name = order.Name,
                    UserId = order.UserId,
                    TotalAmount = order.TotalAmount,
                    CrDate = order.CrDate,
                    UpsDate = order.UpsDate,
                    StoreId = order.StoreId,
                    EtagId = order.EtagId,
                    PaymentType = order.PaymentType,
                    InvoiceId = order.InvoiceId,
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = OrderMessage.GetOrdersSuccessfully
                });
            }

            var storeIsDeflag = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.Id == user.Id && x.Deflag == true);
            var response = new GetUserResponse()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = UserMessage.GetListSuccess,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Birthday = user.Birthday,
                Gender = user.Gender,
                Description = user.Description,
                Cccd = user.Cccd,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                PinCode = user.PinCode,
                MarketZoneId = user.MarketZoneId,
                StoreId = user.StoreId,
                RoleId = user.RoleId,
                Role = new GetRoleResponse()
                {
                    Id = user.RoleId,
                    Name = userRole
                },
                Store = new GetStoreResponse()
                {
                    Id = user.StoreId,
                    StoreType = userStoreType,
                    Name = userStore,
                    Address = user.Address,
                    CrDate = user.CrDate,
                    UpsDate = user.UpsDate,
                    Deflag = storeIsDeflag?.Deflag,
                    PhoneNumber = user.PhoneNumber,
                    ShortName = userStoreShortName,
                    Email = user.Email,
                    HouseId = userHouseId,
                    MarketZoneId = user.MarketZoneId,
                    Description = user.Description,
                    Status = user.Status,
                    //House =  
                    //MarketZone =  
                    DisputeReports = new List<DisputeReport>(),
                    Menus = new List<Menu>(),
                    Orders = new List<Order>(),
                    ProductCategories = new List<ProductCategory>(),
                    Users = new List<User>()

                },

                orders = orderResponses,
                userWallets = walletResponses, // Assign the list of wallet responses
                Etags = etagResponses
            };
            return response;

        }

        public async Task<GetUserResponse> UpdateUserById(UpdateUserAccountRequest req, Guid userId)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == userId);
            if (user == null)
            {
                return new GetUserResponse()
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
            await _unitOfWork.CommitAsync();
            return new GetUserResponse()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = UserMessage.UpdateUserSuccessfully,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                RoleId = user.RoleId,
                userWallets = new List<GetWalletResponse>(),
                PhoneNumber = user.PhoneNumber,
                Birthday = user.Birthday,
                Gender = user.Gender,
                ImageUrl = user.ImageUrl,
                Cccd = user.Cccd,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),

            };

        }

        //public async Task<ResponseAPI> DeleteUserById(Guid UserId)
        //{
        //    var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == UserId);
        //    if (user == null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            StatusCode = HttpStatusCodes.NotFound,
        //            MessageResponse = UserMessage.NotFoundUser
        //        };
        //    }

        //    _unitOfWork.GetRepository<User>().DeleteAsync(user);
        //    await _unitOfWork.CommitAsync();
        //    return new ResponseAPI()
        //    {
        //        StatusCode = HttpStatusCodes.OK,
        //        MessageResponse = UserMessage.UpdateUserSuccessfully
        //    };
        //}


    }
}

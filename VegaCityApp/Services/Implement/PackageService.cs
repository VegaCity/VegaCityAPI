using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pos_System.API.Constants;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Domain.Paginate;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class PackageService: BaseService<PackageService>, IPackageService
    {
        public PackageService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PackageService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        public async Task<ResponseAPI> CreatePackage(CreatePackageRequest req)
        {
            var result = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name);
            if (result != null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.ExistedPackageName, 
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            DateTime currentDate = DateTime.UtcNow;
            if (req.EndDate < currentDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.EndateInThePast,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate == req.StartDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.SameStrAndEndDate,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }

            TimeSpan? duration = req.EndDate - req.StartDate;
            if (duration.HasValue)
            {
                double totalHours = duration.Value.TotalHours;
                if (totalHours < 48)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = MessageConstant.PackageMessage.durationLimit,
                        StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                    };
                }
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.InvalidDuration,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            //var adminId = Guid.Parse(EnvironmentVariableConstant.AdminId);
            //var isAdmin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == UserId && x.RoleId == adminId);
            //if (isAdmin == null)
            //{
            //    return new ResponseAPI
            //    {
            //        MessageResponse = MessageConstant.UserMessage.UnauthorizedAccess,
            //        StatusCode = MessageConstant.HttpStatusCodes.Unauthorized
            //    };
            //}
            var newPackage = new Package()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId),
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
            };
            await _unitOfWork.GetRepository<Package>().InsertAsync(newPackage);
            var response = new ResponseAPI()
            {
                MessageResponse = MessageConstant.PackageMessage.CreatePackageSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = newPackage.Id

            };
            int check = await _unitOfWork.CommitAsync();

            return check > 0 ? response : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = PackageMessage.CreatePackageFail
            };
        }

        public async Task<ResponseAPI> UpdatePackage(UpdatePackageRequest req)
        {
          
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageId);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.NotFoundPackage
                };
            }
            DateTime currentDate = DateTime.UtcNow;
            if (req.EndDate < currentDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.EndateInThePast,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate == req.StartDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.SameStrAndEndDate,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }

            TimeSpan? duration = req.EndDate - req.StartDate;
            if (duration.HasValue)
            {
                double totalHours = duration.Value.TotalHours;
                if (totalHours < 48)
                {
                    return new ResponseAPI()
                    {
                        MessageResponse = MessageConstant.PackageMessage.durationLimit,
                        StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                    };
                }
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.InvalidDuration,
                    StatusCode = MessageConstant.HttpStatusCodes.BadRequest
                };
            }
            package.Name = req.Name;
            package.Description = req.Description;
            package.Price = req.Price;
            package.StartDate = req.StartDate;
            package.EndDate = req.EndDate;
            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.UpdatePackageSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.UpdatePackageFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }

        public async Task<IPaginate<GetPackageResponse>> SearchAllPackage(int size, int page)
        {
            IPaginate<GetPackageResponse> data = await _unitOfWork.GetRepository<Package>().GetPagingListAsync(

                selector: x => new GetPackageResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    MarketZoneId = x.MarketZoneId,
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Deflag == false
            );
            return data;
        }
        //public async Task<GetListPackageResponse> GetListPackage(GetListParameterRequest req)
        //{
        //    var packageRepo = _unitOfWork.GetRepository<Package>();
        //    var allPackages = await packageRepo.GetListAsync();
        //    var response = new GetListPackageResponse();
        //    try
        //    {
        //        IEnumerable<Package> filtereds = allPackages;
        //        if (req != null)
        //        {
        //            if (!string.IsNullOrEmpty(req.Search))
        //            {
        //                filtereds = filtereds
        //                    .Where(x => x.Name.Contains(req.Search) || x.Description.Contains(req.Search));
        //            }
        //            if (req.Page.HasValue && req.PageSize.HasValue)
        //            {
        //                var skip = (req.Page.Value - 1) * req.PageSize.Value;
        //                filtereds = filtereds.Skip(skip).Take(req.PageSize.Value);
        //            }
        //        }



        //        if (!filtereds.Any())
        //        {
        //            response.StatusCode = MessageConstant.HttpStatusCodes.NotFound;
        //            response.MessageResponse = UserMessage.NotFoundUser;
        //            return response;
        //        }
        //        response.StatusCode = MessageConstant.HttpStatusCodes.OK;
        //        response.MessageResponse = UserMessage.GetListSuccess;
        //        response.Packages = filtereds.ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        response.StatusCode = MessageConstant.HttpStatusCodes.InternalServerError;
        //        response.MessageResponse = $"An error occurred: {ex.Message}";
        //    }

        //    return response;
        //}

        //public async Task<GetPackageResponse> GetPackageDetail(Guid PackageId)
        //{
        //    var isPackage = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == PackageId);
        //    if (isPackage == null)
        //    {
        //        return new GetPackageResponse()
        //        {
        //            StatusCode = HttpStatusCodes.NotFound,
        //            MessageResponse = MessageConstant.PackageMessage.NotFoundPackage
        //        };
        //    }

        //    return new GetPackageResponse()
        //    {
        //        Id = isPackage.Id,
        //        Name = isPackage.Name,
        //        Description = isPackage.Description,
        //        Price = isPackage.Price,
        //        StartDate = isPackage.StartDate,
        //        EndDate = isPackage.EndDate,
        //        MarketZoneId = isPackage.MarketZoneId,
        //        CrDate = isPackage.CrDate,
        //        UpsDate = isPackage.UpsDate,
        //        MarketZone = isPackage.MarketZone,
        //        PackageETagTypeMappings = isPackage.PackageETagTypeMappings.ToList(),
        //        StatusCode = HttpStatusCodes.Found,
        //        MessageResponse = MessageConstant.PackageMessage.FoundPackage,
        //    };
        //}
        public async Task<ResponseAPI> SearchPackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageId && x.Deflag==false,
                include: user => user
                    .Include(y => y.PackageETagTypeMappings)
                    .Include(y => y.MarketZone)
            );

            if (package == null)
            {
                return new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.NotFoundPackage,
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound
                };
            }

            return new ResponseAPI()
            {
                MessageResponse = PackageMessage.GetPackagesSuccessfully,
                StatusCode = MessageConstant.HttpStatusCodes.OK,
                Data = new
                {
                   package
                   
                }
            };
        }

        public async Task<ResponseAPI> DeletePackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == PackageId);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.NotFoundPackage
                };
            }

            package.Deflag = true;
            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.DeleteSuccess,
                    StatusCode = HttpStatusCodes.OK
                }
                : new ResponseAPI()
                {
                    MessageResponse = MessageConstant.PackageMessage.DeleteFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
    }
}

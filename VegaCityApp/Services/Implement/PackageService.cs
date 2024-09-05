using AutoMapper;
using Pos_System.API.Constants;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PackageResponse;
using VegaCityApp.API.Payload.Response.UserResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;
using static VegaCityApp.API.Constants.MessageConstant;

namespace VegaCityApp.API.Services.Implement
{
    public class PackageService: BaseService<PackageService>, IPackageService
    {
        public PackageService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PackageService> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(unitOfWork, logger, httpContextAccessor, mapper)
        {
        }

        //public async Task<ResponseAPI> CreateEtagType(EtagTypeRequest req)
        //{
        //    var newEtagType = new EtagType
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = req.Name,
        //        ImageUrl = req.ImageUrl,
        //        MarketZoneId = Guid.Parse(EnvironmentVariableConstant.ZoneId)
        //    };
        //    await _unitOfWork.GetRepository<EtagType>().InsertAsync(newEtagType);
        //    return await _unitOfWork.CommitAsync() > 0 ? new ResponseAPI()
        //    {
        //        MessageResponse = MessageConstant.EtagTypeMessage.CreateSuccessFully,
        //        StatusCode = MessageConstant.HttpStatusCodes.Created,
        //        Data = new {
        //            EtagTypeId = newEtagType.Id,
        //        }
        //    } : new ResponseAPI()
        //    {
        //        MessageResponse = MessageConstant.EtagTypeMessage.CreateFail,
        //        StatusCode = MessageConstant.HttpStatusCodes.BadRequest
        //    };
        //}
        //public async Task<ResponseAPI> CreatePackage(CreatePackageRequest req, Guid UserId)
        //{
        //    var adminId = Guid.Parse(EnvironmentVariableConstant.AdminId);
        //    var isAdmin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == UserId && x.RoleId == adminId);
        //    var existedPackageName = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Name == req.Name);
        //    if(existedPackageName != null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = MessageConstant.PackageMessage.ExistedPackageName,
        //            StatusCode = MessageConstant.HttpStatusCodes.BadRequest
        //        };
        //    }

        //    if(isAdmin == null)
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = MessageConstant.UserMessage.UnauthorizedAccess,
        //            StatusCode = MessageConstant.HttpStatusCodes.Unauthorized
        //        };

        //    }
        //    var etagType = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.ETagTypeId);
        //    if (etagType == null)
        //    {
        //        return new ResponseAPI()
        //        {
        //            MessageResponse = MessageConstant.PackageMessage.NotFoundETagType,
        //            StatusCode = MessageConstant.HttpStatusCodes.NotFound
        //        };
        //    }
        //    var marketZone = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate:x => x.Id == etagType.MarketZoneId);
        //    if (marketZone == null)
        //    {
        //        return new ResponseAPI
        //        {
        //            MessageResponse = MessageConstant.PackageMessage.MKZoneNotFound,
        //            StatusCode = MessageConstant.HttpStatusCodes.NotFound
        //        };
        //    }

        //    var newPackage = new Package()
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = req.Name,
        //        Description = req.Description,
        //        Price = req.Price,
        //        EtagTypeId = req.ETagTypeId,
        //        StartDate = req.StartDate,
        //        EndDate = req.EndDate,
        //        MarketZoneId = req.MarketZoneId,
        //        CrDate = TimeUtils.GetCurrentSEATime(),
        //        UpsDate = TimeUtils.GetCurrentSEATime(),
        //    };

        //    await _unitOfWork.GetRepository<Package>().InsertAsync(newPackage);
        //    var result = await _unitOfWork.CommitAsync();

        //    return result > 0
        //        ? new ResponseAPI
        //        {
        //            MessageResponse = MessageConstant.PackageMessage.CreatePackageSuccessfully,
        //            StatusCode = MessageConstant.HttpStatusCodes.Created,
        //            Data = new { packageId = newPackage.Id }
        //        }
        //        : new ResponseAPI
        //        {
        //            MessageResponse = MessageConstant.PackageMessage.CreatePackageFail,
        //            StatusCode = MessageConstant.HttpStatusCodes.BadRequest
        //        };
        //}
        public async Task<ResponseAPI> CreatePackage(CreatePackageRequest req, Guid UserId)
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
            var marketZoneIsReal = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == req.MarketZoneId);
            if (marketZoneIsReal == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.MKZoneNotFound
                };
            }
            var ETagTypeIdIsReal = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.ETagTypeId);
            if (ETagTypeIdIsReal == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.NotFoundETagType
                };
            }
            var adminId = Guid.Parse(EnvironmentVariableConstant.AdminId);
            var isAdmin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == UserId && x.RoleId == adminId);
            if (isAdmin == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = MessageConstant.UserMessage.UnauthorizedAccess,
                    StatusCode = MessageConstant.HttpStatusCodes.Unauthorized
                };
            }
            var newPackage = new Package()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                EtagTypeId = req.ETagTypeId,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                MarketZoneId = req.MarketZoneId,
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

        public async Task<ResponseAPI> UpdatePackage(UpddatePackageRequest req ,Guid UserId)
        {
            var adminId = Guid.Parse(EnvironmentVariableConstant.AdminId);
            var isAdmin = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.RoleId == adminId );
            if (isAdmin == null)
            {
                return new ResponseAPI
                {
                    MessageResponse = MessageConstant.UserMessage.UnauthorizedAccess,
                    StatusCode = MessageConstant.HttpStatusCodes.Unauthorized
                };
            }
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == req.PackageId);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.NotFoundPackage
                };
            }
            var marketZoneIsReal = await _unitOfWork.GetRepository<MarketZone>().SingleOrDefaultAsync(predicate: x => x.Id == req.MarketZoneId);
            if (marketZoneIsReal == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.MKZoneNotFound
                };
            }
            var ETagTypeIdIsReal = await _unitOfWork.GetRepository<EtagType>().SingleOrDefaultAsync(predicate: x => x.Id == req.ETagTypeId);
            if (ETagTypeIdIsReal == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = MessageConstant.HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.NotFoundETagType
                };
            }
            package.Name = req.Name;
            package.Description = req.Description;
            package.Price = req.Price;
            package.StartDate = req.StartDate;

            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = "Package updated successfully",
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

        public async Task<GetListPackageResponse> GetListPackage(GetListParameterRequest req)
        {
            var packageRepo = _unitOfWork.GetRepository<Package>();
            var allPackages = await packageRepo.GetListAsync();
            var response = new GetListPackageResponse();
            try
            {
                IEnumerable<Package> filtereds = allPackages;
                if (req != null)
                {
                    if (!string.IsNullOrEmpty(req.Search))
                    {
                        filtereds = filtereds
                            .Where(x => x.Name.Contains(req.Search) || x.Description.Contains(req.Search));
                    }
                    if (req.Page.HasValue && req.PageSize.HasValue)
                    {
                        var skip = (req.Page.Value - 1) * req.PageSize.Value;
                        filtereds = filtereds.Skip(skip).Take(req.PageSize.Value);
                    }
                }

               

                if (!filtereds.Any())
                {
                    response.StatusCode = MessageConstant.HttpStatusCodes.NotFound;
                    response.MessageResponse = UserMessage.NotFoundUser;
                    return response;
                }
                response.StatusCode = MessageConstant.HttpStatusCodes.OK;
                response.MessageResponse = UserMessage.GetListSuccess;
                response.Packages = filtereds.ToList();

            }
            catch (Exception ex)
            {
                response.StatusCode = MessageConstant.HttpStatusCodes.InternalServerError;
                response.MessageResponse = $"An error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<GetPackageResponse> GetPackageDetail(Guid PackageId)
        {
            var isPackage = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == PackageId);
            if (isPackage == null)
            {
                return new GetPackageResponse()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = MessageConstant.PackageMessage.NotFoundPackage
                };
            }

            return new GetPackageResponse()
            {
                Id = isPackage.Id,
                Name = isPackage.Name,
                Description = isPackage.Description,
                Price = isPackage.Price,
                StartDate = isPackage.StartDate,
                EndDate = isPackage.EndDate,
                MarketZoneId = isPackage.MarketZoneId,
                CrDate = isPackage.CrDate,
                UpsDate = isPackage.UpsDate,
                EtagTypeId = isPackage.EtagTypeId,
                MarketZone = isPackage.MarketZone,
                PackageETagTypeMappings = isPackage.PackageETagTypeMappings.ToList(),
                StatusCode = HttpStatusCodes.Found,
                MessageResponse = MessageConstant.PackageMessage.FoundPackage,
            };
        }
    }
}

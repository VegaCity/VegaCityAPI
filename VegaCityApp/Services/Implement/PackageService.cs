﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Payload.Request.Package;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
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
                    MessageResponse = PackageMessage.ExistedPackageName, 
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            DateTime currentDate = DateTime.UtcNow;
            if (req.EndDate < currentDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.EndateInThePast,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate == req.StartDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.SameStrAndEndDate,
                    StatusCode = HttpStatusCodes.BadRequest
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
                        MessageResponse = PackageMessage.durationLimit,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.InvalidDuration,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            var newPackage = new Package()
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                ImageUrl = req.ImageUrl,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
            };
            await _unitOfWork.GetRepository<Package>().InsertAsync(newPackage);
            var response = new ResponseAPI()
            {
                MessageResponse = PackageMessage.CreatePackageSuccessfully,
                StatusCode = HttpStatusCodes.Created,
                Data = new
                {
                    PackageId = newPackage.Id
                }

            };
            int check = await _unitOfWork.CommitAsync();

            return check > 0 ? response : new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = PackageMessage.CreatePackageFail
            };
        }

        public async Task<ResponseAPI> UpdatePackage(Guid packageId, UpdatePackageRequest req)
        {
          
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(predicate: x => x.Id == packageId && !x.Deflag);
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageMessage.NotFoundPackage
                };
            }
            DateTime currentDate = TimeUtils.GetCurrentSEATime();
            if (req.EndDate < currentDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.EndateInThePast,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            if (req.EndDate == req.StartDate)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.SameStrAndEndDate,
                    StatusCode = HttpStatusCodes.BadRequest
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
                        MessageResponse = PackageMessage.durationLimit,
                        StatusCode = HttpStatusCodes.BadRequest
                    };
                }
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.InvalidDuration,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
            package.Name = req.Name;
            package.Description = req.Description;
            package.Price = req.Price;
            package.StartDate = req.StartDate;
            package.EndDate = req.EndDate;
            package.ImageUrl = req.ImageUrl;
            package.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            var result = await _unitOfWork.CommitAsync();
            if (result > 0)
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.UpdatePackageSuccessfully,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageId = package.Id
                    }
                };
            }
            else
            {
                return new ResponseAPI()
                {
                    MessageResponse = PackageMessage.UpdatePackageFailed,
                    StatusCode = HttpStatusCodes.BadRequest
                };
            }
        }

        public async Task<ResponseAPI<IEnumerable<GetPackageResponse>>> SearchAllPackage(int size, int page)
        {
            try
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
                    CrDate = x.CrDate,
                    UpsDate = x.UpsDate,
                    Deflag = x.Deflag,
                    ImageUrl = x.ImageUrl,
                },
                page: page,
                size: size,
                orderBy: x => x.OrderByDescending(z => z.Name),
                predicate: x => x.Deflag == false
            );
                return new ResponseAPI<IEnumerable<GetPackageResponse>>
                {
                    MessageResponse = PackageMessage.GetPackagesSuccessfully,
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
                return new ResponseAPI<IEnumerable<GetPackageResponse>>
                {
                    MessageResponse = PackageMessage.GetPackagesFail + ex.Message,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Data = null,
                    MetaData = null
                };
            }
        }
        public async Task<Package> SearchPackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync(
                predicate: x => x.Id == PackageId && !x.Deflag,
                include: user => user
                    .Include(y => y.PackageETagTypeMappings)
                    .ThenInclude(y => y.EtagType)
            );
            return package;
        }

        public async Task<ResponseAPI> DeletePackage(Guid PackageId)
        {
            var package = await _unitOfWork.GetRepository<Package>().SingleOrDefaultAsync
                (predicate: x => x.Id == PackageId,
                 include: map => map.Include(z => z.PackageETagTypeMappings));
            if (package == null)
            {
                return new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PackageMessage.NotFoundPackage
                };
            }
            //delete mapping
            if(package.PackageETagTypeMappings.Count > 0)
            {
                foreach (var item in package.PackageETagTypeMappings)
                {
                    _unitOfWork.GetRepository<PackageETagTypeMapping>().DeleteAsync(item);
                }
            }
            package.Deflag = true;
            _unitOfWork.GetRepository<Package>().UpdateAsync(package);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = PackageMessage.DeleteSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        PackageId = package.Id
                    }
                }
                : new ResponseAPI()
                {
                    MessageResponse = PackageMessage.DeleteFail,
                    StatusCode = HttpStatusCodes.BadRequest
                };
        }
    }
}

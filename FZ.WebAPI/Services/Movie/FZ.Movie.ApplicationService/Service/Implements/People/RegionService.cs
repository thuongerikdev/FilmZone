using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Search;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.People;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.People;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.People
{
    public class RegionService : MovieServiceBase, IRegionService
    {
        private readonly IRegionRepository _regionRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IMovieIndexService _movieIndexService;

        public RegionService(
            IRegionRepository regionRepository,
            IUnitOfWork unitOfWork,
            ILogger<RegionService> logger,
            IMovieIndexService movieIndexService) : base(logger)
        {
            _regionRepository = regionRepository;
            _unitOfWork = unitOfWork;
            _movieIndexService = movieIndexService;
        }

        public async Task<ResponseDto<Region>> CreateRegion(CreateRegionRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new region with name: {Name}", request.name);
            try
            {
                var existingRegion = await _regionRepository.GetByNameAsync(request.name, ct);
                if (existingRegion != null)
                {
                    _logger.LogWarning("Region with name: {Name} already exists", request.name);
                    return ResponseConst.Error<Region>(400, "Region with the same name already exists");
                }
                Region newRegion = new Region
                {
                    name = request.name,
                    code = request.code,
                    description = request.description,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _regionRepository.AddAsync(newRegion, cancellationToken);
                    return newRegion;
                }, ct: ct);

                _logger.LogInformation("Region created successfully with ID: {RegionID}", newRegion.regionID);
                return ResponseConst.Success("Region created successfully", newRegion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating region with name: {Name}", request.name);
                return ResponseConst.Error<Region>(500, "An error occurred while creating the region");
            }
        }

        public async Task<ResponseDto<Region>> UpdateRegion(UpdateRegionRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating region with ID: {RegionID}", request.regionID);
            try
            {
                var existingRegion = await _regionRepository.GetByIdAsync(request.regionID, ct);
                if (existingRegion == null)
                {
                    _logger.LogWarning("Region with ID: {RegionID} not found", request.regionID);
                    return ResponseConst.Error<Region>(404, "Region not found");
                }

                // Cập nhật thông tin
                existingRegion.name = request.name;
                existingRegion.code = request.code;
                existingRegion.updatedAt = DateTime.UtcNow;

                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _regionRepository.UpdateAsync(existingRegion, cancellationToken);
                    return existingRegion;
                }, ct: ct);

                // ✅ UPDATE SEARCH: Reindex lại toàn bộ phim thuộc Region này để cập nhật tên mới
                await _movieIndexService.ReindexByRegionAsync(existingRegion.regionID, ct);

                _logger.LogInformation("Region with ID: {RegionID} updated successfully", request.regionID);
                return ResponseConst.Success("Region updated successfully", existingRegion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating region with ID: {RegionID}", request.regionID);
                return ResponseConst.Error<Region>(500, "An error occurred while updating the region");
            }
        }

        public async Task<ResponseDto<bool>> DeleteRegion(int regionID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting region with ID: {RegionID}", regionID);
            try
            {
                var existingRegion = await _regionRepository.GetByIdAsync(regionID, ct);
                if (existingRegion == null)
                {
                    _logger.LogWarning("Region with ID: {RegionID} not found", regionID);
                    return ResponseConst.Error<bool>(404, "Region not found");
                }

                // ⚠️ QUAN TRỌNG: Lấy danh sách Movie ID bị ảnh hưởng TRƯỚC khi xóa Region
                // Vì sau khi xóa, quan hệ trong DB có thể bị mất (Set Null) hoặc xóa luôn phim.
                var affectedMovies = await _regionRepository.GetMoviesByRegionIDAsync(regionID, ct);
                var affectedMovieIds = affectedMovies?.Select(m => m.movieID).ToList() ?? new List<int>();

                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _regionRepository.RemoveAsync(existingRegion.regionID);
                    return true;
                }, ct: ct);

                // ✅ UPDATE SEARCH: Index lại các phim vừa bị mất Region
                if (affectedMovieIds.Any())
                {
                    await _movieIndexService.BulkIndexByIdsAsync(affectedMovieIds, ct);
                }

                _logger.LogInformation("Region with ID: {RegionID} deleted successfully", regionID);
                return ResponseConst.Success("Region deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting region with ID: {RegionID}", regionID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the region");
            }
        }

        public async Task<ResponseDto<Region>> GetRegionByID(int regionID, CancellationToken ct)
        {
            _logger.LogInformation("Fetching region with ID: {RegionID}", regionID);
            try
            {
                var region = await _regionRepository.GetByIdAsync(regionID, ct);
                if (region == null)
                {
                    _logger.LogWarning("Region with ID: {RegionID} not found", regionID);
                    return ResponseConst.Error<Region>(404, "Region not found");
                }
                _logger.LogInformation("Region with ID: {RegionID} fetched successfully", regionID);
                return ResponseConst.Success("Region fetched successfully", region);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching region with ID: {RegionID}", regionID);
                return ResponseConst.Error<Region>(500, "An error occurred while fetching the region");
            }
        }

        public async Task<ResponseDto<List<Region>>> GetAllRegions(CancellationToken ct)
        {
            _logger.LogInformation("Fetching all regions");
            try
            {
                var regions = await _regionRepository.GetALLRegionMoviesAsync(ct);
                _logger.LogInformation("Fetched {Count} regions", regions.Count);
                return ResponseConst.Success("Regions fetched successfully", regions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all regions");
                return ResponseConst.Error<List<Region>>(500, "An error occurred while fetching regions");
            }
        }

        public async Task<ResponseDto<List<Movies>>> GetMoviesByRegionIDAsync(int regionID, CancellationToken ct)
        {
            _logger.LogInformation("Fetching movies for region ID: {RegionID}", regionID);
            try
            {
                var regions = await _regionRepository.GetMoviesByRegionIDAsync(regionID, ct);
                if (regions == null || regions.Count == 0)
                {
                    _logger.LogWarning("No movies found for region ID: {RegionID}", regionID);
                    return ResponseConst.Error<List<Movies>>(404, "No movies found for the specified region");
                }
                _logger.LogInformation("Fetched {Count} movies for region ID: {RegionID}", regions.Count, regionID);
                return ResponseConst.Success("Movies fetched successfully", regions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching movies for region ID: {RegionID}", regionID);
                return ResponseConst.Error<List<Movies>>(500, "An error occurred while fetching movies for the region");
            }
        }

        public async Task<ResponseDto<List<Person>>> GetPeopleByRegionID(int regionID, CancellationToken ct)
        {
            _logger.LogInformation("Fetching people for region ID: {RegionID}", regionID);
            try
            {
                var people = await _regionRepository.GetPeopleByRegionID(regionID, ct);
                if (people == null || people.Count == 0)
                {
                    _logger.LogWarning("No people found for region ID: {RegionID}", regionID);
                    return ResponseConst.Error<List<Person>>(404, "No people found for the specified region");
                }
                _logger.LogInformation("Fetched {Count} people for region ID: {RegionID}", people.Count, regionID);
                return ResponseConst.Success("People fetched successfully", people);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching people for region ID: {RegionID}", regionID);
                return ResponseConst.Error<List<Person>>(500, "An error occurred while fetching people for the region");
            }
        }
    }
}
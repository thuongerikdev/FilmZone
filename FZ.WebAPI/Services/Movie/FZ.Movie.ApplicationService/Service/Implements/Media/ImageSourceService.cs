using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Media;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Media;
using FZ.Shared.ApplicationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media
{
    public class ImageSourceService : MovieServiceBase , IImageSourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageSourceRepository _imageSourceRepository;
        private readonly ICloudinaryService _cloudinaryService;
        public ImageSourceService(
            IUnitOfWork unitOfWork, 
            IImageSourceRepository imageSourceRepository, 
            ILogger<ImageSourceService> logger,
            ICloudinaryService cloudinaryService

            ) : base(logger)
        {
            _unitOfWork = unitOfWork;
            _imageSourceRepository = imageSourceRepository;
            _cloudinaryService = cloudinaryService;

        }
        public async Task<ResponseDto<ImageSource>> CreateImageSource(CreateImageSourceRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new image source for mediaID");
            try
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(request.source);


                ImageSource newImageSource = new ImageSource
                {
                    imageSourceName = request.imageSourceName,  
                    imageSourcetype = request.imageSourcetype,
                    source = uploadResult,
                    status = request.status,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _imageSourceRepository.AddImageSourceAsync(newImageSource, cancellationToken);
                    return newImageSource;
                }, ct: ct);
                _logger.LogInformation("Image source created successfully with ID: {ImageSourceID}", newImageSource.imageSourceID);
                return ResponseConst.Success("Image source created successfully", newImageSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating image source ");
                return ResponseConst.Error<ImageSource>(500, "An error occurred while creating the image source");
            }
        }
        public async Task<ResponseDto<ImageSource>> UpdateImageSource(
      UpdateImageSourceRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating image source {Id}", request.imageSourceID);

            try
            {
                // 1) Lấy entity có tracking (hoặc dùng GetTrackedAsync)
                var e = await _imageSourceRepository.GetByIdAsync(request.imageSourceID, ct);
                if (e is null)
                    return ResponseConst.Error<ImageSource>(404, "Image source not found");

                // 2) (Optional) validate input...
                e.imageSourceName = request.imageSourceName?.Trim();
                e.imageSourcetype = request.imageSourcetype;
                e.status = request.status;

                // Chuẩn bị thay ảnh (upload trước, delete sau)
                var oldSource = e.source;
                string? newSource = null;
                var needReplace = request.source is not null;

                if (needReplace)
                {
                    // Upload trước (ngoài DB tx)
                    newSource = await _cloudinaryService.UploadImageAsync(request.source!);
                }

                e.updatedAt = DateTime.UtcNow;

                // 3) Giao dịch DB
                await _unitOfWork.ExecuteInTransactionAsync(async txCt =>
                {
                    if (newSource is not null)
                        e.source = newSource;

                    await _imageSourceRepository.UpdateAsync(e, txCt);
                    // nếu UoW tự SaveChanges bên trong thì OK; nếu không, nhớ SaveChanges ở đây
                    return 0;
                });

                // 4) Sau khi COMMIT thành công mới xóa ảnh cũ (best-effort)
                if (needReplace && !string.IsNullOrEmpty(oldSource))
                {
                    try { await _cloudinaryService.DeleteImageAsync(oldSource!); }
                    catch (Exception delEx)
                    {
                        _logger.LogWarning(delEx, "Failed to delete old image {Url}", oldSource);
                        // Có thể queue vào Outbox để retry
                    }
                }

                _logger.LogInformation("Updated image source {Id}", e.imageSourceID);
                return ResponseConst.Success("Image source updated successfully", e);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict when updating image source {Id}", request.imageSourceID);
                return ResponseConst.Error<ImageSource>(409, "The image source was modified by another process. Please retry.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image source {Id}", request.imageSourceID);
                return ResponseConst.Error<ImageSource>(500, "An error occurred while updating the image source");
            }
        }
        public async Task<ResponseDto<bool>> DeleteImageSource(int imageSourceID, CancellationToken ct)
        {
                       _logger.LogInformation("Deleting image source with ID: {ImageSourceID}", imageSourceID);
            try
            {
                var existingImageSource = await _imageSourceRepository.GetByIdAsync(imageSourceID, ct);
                if (existingImageSource == null)
                {
                    _logger.LogWarning("Image source with ID: {ImageSourceID} not found", imageSourceID);
                    return ResponseConst.Error<bool>(404, "Image source not found");
                }
                var sourceToDelete = existingImageSource.source;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _imageSourceRepository.RemoveAsync(existingImageSource);
                    return true;
                }, ct: ct);
                // Xóa ảnh khỏi Cloudinary (best-effort)
                if (!string.IsNullOrEmpty(sourceToDelete))
                {
                    try
                    {
                        await _cloudinaryService.DeleteImageAsync(sourceToDelete);
                    }
                    catch (Exception delEx)
                    {
                        _logger.LogWarning(delEx, "Failed to delete image from Cloudinary: {Url}", sourceToDelete);
                        // Có thể queue vào Outbox để retry
                    }
                }
                _logger.LogInformation("Image source deleted successfully with ID: {ImageSourceID}", imageSourceID);
                return ResponseConst.Success("Image source deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting image source with ID: {ImageSourceID}", imageSourceID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the image source");
            }
        }
        public async Task<ResponseDto<ImageSource>> GetImageSourceByID(int imageSourceID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving image source with ID: {ImageSourceID}", imageSourceID);
            try
            {
                var imageSource = await _imageSourceRepository.GetByIdAsync(imageSourceID, ct);
                if (imageSource == null)
                {
                    _logger.LogWarning("Image source with ID: {ImageSourceID} not found", imageSourceID);
                    return ResponseConst.Error<ImageSource>(404, "Image source not found");
                }
                _logger.LogInformation("Image source with ID: {ImageSourceID} retrieved successfully", imageSourceID);
                return ResponseConst.Success("Image source retrieved successfully", imageSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving image source with ID: {ImageSourceID}", imageSourceID);
                return ResponseConst.Error<ImageSource>(500, "An error occurred while retrieving the image source");
            }
        }
        public async Task<ResponseDto<List<ImageSource>>> GetImageSourcesByTpe(string type, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving image sources with type: {Type}", type);
            try
            {
                var imageSources = await _imageSourceRepository.GetImageSourceByType(type, ct);
                if (imageSources == null || imageSources.Count == 0)
                {
                    _logger.LogWarning("No image sources found with type: {Type}", type);
                    return ResponseConst.Error<List<ImageSource>>(404, "No image sources found");
                }
                _logger.LogInformation("Image sources with type: {Type} retrieved successfully", type);
                return ResponseConst.Success("Image sources retrieved successfully", imageSources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving image sources with type: {Type}", type);
                return ResponseConst.Error<List<ImageSource>>(500, "An error occurred while retrieving the image sources");
            }
        }

    }
}

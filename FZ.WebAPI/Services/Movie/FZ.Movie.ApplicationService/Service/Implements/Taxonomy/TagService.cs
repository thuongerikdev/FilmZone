using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Search;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Taxonomy;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Taxonomy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Taxonomy
{
    public class TagService : MovieServiceBase, ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovieIndexService _movieIndexService;
        private readonly IMovieTagRepository _movieTagRepository;
        public TagService(
            ITagRepository tagRepository, 
            IUnitOfWork unitOfWork, 
            ILogger<TagService> logger , 
            IMovieTagRepository movieTagRepository,

            IMovieIndexService movieIndexService) : base(logger)
        {
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
            _movieIndexService = movieIndexService;
            _movieTagRepository = movieTagRepository;
        }

        public async Task<ResponseDto<Tag>> CreateTag(CreateTagRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new tag with name: {Name}", request.tagName);
            try
            {
                var existingTag = await _tagRepository.GetByTagName(request.tagName, ct);
                if (existingTag != null)
                {
                    _logger.LogWarning("Tag with name: {Name} already exists", request.tagName);
                    return ResponseConst.Error<Tag>(400, "Tag with the same name already exists");
                }
                Tag newTag = new Tag
                {
                   tagName = request.tagName,
                   tagDescription = request.tagDescription,
                    createAt = DateTime.UtcNow,
                    updateAt = DateTime.UtcNow,

                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _tagRepository.AddAsync(newTag, cancellationToken);
                    return newTag;
                }, ct: ct);
              
                _logger.LogInformation("Tag created successfully with ID: {TagID}", newTag.tagID);
                return ResponseConst.Success("Tag created successfully", newTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag with name: {Name}", request.tagName);
                return ResponseConst.Error<Tag>(500, "An error occurred while creating the tag");
            }
        }
        public async Task<ResponseDto<Tag>> UpdateTag(UpdateTagRequest request, CancellationToken ct)
        {
                       _logger.LogInformation("Updating tag with ID: {TagID}", request.tagID);
            try
            {
                var existingTag = await _tagRepository.GetByIdAsync(request.tagID, ct);
                if (existingTag == null)
                {
                    _logger.LogWarning("Tag with ID: {TagID} not found", request.tagID);
                    return ResponseConst.Error<Tag>(404, "Tag not found");
                }
                existingTag.tagName = request.tagName;
                existingTag.tagDescription = request.tagDescription;
                existingTag.updateAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _tagRepository.UpdateAsync(existingTag, cancellationToken);
                    return existingTag;
                }, ct: ct);

                if (existingTag.tagID > 0)
                {
                    await _movieIndexService.ReindexByTagAsync(existingTag.tagID, ct);
                }
                _logger.LogInformation("Tag updated successfully with ID: {TagID}", existingTag.tagID);
                return ResponseConst.Success("Tag updated successfully", existingTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating tag with ID: {TagID}", request.tagID);
                return ResponseConst.Error<Tag>(500, "An error occurred while updating the tag");
            }
        }
        public async Task<ResponseDto<bool>> DeleteTag(int tagID, CancellationToken ct)
        {
                       _logger.LogInformation("Deleting tag with ID: {TagID}", tagID);
            try
            {
                var existingTag = await _tagRepository.GetByIdAsync(tagID, ct);
                if (existingTag == null)
                {
                    _logger.LogWarning("Tag with ID: {TagID} not found", tagID);
                    return ResponseConst.Error<bool>(404, "Tag not found");
                }
                var associatedMovieTags = await _movieTagRepository.GetByTagID(tagID, ct);



                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _tagRepository.RemoveAsync(existingTag.tagID);
                    foreach (var movieTag in associatedMovieTags)
                    {
                        await _movieTagRepository.RemoveAsync(movieTag.movieTagID);
                    }
                    return true;
                }, ct: ct);
                if (existingTag.tagID > 0)
                {
                    await _movieIndexService.ReindexByTagAsync(existingTag.tagID, ct);
                }
                _logger.LogInformation("Tag deleted successfully with ID: {TagID}", tagID);
                return ResponseConst.Success("Tag deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tag with ID: {TagID}", tagID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the tag");
            }
        }
        public async Task<ResponseDto<Tag>> GetTagByID(int tagID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving tag with ID: {TagID}", tagID);
            try
            {
                var tag = await _tagRepository.GetByIdAsync(tagID, ct);
                if (tag == null)
                {
                    _logger.LogWarning("Tag with ID: {TagID} not found", tagID);
                    return ResponseConst.Error<Tag>(404, "Tag not found");
                }
                _logger.LogInformation("Tag with ID: {TagID} retrieved successfully", tagID);
                return ResponseConst.Success("Tag retrieved successfully", tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tag with ID: {TagID}", tagID);
                return ResponseConst.Error<Tag>(500, "An error occurred while retrieving the tag");
            }
        }
        public async Task<ResponseDto<List<Tag>>> GetAllTags(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all tags");
            try
            {
                var tags = await _tagRepository.GetAllTagAsync(ct);
                _logger.LogInformation("Successfully retrieved {Count} tags", tags.Count);
                return ResponseConst.Success("Tags retrieved successfully", tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all tags");
                return ResponseConst.Error<List<Tag>>(500, "An error occurred while retrieving the tags");
            }
        }
    }
}

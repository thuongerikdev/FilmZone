using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Interactions;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Interactions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Interactions
{
    public class CommentService :MovieServiceBase, ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        private readonly IUnitOfWork _unitOfWork;
        public CommentService(ICommentRepository commentRepository, IUnitOfWork unitOfWork , ILogger<CommentService> logger) : base(logger)
        {
            _commentRepository = commentRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<Comment>> CreateComment(CreateCommentRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new comment for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
            try
            {
                Comment newComment = new Comment
                {
                    content = request.content,
                    userID = request.userID,
                    movieID = request.movieID,
                    parentID = request.parentID,
                    isEdited = false,
                    likeCount = 0,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _commentRepository.AddCommentAsync(newComment, cancellationToken);
                    return newComment;
                }, ct: ct);
                _logger.LogInformation("Comment created successfully with ID: {CommentID}", newComment.commentID);
                return ResponseConst.Success("Comment created successfully", newComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating comment for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
                return ResponseConst.Error<Comment>(500, "An error occurred while creating the comment");
            }
        }
        public async Task<ResponseDto<Comment>> UpdateComment(UpdateCommentRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating comment with ID: {CommentID}", request.commentID);
            try
            {
                var existingComment = await _commentRepository.GetByIdAsync(request.commentID, ct);
                if (existingComment == null)
                {
                    _logger.LogWarning("Comment with ID: {CommentID} not found", request.commentID);
                    return ResponseConst.Error<Comment>(404, "Comment not found");
                }
                existingComment.content = request.content;
                existingComment.likeCount = request.likeCount;
                existingComment.parentID = request.parentID;
                existingComment.movieID = request.movieID;
                existingComment.isEdited = true;
                existingComment.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _commentRepository.UpdateAsync(existingComment, cancellationToken);
                    return existingComment;
                }, ct: ct);
                _logger.LogInformation("Comment with ID: {CommentID} updated successfully", request.commentID);
                return ResponseConst.Success("Comment updated successfully", existingComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment with ID: {CommentID}", request.commentID);
                return ResponseConst.Error<Comment>(500, "An error occurred while updating the comment");
            }
        }

        public async Task<ResponseDto<bool>> DeleteComment(int commentID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting comment with ID: {CommentID}", commentID);
            try
            {
                var exists = await _commentRepository.ExistsAsync(commentID, ct);
                if (!exists)
                {
                    _logger.LogWarning("Comment with ID: {CommentID} not found", commentID);
                    return ResponseConst.Error<bool>(404, "Comment not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _commentRepository.RemoveAsync(commentID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Comment with ID: {CommentID} deleted successfully", commentID);
                return ResponseConst.Success("Comment deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment with ID: {CommentID}", commentID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the comment");
            }
        }
        public async Task<ResponseDto<Comment>> GetCommentByID(int commentID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving comment with ID: {CommentID}", commentID);
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentID, ct);
                if (comment == null)
                {
                    _logger.LogWarning("Comment with ID: {CommentID} not found", commentID);
                    return ResponseConst.Error<Comment>(404, "Comment not found");
                }
                _logger.LogInformation("Comment with ID: {CommentID} retrieved successfully", commentID);
                return ResponseConst.Success("Comment retrieved successfully", comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comment with ID: {CommentID}", commentID);
                return ResponseConst.Error<Comment>(500, "An error occurred while retrieving the comment");
            }
        }
        public async Task<ResponseDto<List<Comment>>> GetCommentsByUserID(int userID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving comments for userID: {UserID}", userID);
            try
            {
                var comments = await _commentRepository.GetCommentsByUserIdAsync(userID, ct);
                _logger.LogInformation("Comments for userID: {UserID} retrieved successfully", userID);
                return ResponseConst.Success("Comments retrieved successfully", comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comments for userID: {UserID}", userID);
                return ResponseConst.Error<List<Comment>>(500, "An error occurred while retrieving the comments");
            }
        }
        public async Task<ResponseDto<List<Comment>>> GetCommentsByMovieID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving comments for movieID: {MovieID}", movieID);
            try
            {
                var comments = await _commentRepository.GetCommentsByMovieIdAsync(movieID, ct);
                _logger.LogInformation("Comments for movieID: {MovieID} retrieved successfully", movieID);
                return ResponseConst.Success("Comments retrieved successfully", comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving comments for movieID: {MovieID}", movieID);
                return ResponseConst.Error<List<Comment>>(500, "An error occurred while retrieving the comments");
            }
        }

    }
}

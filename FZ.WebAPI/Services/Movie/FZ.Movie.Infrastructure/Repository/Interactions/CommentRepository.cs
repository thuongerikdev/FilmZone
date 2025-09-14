using FZ.Movie.Domain.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Interactions
{
    public interface ICommentRepository
    {
        Task AddCommentAsync(Comment comment, CancellationToken ct);    
        Task<Comment?> GetByIdAsync(int commentID, CancellationToken ct);
        Task<Comment?> GetTrackedAsync(int commentID, CancellationToken ct);
        Task<bool> ExistsAsync(int commentID, CancellationToken ct);
        Task UpdateAsync(Comment comment, CancellationToken ct);
        Task<bool> PatchAsync(int commentID, Action<Comment> apply, CancellationToken ct);
        Task RemoveAsync(int commentID);
        Task<int> HardDeleteAsync(int commentID, CancellationToken ct);
        Task<List<Comment>> GetCommentsByMovieIdAsync(int movieID, CancellationToken ct);
        Task<List<Comment>> GetAllCommentAsync(CancellationToken ct);
        Task<List<Comment>> GetCommentsByUserIdAsync(int userID, CancellationToken ct);

    }
    public sealed class CommentRepository : ICommentRepository
    {
        private readonly MovieDbContext _context;
        public CommentRepository(MovieDbContext context) => _context = context;
        public Task AddCommentAsync(Comment comment, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(comment);
            return _context.Comments.AddAsync(comment, ct).AsTask();
        }
        public Task<Comment?> GetByIdAsync(int commentID, CancellationToken ct)
            => _context.Comments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.commentID == commentID, ct);
        public Task<Comment?> GetTrackedAsync(int commentID, CancellationToken ct)
            => _context.Comments.FirstOrDefaultAsync(x => x.commentID == commentID, ct);
        public Task<bool> ExistsAsync(int commentID, CancellationToken ct)
            => _context.Comments.AsNoTracking().AnyAsync(x => x.commentID == commentID, ct);
        public Task UpdateAsync(Comment comment, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(comment);
            _context.Comments.Update(comment);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int commentID, Action<Comment> apply, CancellationToken ct)
        {
            var comment = await GetTrackedAsync(commentID, ct);
            if (comment is null) return false;
            apply(comment);
            return true;
        }
        public  Task RemoveAsync(int commentID)
        {
            var stub = new Comment { commentID = commentID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;


        }
        public Task<int> HardDeleteAsync(int commentID, CancellationToken ct)
            => _context.Comments.Where(c => c.commentID == commentID)
                .ExecuteDeleteAsync(ct);

        public Task<List<Comment>> GetCommentsByMovieIdAsync(int movieID, CancellationToken ct)
            => _context.Comments.AsNoTracking()
                .Where(c => c.movieID == movieID)
                .ToListAsync(ct);
        public Task<List<Comment>> GetAllCommentAsync(CancellationToken ct)
            => _context.Comments.AsNoTracking()
                .ToListAsync(ct);
        public Task<List<Comment>> GetCommentsByUserIdAsync(int userID, CancellationToken ct)
            => _context.Comments.AsNoTracking()
                .Where(c => c.userID == userID)
                .ToListAsync(ct);
    }
}

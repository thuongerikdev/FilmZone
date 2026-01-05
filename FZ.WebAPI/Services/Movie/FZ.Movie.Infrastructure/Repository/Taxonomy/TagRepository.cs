using FZ.Movie.Domain.Taxonomy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Taxonomy
{
    public interface ITagRepository
    {
        Task AddAsync(Domain.Taxonomy.Tag tag, CancellationToken ct);
        Task<Domain.Taxonomy.Tag?> GetByIdAsync(int tagID, CancellationToken ct);
        Task<Domain.Taxonomy.Tag?> GetTrackedAsync(int tagID, CancellationToken ct);
        Task<bool> ExistsAsync(int tagID, CancellationToken ct);
        Task UpdateAsync(Domain.Taxonomy.Tag tag, CancellationToken ct);
        Task<bool> PatchAsync(int tagID, Action<Domain.Taxonomy.Tag> apply, CancellationToken ct);
        Task RemoveAsync(int tagID);
        Task<int> HardDeleteAsync(int tagID, CancellationToken ct);
        Task<List<Domain.Taxonomy.Tag>> GetAllTagAsync(CancellationToken ct);
        Task<Tag>  GetByTagName (string tagName, CancellationToken ct);
    }
    public sealed class TagRepository : ITagRepository
    {
        private readonly MovieDbContext _context;
        public TagRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.Taxonomy.Tag tag, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(tag);
            return _context.Tags.AddAsync(tag, ct).AsTask();
        }
        public Task<Domain.Taxonomy.Tag?> GetByIdAsync(int tagID, CancellationToken ct)
            => _context.Tags.AsNoTracking()
                .FirstOrDefaultAsync(x => x.tagID == tagID, ct);
        public Task<Domain.Taxonomy.Tag?> GetTrackedAsync(int tagID, CancellationToken ct)
            => _context.Tags.FirstOrDefaultAsync(x => x.tagID == tagID, ct);
        public Task<bool> ExistsAsync(int tagID, CancellationToken ct)
            => _context.Tags.AsNoTracking().AnyAsync(x => x.tagID == tagID, ct);
        public Task UpdateAsync(Domain.Taxonomy.Tag tag, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(tag);
            _context.Tags.Update(tag);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int tagID, Action<Domain.Taxonomy.Tag> apply, CancellationToken ct)
        {
            var tag = await GetTrackedAsync(tagID, ct);
            if (tag is null) return false;
            apply(tag);
            return true;
        }
        public Task RemoveAsync(int tagID)
        {
            var stub = new Domain.Taxonomy.Tag { tagID = tagID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int tagID, CancellationToken ct)
            => _context.Tags
                .Where(x => x.tagID == tagID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Taxonomy.Tag>> GetAllTagAsync(CancellationToken ct)
            => _context.Tags.AsNoTracking().ToListAsync(ct);
        public Task<Tag> GetByTagName(string tagName, CancellationToken ct)
            => _context.Tags.AsNoTracking()
                .FirstOrDefaultAsync(x => x.tagName == tagName, ct);
    }
}

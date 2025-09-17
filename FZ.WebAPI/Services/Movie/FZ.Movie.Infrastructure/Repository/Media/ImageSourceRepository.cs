using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Media;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Media
{
    public interface IImageSourceRepository
    {
        Task<ImageSource> AddImageSourceAsync(Domain.Media.ImageSource imageSource, CancellationToken ct);
        Task<ImageSource> UpdateAsync(Domain.Media.ImageSource imageSource, CancellationToken ct);
        Task<ImageSource> RemoveAsync(ImageSource imageSource);
        Task<List<Domain.Media.ImageSource>> GetAllImageSourcesAsync(CancellationToken ct);
        Task<List<ImageSource>> GetImageSourceByType(string type, CancellationToken ct);
        Task<ImageSource?> GetByIdAsync(int imageSourceID, CancellationToken ct);
    }
    public sealed class ImageSourceRepository : IImageSourceRepository
    {
        private readonly MovieDbContext _context;
        public ImageSourceRepository(MovieDbContext context) => _context = context;

        public async Task<ImageSource> AddImageSourceAsync(Domain.Media.ImageSource imageSource, CancellationToken ct)
        {
            var result = await _context.ImageSources.AddAsync(imageSource, ct);
            return result.Entity;
        }

        public async Task<ImageSource> UpdateAsync(Domain.Media.ImageSource imageSource, CancellationToken ct)
        {
            var existingOrder = await _context.ImageSources.FindAsync(new object[] { imageSource.imageSourceID }, ct);
            if (existingOrder == null)
            {
                throw new KeyNotFoundException($"Order with ID {imageSource.imageSourceID} not found.");
            }
            _context.Entry(existingOrder).CurrentValues.SetValues(imageSource);
            return existingOrder;
        }
        public Task<ImageSource>RemoveAsync(ImageSource imageSource)
        {
            _context.Attach(imageSource);
            var result = _context.ImageSources.Remove(imageSource);
            return Task.FromResult(result.Entity);

        }
        public Task<List<Domain.Media.ImageSource>> GetAllImageSourcesAsync(CancellationToken ct)
            => _context.ImageSources.AsNoTracking().ToListAsync(ct);
        public Task<List<ImageSource>> GetImageSourceByType(string type, CancellationToken ct)
            => _context.ImageSources.AsNoTracking()
                .Where(x => x.imageSourcetype == type)
                .ToListAsync(ct);

        public Task<ImageSource?> GetByIdAsync(int imageSourceID, CancellationToken ct)
            => _context.ImageSources
                .FirstOrDefaultAsync(x => x.imageSourceID == imageSourceID, ct);
    }
}

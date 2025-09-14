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
        Task AddImageSourceAsync(Domain.Media.ImageSource imageSource, CancellationToken ct);
        Task UpdateAsync(Domain.Media.ImageSource imageSource, CancellationToken ct);
        Task RemoveAsync(int imageSourceID);
        Task<List<Domain.Media.ImageSource>> GetAllImageSourcesAsync(CancellationToken ct);
        Task<List<ImageSource>> GetImageSourceByType(string type, CancellationToken ct);
        Task<ImageSource?> GetByIdAsync(int imageSourceID, CancellationToken ct);
    }
    public sealed class ImageSourceRepository : IImageSourceRepository
    {
        private readonly MovieDbContext _context;
        public ImageSourceRepository(MovieDbContext context) => _context = context;
        public Task AddImageSourceAsync(Domain.Media.ImageSource imageSource, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(imageSource);
            return _context.ImageSources.AddAsync(imageSource, ct).AsTask();
        }
        public Task UpdateAsync(Domain.Media.ImageSource imageSource, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(imageSource);
            _context.ImageSources.Update(imageSource);
            return Task.CompletedTask;
        }
        public Task RemoveAsync(int imageSourceID)
        {
            var stub = new Domain.Media.ImageSource { imageSourceID = imageSourceID };
            _context.Entry(stub).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            return Task.CompletedTask;
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

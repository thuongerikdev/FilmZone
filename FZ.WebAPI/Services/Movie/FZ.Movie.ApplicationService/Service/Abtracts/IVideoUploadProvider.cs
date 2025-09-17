using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Abtracts
{
    public record UploadContext(
        string JobId,
        string SourceType,
        string Scope,
        int TargetId,
        string Quality,
        string Language,
        bool IsVipOnly,
        bool IsActive,
        Stream? FileStream,
        long FileSize,
        string? LinkUrl,
        string? FileName,
        string? TempFilePath,        // 👈 NEW: để dọn file sau khi xong
        CancellationToken Ct
    );


    public interface IVideoUploadProvider
    {
        string SourceType { get; }      // "vimeo-file" | "vimeo-link"
        Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress);
    }

    public record ProviderResult(
        bool Success,
        string? VendorVideoId,  // "123456789" hoặc id kiểu khác
        string? VendorUri,      // "/videos/{id}"
        string? PlayerUrl,      // https://player.vimeo.com/video/{id}
        string? Error
    );

    public class ProviderResolver
    {
        private readonly IEnumerable<IVideoUploadProvider> _providers;
        public ProviderResolver(IEnumerable<IVideoUploadProvider> providers) => _providers = providers;

        public IVideoUploadProvider Resolve(string sourceType) =>
            _providers.First(p => p.SourceType.Equals(sourceType, StringComparison.OrdinalIgnoreCase));
    }

}

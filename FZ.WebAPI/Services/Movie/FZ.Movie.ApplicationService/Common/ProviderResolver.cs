using FZ.Movie.ApplicationService.Service.Abtracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Common
{
    public class ProviderResolver
    {
        private readonly IEnumerable<IVideoUploadProvider> _providers;
        public ProviderResolver(IEnumerable<IVideoUploadProvider> providers) => _providers = providers;
        public IVideoUploadProvider Resolve(string sourceType) =>
            _providers.First(p => p.SourceType.Equals(sourceType, StringComparison.OrdinalIgnoreCase));
    }
}

using FZ.Movie.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Common
{
    public class MovieServiceBase
    {
        public readonly ILogger _logger;
        public MovieServiceBase(ILogger logger )
        {
            _logger = logger;
        }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Shared.ApplicationService
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        public Task DeleteImageAsync(string imageUrl);
    }
}

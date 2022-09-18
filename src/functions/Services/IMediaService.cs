using Contoso.Infrastructure.Models;
using System;
using System.Threading.Tasks;

namespace Contoso
{
    public interface IMediaService
    {
        Task<VideoEncodingJob> SubmitJobAsync(Uri videoUrl, string videoName, string videoDescription, string fileName);
    }
}
using Contoso.Infrastructure.Models;
using System.Threading.Tasks;

namespace Contoso
{
    public interface IMediaService
    {
        Task<VideoEncodingJob> SubmitJobAsync(string videoUrl, string videoName, string videoDescription, string fileName);
    }
}
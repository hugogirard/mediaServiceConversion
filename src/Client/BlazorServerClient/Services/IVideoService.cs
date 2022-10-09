using BlazorServerClient.ViewModel;
using Contoso.Infrastructure.Models;

namespace BlazorServerClient.Services
{
    public interface IVideoService
    {
        Task<IEnumerable<VideoEncodingJob>> GetJobs();

        Task UploadVideoAsync(FileUploadViewModel vm, string fileName, Stream content);

        //Task<string> GetUploadSasUrl(string name);
    }
}
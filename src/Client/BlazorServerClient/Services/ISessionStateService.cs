using Contoso.Infrastructure.Models;

namespace BlazorServerClient.Services
{
    public interface ISessionStateService
    {
        VideoEncodingJob VideoEncodingJob { get; set; }
    }
}
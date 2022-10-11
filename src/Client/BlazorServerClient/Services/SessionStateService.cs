using Contoso.Infrastructure.Models;

namespace BlazorServerClient.Services;

public class SessionStateService : ISessionStateService
{
    public VideoEncodingJob VideoEncodingJob { get; set; }
}

using System.Threading.Tasks;

namespace Contoso
{
    public interface IMediaServiceFactory
    {
        Task<IAzureMediaServicesClient> GetMediaServiceClientAsync();
    }
}
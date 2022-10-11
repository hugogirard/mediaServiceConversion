using BlazorServerClient.ViewModel;
using Contoso.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorServerClient.Pages
{
    public class PlayVideoModel : PageModel        
    {
        private readonly IVideoService _videoService;

        public PlayVideoViewModel VM { get; private set; }

        public PlayVideoModel(IVideoService videoService)
        {
            _videoService = videoService;
        }

        public async Task OnGet(string id)
        {
            // Retrieve the VideoJob
            var video = await _videoService.GetJob(id);

            if (video == null) 
            {
                return;
            }

            var streamingEndpoint = video.StreamingLocationUrl.FirstOrDefault(s => !s.Contains("format"));

            VM = new PlayVideoViewModel
            {
                Name = video.FileMetadata.Name,
                Description = video.FileMetadata.Description,
                StreamingUrl = streamingEndpoint
            };
        }
    }
}

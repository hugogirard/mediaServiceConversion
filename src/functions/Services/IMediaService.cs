using Contoso.Infrastructure.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contoso
{
    public interface IMediaService
    {
        Task<VideoEncodingJob> SubmitJobAsync(Uri videoUrl, string videoName, string videoDescription, string fileName);

        Task<bool> AssetExistAsync(string assetName);

        Task<IList<string>> GetStreamingUrlAsync(string assetName);
    }
}
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;

namespace Contoso.Infrastructure.Models;

public class VideoEncodingJob
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("jobName")]
    public string JobName { get; set; }

    [JsonProperty("outputAssetName")]
    public string OutputAssetName { get; set; }

    [JsonProperty("startedTime")]
    public DateTime? StartedTime { get; set; }

    [JsonProperty("completedTime")]
    public DateTime? CompletedTime { get; set; }

    [JsonProperty("fileMetadata")]
    public VideoMetadata FileMetadata { get; set; }

    [JsonProperty("state")]
    public JobState State { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("streamingLocationUrl")]
    public string StreamingLocationUrl { get; set; }

    public VideoEncodingJob(string fileName, string videoName, string videoDescription)
    {
        FileMetadata = new VideoMetadata();
        FileMetadata.Name = videoName;
        FileMetadata.Description = videoDescription;
        FileMetadata.Filename = fileName;                
    }
}

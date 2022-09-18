namespace Contoso.Infrastructure.Models;

public class VideoEncodingJob
{
    public string Id { get; set; }

    public string JobName { get; set; }

    public string OutputAssetName { get; set; }

    public DateTime StartedTime { get; set; }

    public DateTime CompletedTime { get; set; }

    public VideoMetadata FileMetadata { get; set; }

    public VideoEncodingJob(string fileName, string videoName, string videoDescription)
    {
        FileMetadata = new VideoMetadata();
        FileMetadata.Name = videoName;
        FileMetadata.Description = videoDescription;
        FileMetadata.Filename = fileName;        
    }
}

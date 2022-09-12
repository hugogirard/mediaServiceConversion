namespace Contoso.Infrastructure.Models;

public class VideoEncodingJob
{
    public string Id { get; set; }

    public DateTime StartedTime { get; set; }

    public DateTime CompletedTime { get; set; }

    public VideoMetadata FileMetadata { get; set; }

    public VideoEncodingJob()
    {
        FileMetadata = new VideoMetadata();
    }
}

using Newtonsoft.Json;

namespace Contoso.Infrastructure.Models;

public class VideoMetadata
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    //public string Description { get; set; }
}
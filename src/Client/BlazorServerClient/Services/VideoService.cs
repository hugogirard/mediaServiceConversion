using Azure.Storage.Blobs;
using BlazorServerClient.ViewModel;
using Contoso.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text.Json;

namespace BlazorServerClient.Services;

public class VideoService : IVideoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string BASE_URL;
    private readonly string FUNCTION_CODE;
    private readonly BlobContainerClient _container;

    public VideoService(IHttpClientFactory httpClientfactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientfactory;
        BASE_URL = configuration["FunctionBaseUrl"];
        FUNCTION_CODE = configuration["FunctionKeyCode"];

        var blobServiceClient = new BlobServiceClient(configuration["StrCnxString"]);
        _container = blobServiceClient.GetBlobContainerClient("videos");
    }

    public async Task UploadVideoAsync(FileUploadViewModel vm, string fileName, Stream content) 
    {

        IDictionary<string, string> metadata = new Dictionary<string, string>();
        metadata.Add("name", vm.Name);
        metadata.Add("description", vm.Description);

        BlobClient blob = _container.GetBlobClient(fileName);
                
        await blob.UploadAsync(content, true);
        
        await blob.SetMetadataAsync(metadata);
        
    }

    public async Task<IEnumerable<VideoEncodingJob>> GetJobs()
    {
        string url = $"{BASE_URL}/GetJobs";

        HttpResponseMessage response = await SendRequestAsync(url, HttpMethod.Get);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<VideoEncodingJob>>(json);
        }

        return new List<VideoEncodingJob>();
    }

    public async Task<VideoEncodingJob?> GetJob(string id) 
    {
        string url = $"{BASE_URL}/GetJobById?id={id}";

        HttpResponseMessage response = await SendRequestAsync(url, HttpMethod.Get);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VideoEncodingJob>(json);
        }

        return null;

    }

    private async Task<HttpResponseMessage> SendRequestAsync(string url, HttpMethod method)
    {
        return await SendRequestAsync(url, method, null);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(string url, HttpMethod method, dynamic? payload)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("x-functions-key", FUNCTION_CODE);
        if (payload != null)
        {
            //request.Content = new StringContent(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //}));
        }

        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request);
    }
}

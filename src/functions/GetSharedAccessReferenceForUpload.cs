using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace functions
{
    public class GetSharedAccessReferenceForUpload
    {
        private readonly ILogger<GetSharedAccessReferenceForUpload> _logger;
        private readonly IStorageService _storageService;

        public GetSharedAccessReferenceForUpload(ILogger<GetSharedAccessReferenceForUpload> log, IStorageService storageService)
        {
            _logger = log;
            _storageService = storageService;
        }

        [FunctionName("GetSharedAccessReferenceForUpload")]
        [OpenApiOperation(operationId: "GetSharedAccessReferenceForUpload", tags: new[] { "UploadBlob" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "filename", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The name of the file to upload")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Uri), Description = "The SAS URI")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string filename = req.Query["filename"];

            if (string.IsNullOrEmpty(filename))
                return new BadRequestObjectResult("The filename is not present in the query string");

            Uri sas = _storageService.GetSharedAccessReferenceForUpload(filename);

            return new OkObjectResult(sas);
        }
    }
}


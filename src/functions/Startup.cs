using Azure.Storage.Blobs;
using functions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

[assembly: FunctionsStartup(typeof(Contoso.Startup))]

namespace Contoso
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new MediaServiceConfiguration(new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables() // parses the values from the optional .env file at the solution root
                            .Build());

            builder.Services.AddSingleton<IMediaServiceConfiguration>(config);
            builder.Services.AddScoped<IMediaServiceFactory, MediaServiceFactory>();
            builder.Services.AddScoped<IMediaService, MediaService>();
            builder.Services.AddSingleton<IStorageService, StorageService>();

        }

    }
}

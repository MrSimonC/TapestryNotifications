using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PuppeteerSharp;
using System.IO;
using System.Runtime.InteropServices;
using TapestryNotifications;

[assembly: FunctionsStartup(typeof(Startup))]

namespace TapestryNotifications
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            BrowserFetcherOptions? bfOptions = new BrowserFetcherOptions();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                bfOptions.Path = Path.GetTempPath();
            }
            BrowserFetcher? bf = new BrowserFetcher(bfOptions);
            bf.DownloadAsync(BrowserFetcher.DefaultRevision).Wait();
            AppInfo? info = new AppInfo
            {
                BrowserExecutablePath = bf.GetExecutablePath(BrowserFetcher.DefaultRevision)
            };
            builder.Services.AddSingleton(info);
        }
    }
}
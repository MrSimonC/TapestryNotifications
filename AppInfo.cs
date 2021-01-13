using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TapestryNotifications;

[assembly: FunctionsStartup(typeof(Startup))]

namespace TapestryNotifications
{
    public class AppInfo
    {
        public string BrowserExecutablePath { get; set; }
    }
}
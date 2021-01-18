using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading;
using TapestryNotifications.Models;

namespace TapestryNotifications.Functions
{
    public class Notify
    {
        private readonly AppInfo? AppInfo;

        public Notify(AppInfo appInfo)
        {
            AppInfo = appInfo;
        }

        [FunctionName("Notify")]
        public async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 5 7-18 * * *")] TimerInfo myTimer, // 7:05->18:05 hourly 
            ILogger log,
            [DurableClient] IDurableEntityClient durableClient)
        {
            void logInfo(string message) => log.LogInformation($"{nameof(Notify)}: {message}");
            logInfo("Function has started");
            Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = AppInfo?.BrowserExecutablePath ?? throw new NullReferenceException("Can't find browser path")
            });

            // make new browser
            logInfo("Make browser");
            Page page = await browser.NewPageAsync();
            await page.SetUserAgentAsync("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.50 Safari/537.36");

            // set up variables
            logInfo("set up variables");
            string url = Environment.GetEnvironmentVariable("TAPESTRY_URL") ?? throw new NullReferenceException("TAPESTRY_URL");
            string email = Environment.GetEnvironmentVariable("TAPESTRY_EMAIL") ?? throw new NullReferenceException("TAPESTRY_USERNAME");
            string password = Environment.GetEnvironmentVariable("TAPESTRY_PASSWORD") ?? throw new NullReferenceException("TAPESTRY_PASSWORD");

            // Login
            logInfo($"going to url {url}");
            await page.GoToAsync(url);
            await page.WaitForSelectorAsync("#email_generated_id");
            await page.TypeAsync("#email_generated_id", email);
            await page.TypeAsync("#password_generated_id", password);
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForSelectorAsync("#userDropDownContainer");

            // Get all observation details
            logInfo("Get all observation details");
            string? html = await page.GetContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode? observationsCollection = doc.GetElementbyId("oljPages");
            var observations = observationsCollection.Descendants()
                .Where(oc => oc.Attributes["class"]?.Value.Contains("observation-item") ?? false)
                .Select(oi => oi.Descendants()
                    .Where(oi => oi.Attributes["class"]?.Value.Contains("media-heading") ?? false)
                    .Select(mh => new ExpandedObservation
                    {
                        Title = mh.Element("a")?.InnerText.Trim('\n').Trim() ?? "",
                        Id = oi.Attributes["data-obs-id"].Value,
                        Url = mh.Element("a")?.Attributes["href"].Value ?? ""
                    }).FirstOrDefault()
                )
                .Where(x => x != null)
                .ToList();
            // get more detail in each one
            foreach (ExpandedObservation? ob in observations)
            {
                await page.GoToAsync(ob.Url);
                html = await page.GetContentAsync();
                doc.LoadHtml(html);
                ob.Description = Helpers.CleanText(doc.DocumentNode.Descendants().Where(x => x.HasClass("page-note")).FirstOrDefault()?.InnerText) ?? "";
                ob.LatestUpdate = Helpers.CleanText(doc.GetElementbyId("oljComments")?.InnerText) ?? "";
                Thread.Sleep(1000); // time given to tapestry website
            }

            logInfo("call SaveCompareNotify()");
            var process = new SaveCompareNotify(log);
            await process.SaveAndNotify(durableClient, "tapestry", observations);

            // log out
            logInfo("Log out");
            await page.GoToAsync("https://tapestryjournal.com/logout");
        }
    }
}

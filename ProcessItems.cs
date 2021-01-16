using TapestryNotifications.Entities;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Prowl;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TapestryNotifications.Functions;

namespace ItemStorage
{
    public class ProcessItems
    {
        private readonly ProwlMessage prowl = new ProwlMessage();
        private readonly ILogger log;

        public ProcessItems(ILogger iLogger) => log = iLogger;

        public async Task SaveAndNotify(IDurableEntityClient client,
                                        string websiteName,
                                        List<ExpandedObservation> itemsToProcess)
        {
            foreach (ExpandedObservation item in itemsToProcess)
            {
                log.LogInformation($"Looking at entity: {websiteName}{item.Id}");
                var entityId = new EntityId(nameof(ExpandedObservation), $"{websiteName}{item.Id}");
                EntityStateResponse<ExpandedObservation> stateResponse = await client.ReadEntityStateAsync<ExpandedObservation>(entityId);

                if (!stateResponse.EntityExists)
                {
                    log.LogInformation($"{websiteName}{item.Id}: doesn't exist. Saving all details.");
                    await SetAllEntityProperties(client, item, entityId);
                    await SendProwl(client, websiteName, item, entityId);
                }
                else
                {
                    log.LogInformation($"{websiteName}{item.Id}: exists.");
                    ExpandedObservation old = stateResponse.EntityState;
                    await ProcessChanges(client, websiteName, item, entityId, old);
                }
            }
        }

        private async Task SetAllEntityProperties(IDurableEntityClient client, ExpandedObservation item, EntityId entityId)
        {
            await client.SignalEntityAsync<Observation>(entityId, e => e.SetTitle(item.Title));
            await client.SignalEntityAsync<Observation>(entityId, e => e.SetDescription(item.Description));
            await client.SignalEntityAsync<Observation>(entityId, e => e.SetLatestUpdate(item.LatestUpdate));
        }

        private async Task ProcessChanges(IDurableEntityClient client, string websiteName, ExpandedObservation item, EntityId entityId, ExpandedObservation old)
        {
            var prowlMessages = new List<string>();
            if (old.LatestUpdate != item.LatestUpdate)
            {
                prowlMessages.Add($"Update made to {item.Title}: latest comment: {item.LatestUpdate}");
            }

            if (prowlMessages.Any())
            {
                HttpResponseMessage? prowlResponse = await prowl.SendAsync(websiteName, $"{string.Join(", ", prowlMessages)}");
                if (prowlResponse?.IsSuccessStatusCode ?? false)
                {
                    await SetAllEntityProperties(client, item, entityId);
                }
            }
        }

        private async Task SendProwl(IDurableEntityClient client, string websiteName, ExpandedObservation item, EntityId entityId)
        {
            log.LogInformation($"{websiteName}{item.Id}: sending Prowl");
            HttpResponseMessage? prowlResponse = await prowl.SendAsync(websiteName, $"New entry: {item.Title}: \n {item.Description}");
        }
    }
}

using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Prowl;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TapestryNotifications.Entities;
using TapestryNotifications.Models;

namespace TapestryNotifications
{
    public class SaveCompareNotify
    {
        private readonly ProwlMessage prowl = new ProwlMessage();
        private readonly ILogger log;

        public SaveCompareNotify(ILogger iLogger)
        {
            log = iLogger;
        }

        public async Task SaveAndNotify(IDurableEntityClient client,
                                        string websiteName,
                                        List<ExpandedObservation> itemsToProcess)
        {
            foreach (ExpandedObservation item in itemsToProcess)
            {
                log.LogInformation($"Looking at entity: {websiteName}{item.Id}");
                var entityId = new EntityId(nameof(Observation), $"{websiteName}{item.Id}");
                EntityStateResponse<Observation> stateResponse = await client.ReadEntityStateAsync<Observation>(entityId);

                if (!stateResponse.EntityExists)
                {
                    log.LogInformation($"{websiteName}{item.Id}: doesn't exist. Saving all details.");
                    await SetAllEntityProperties(client, item, entityId);
                    await SendProwl(websiteName, item);
                }
                else
                {
                    log.LogInformation($"{websiteName}{item.Id}: exists.");
                    Observation old = stateResponse.EntityState;
                    await CheckForEntityChangesThenProwl(client, websiteName, item, entityId, old);
                }
            }
        }

        private async Task SetAllEntityProperties(IDurableEntityClient client, Observation item, EntityId entityId)
        {
            await client.SignalEntityAsync<IObservation>(entityId, e => e.SetTitle(item.Title));
            await client.SignalEntityAsync<IObservation>(entityId, e => e.SetDescription(item.Description));
            await client.SignalEntityAsync<IObservation>(entityId, e => e.SetLatestUpdate(item.LatestUpdate));
        }

        private async Task CheckForEntityChangesThenProwl(
            IDurableEntityClient client,
            string websiteName,
            ExpandedObservation item,
            EntityId entityId,
            Observation old)
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

        private async Task SendProwl(string websiteName, ExpandedObservation item)
        {
            log.LogInformation($"{websiteName}{item.Id}: sending Prowl");
            HttpResponseMessage? _ = await prowl.SendAsync(websiteName, $"New entry: {item.Title}:\n{item.Description}");
        }
    }
}

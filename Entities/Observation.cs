﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TapestryNotifications.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Observation : IObservation
    {
        [JsonProperty("title")]
        public string Title { get; set; } = "";
        public void SetTitle(string title) => Title = title;

        [JsonProperty("description")]
        public string Description { get; set; } = "";
        public void SetDescription(string description) => Description = description;

        [JsonProperty("latestupdate")]
        public bool LatestUpdate { get; set; } = true;
        public void SetLatestUpdate(bool latestUpdate) => LatestUpdate = latestUpdate;

        [FunctionName(nameof(Observation))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<Observation>();
    }
}

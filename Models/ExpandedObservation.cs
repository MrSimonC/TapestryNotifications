using TapestryNotifications.Entities;

namespace TapestryNotifications.Models
{
    public class ExpandedObservation : Observation
    {
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
    }
}

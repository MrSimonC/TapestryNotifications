namespace TapestryNotifications.Entities
{
    public interface IObservation
    {
        void SetDescription(string description);
        void SetLatestUpdate(bool latestUpdate);
        void SetTitle(string title);
    }
}
namespace TapestryNotifications.Entities
{
    public interface IObservation
    {
        void SetDescription(string description);
        void SetLatestUpdate(string latestUpdate);
        void SetTitle(string title);
    }
}
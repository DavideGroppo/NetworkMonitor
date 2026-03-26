namespace NetworkMonitor
{
    public class TrayIconSettings
    {

        private NotificationSettings notificationType;

        public TrayIconSettings()
        {
            notificationType = NotificationSettings.OnlyBad;
        }

        public bool IsEventToNotify(PingMetrics.NetworkQuality oldStatus, PingMetrics.NetworkQuality newStatus)
        {

            if (oldStatus == newStatus) { return false; }
            if (notificationType == NotificationSettings.Always) { return true; }

            if (notificationType == NotificationSettings.OnlyBad && newStatus == PingMetrics.NetworkQuality.Bad)
            {
                return true;
            }

            return false;
        }
        
        public void UpdateNotificationSettings(NotificationSettings newValue)
        {
            notificationType = newValue;
        }

        public enum NotificationSettings
        {
            Always,
            OnlyBad,
            Never
        }


    }
}
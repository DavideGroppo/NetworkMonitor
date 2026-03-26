using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace NetworkMonitor
{
    public class NetworkMonitorProcess
    {

        public NetworkMonitorProcess()
        {
            metrics = new(historyLength);
            history = new(historyLength);
            trayManager = new(ref history);
        }
        private TrayIconManager trayManager;
        private PingMetrics metrics;
        private PingHistory history;
        private const string server = "8.8.8.8";
        private const int historyLength = 30;

        private const int pingDelay = 1000;

        public async Task<bool> runProcess()
        {
            while (true)
            {
                
                PingReply? reply = await DoPingRequest();
                history.AddToHistory(reply);
                UpdateMetrics();
                trayManager.UpdateStatus(metrics.getNetworkQuality());

                await Task.Delay(pingDelay);
            }
        }

        private void UpdateMetrics()
        {
            var pingHystory = history.getPingHistory();
            metrics.totalSuccess = pingHystory.Count(r => r.Status == IPStatus.Success);
            IEnumerable<long> replyTimes = pingHystory.Select(r => r.RoundtripTime);
            metrics.averageResponse = replyTimes.Average();
        }
        private async Task<PingReply?> DoPingRequest()
        {

            string data = "0123456789012345"; // 16 byte
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            PingOptions options = new PingOptions(64, true);
            int timeout = 1000;

            using (Ping pingSender = new Ping())
            {
                try
                {
                    PingReply reply = await pingSender.SendPingAsync(IPAddress.Parse(server),
                                                                     timeout,
                                                                     buffer,
                                                                     options);

                    return reply;
                }
                catch (PingException ex)
                {
                    Console.WriteLine($" -- Impossibile raggiungere l'host --");
                    Console.WriteLine($"Dettaglio: {ex.InnerException?.Message ?? ex.Message}");
                    return null;
                }

            }
        }
    }
}

using System.Net.NetworkInformation;
using System.Numerics;

namespace NetworkMonitor
{
    public class PingHistory
    {
        public PingHistory()
        {
            historyLength = 20;
            pingHystory = new();
        }
        public PingHistory(int historyLength){
            this.historyLength = historyLength;
            pingHystory = new();
        }
        private Queue<PingReply> pingHystory;
        private int historyLength;

        public void AddToHistory(PingReply? reply)
        {
            if (reply is null) { return; }
            
            if (pingHystory.Count >= historyLength)
            {
                pingHystory.Dequeue();
            }
            pingHystory.Enqueue(reply);
        }

        public Queue<PingReply> getPingHistory()
        {
            return pingHystory;
        }

        public long[] getRttHistory()
        {
            return pingHystory.Select(r => r.RoundtripTime).ToArray();
        }

        public override string ToString()
        {
            IEnumerable<String> history = Enumerable.Empty<String>();
            foreach (PingReply ping in pingHystory){
                history = history.Append($"{ping.RoundtripTime} - {ping.Status}");
            }
            return String.Join('\n', history.ToArray<String>());
        }

    }
}
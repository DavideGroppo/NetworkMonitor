namespace NetworkMonitor
{
    public class PingMetrics
    {

        public PingMetrics()
        {
            averageResponse = 0;
            totalSuccess = 20;
            this.historyLength = 20;
        }

        public PingMetrics(int historyLength)
        {
            averageResponse = 0;
            totalSuccess = historyLength;
            this.historyLength = historyLength;
        }

        public double averageResponse;
        public int totalSuccess;

        public int historyLength;
        public double successPercentage => (double)totalSuccess / historyLength;
        public enum NetworkQuality
        {
            Bad,
            NotSoGood,
            AveragelyGood,
            Good,
            Optimal
        }

        public NetworkQuality getNetworkQuality()
        {
            if (successPercentage >= 0.9 && averageResponse <= 18)
            {
                return NetworkQuality.Optimal;
            }
            if ((successPercentage >= 0.9 && averageResponse <= 30) ||
                 (successPercentage >= 0.8 && averageResponse <= 25))
            {
                return NetworkQuality.Good;
            }
            if ((successPercentage >= 0.9) ||
                 (successPercentage >= 0.7 && averageResponse <= 40))
            {
                return NetworkQuality.AveragelyGood;
            }
            if (successPercentage >= 0.5)
            {
                return NetworkQuality.NotSoGood;
            }
            return NetworkQuality.Bad;
        }

    }
}
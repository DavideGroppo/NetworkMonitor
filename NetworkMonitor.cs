using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetworkMonitor
{
    class NetworkMonitor
    {
        // Rendiamo il Main asincrono per usare le funzioni moderne
        static async Task Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== NETWORK MONITOR 1.0 ===");
            Console.ResetColor();

            NetworkMonitorProcess nmp = new();
            Task.Run(async () => nmp.runProcess());
            Application.Run();
        }
    }
}
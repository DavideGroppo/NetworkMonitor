
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace NetworkMonitor
{
    public class TrayIconManager : IDisposable
    {

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

        const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        const int DWMWCP_ROUND = 2;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private Icon[] icons;

        private PingHistory history;

        private PingMetrics metrics;

        private TrayIconSettings settings;

        private PingMetrics.NetworkQuality lastStatus;

        private MonitorWindow? window;

        public TrayIconManager(ref PingHistory history, ref PingMetrics metrics)
        {

            this.history = history;
            this.metrics = metrics;
            lastStatus = PingMetrics.NetworkQuality.Bad;
            icons = LoadIcons();

            //menu contestuale con il tasto destro
            contextMenu = CreateContextMenu();

            //creo thread per window UI
            CreateUiThread();

            //configura l'icona
            notifyIcon = CreateNotifyIcon();

            //configuro le impostazioni della tray icon
            settings = new TrayIconSettings();

        }
        
        private async void CreateUiThread()
        {
            Thread uiThread = new Thread((ThreadStart)(() =>
            {
                this.window = new MonitorWindow();
                Dispatcher.Run();
            }));
            
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.IsBackground = true;
            uiThread.Start();
        }

        private NotifyIcon CreateNotifyIcon()
        {
            NotifyIcon nIcon = new NotifyIcon
            {
                Icon = icons[0],
                Text = lastStatus.ToString(),
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            // 3. Gestisci il doppio click (opzionale)
            nIcon.DoubleClick += ((s, e) =>
            {
                window?.Dispatcher.Invoke(() =>
                {

                    window.TxtTitle.Text = metrics.getNetworkQuality().ToString();
                    if (!window.IsVisible)
                        window.Show();
                    window?.Activate();

                });
            });

            return nIcon;
        }

        private Icon[] LoadIcons()
        {
            try
            {
                return [
                    new Icon(System.IO.Path.GetFullPath(@"img\wifi_icons_green.ico")),
                    new Icon(System.IO.Path.GetFullPath(@"img\wifi_icons_blue.ico")),
                    new Icon(System.IO.Path.GetFullPath(@"img\wifi_icons_red.ico"))
                ];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore icone: {ex.Message}. Uso icone di sistema.");
                return [
                    SystemIcons.Information, // Green (Good)
                    SystemIcons.Warning,     // Blue (Medium/Fair)
                    SystemIcons.Error        // Red (Bad)
                ];
            }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            ContextMenuStrip ctxMenu = new ContextMenuStrip
            {
                Renderer = new MenuRenderer(),
                ForeColor = Color.White,
                ShowCheckMargin = false,
                ShowImageMargin = false,
                Font = new Font("Segoe UI Variable Display", 9.5f)
            };

            // Forza l'arrotondamento quando il menu si apre
            ctxMenu.Opening += (s, e) =>
            {
                var handle = ctxMenu.Handle;
                int attribute = DWMWCP_ROUND;
                DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref attribute, sizeof(int));
            };


            ctxMenu.Items.Add("Monitoring settings", null, OnStatusClick);
            ctxMenu.Items.Add(getNotificationSettingsMenu());
            
            ctxMenu.Items.Add(new ToolStripSeparator());    //line between menu options

            ctxMenu.Items.Add("End monitoring", null, EndMonitoring);

            //ctxMenu.Closing += OnClosingMenu;

            return ctxMenu;
        }

        private ToolStripMenuItem getNotificationSettingsMenu()
        {
            ToolStripMenuItem notifySettings = new("Notification settings");

            ToolStripMenuItem notificationAlways = new("Always", null, OnNotificationSettingsChanged);
            ToolStripMenuItem notificationOnlyBad = new("Only for Bad status", null, OnNotificationSettingsChanged);
            ToolStripMenuItem notificationNever = new("Never", null, OnNotificationSettingsChanged);

            setNotificationItemProperty(ref notificationAlways, TrayIconSettings.NotificationSettings.Always);
            setNotificationItemProperty(ref notificationOnlyBad, TrayIconSettings.NotificationSettings.OnlyBad);
            setNotificationItemProperty(ref notificationNever, TrayIconSettings.NotificationSettings.Never);

            notifySettings.DropDownItems.AddRange(new ToolStripItem[] { notificationAlways, notificationOnlyBad, notificationNever });

            notifySettings.DropDown.Renderer = new MenuRenderer();
            notifySettings.DropDown.ForeColor = Color.White;
            notifySettings.DropDown.Font = new Font("Segoe UI Variable Display", 9.5f);

            notifySettings.DropDownOpening += (s, e) =>
            {
                var handle = notifySettings.DropDown.Handle;
                int attribute = DWMWCP_ROUND;
                DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref attribute, sizeof(int));
            };
            notifySettings.DropDown.Closing += OnClosingMenu;

            return notifySettings;
        }

        private void setNotificationItemProperty(ref ToolStripMenuItem menuItem, TrayIconSettings.NotificationSettings type)
        {
            menuItem.CheckOnClick = true;
            menuItem.Tag = type;

            if (type == TrayIconSettings.NotificationSettings.OnlyBad)
            {
                menuItem.Checked = true;
            }

        }
        
        private void OnClosingMenu(object? sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                //impedisci la chiusura automatica
                e.Cancel = true;
            }
        }

        private void OnNotificationSettingsChanged(object? sender, EventArgs e)
        {
            if (sender is null) { return; }

            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            var parent = menuItem;
            if (parent?.OwnerItem is null){ return; }

            foreach (ToolStripMenuItem item in ((ToolStripDropDownItem)parent.OwnerItem).DropDownItems)
            {
                item.Checked = false;
            }
            menuItem.Checked = true;

            var tag = menuItem?.Tag;
            
            if (tag is null) { return; }
            TrayIconSettings.NotificationSettings notificationType = (TrayIconSettings.NotificationSettings)tag;

            settings.UpdateNotificationSettings(notificationType);
        }

        // Metodo per aggiornare lo stato dinamicamente
        public void UpdateStatus(PingMetrics.NetworkQuality networkStatus)
        {

            notifyIcon.Text = networkStatus.ToString();

            bool notifyEvent = settings.IsEventToNotify(lastStatus, networkStatus);

            if (lastStatus != networkStatus)
            {
                notifyIcon.Icon = GetTrayIcon(networkStatus);
            }

            if (notifyEvent)
            {
                notifyIcon.ShowBalloonTip(1000, "Network monitor", $"Network status changed: {lastStatus} => {networkStatus}", GetToolTipIcon(networkStatus));
            }

            lastStatus = networkStatus;
        }

        private ToolTipIcon GetToolTipIcon(PingMetrics.NetworkQuality status)
        {
            switch (status)
            {
                case PingMetrics.NetworkQuality.Optimal:
                case PingMetrics.NetworkQuality.Good:
                    return ToolTipIcon.Info;
                case PingMetrics.NetworkQuality.NotSoGood:
                case PingMetrics.NetworkQuality.AveragelyGood:
                    return ToolTipIcon.Warning;
                case PingMetrics.NetworkQuality.Bad:
                    return ToolTipIcon.Error;
            }
            return ToolTipIcon.None;
        }

        private void OnStatusClick(object? sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(3000, "cliccato", $"cliccato", ToolTipIcon.Info);
        }

        public void Dispose()
        {
            // Importante: Rimuove l'icona dalla tray quando l'app chiude
            notifyIcon.Dispose();
            contextMenu.Dispose();
        }
        public void EndMonitoring(object? a, EventArgs s)
        {
            Dispose();
            Application.Exit();
        }

        private Icon GetTrayIcon(PingMetrics.NetworkQuality quality)
        {
            int trayIcon = 0;

            switch (quality)
            {
                case PingMetrics.NetworkQuality.AveragelyGood:
                case PingMetrics.NetworkQuality.NotSoGood:
                    trayIcon = 1;
                    break;
                case PingMetrics.NetworkQuality.Bad:
                    trayIcon = 2;
                    break;
            }

            return icons[trayIcon];
        }

    }
}

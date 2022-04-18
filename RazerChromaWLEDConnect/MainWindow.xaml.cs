// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ChromaBroadcast;
using System.Windows.Controls;
using System.Management;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace ChromaBroadcastSampleApplication
{
    public partial class MainWindow : Window
    {
        static readonly string AppName = "RazerChromaWLEDConnect";

        protected UdpClient _udpClient;
        protected IPEndPoint _ep;

        protected string _wledIP;

        protected TextBox _log;
        protected Label _labelWebsocketState;
        protected Label _labelRazerState;
        protected int _totalLeds;
        protected bool _ledsOn = false;

        protected AppSettings _appSettings;

        private ManagementEventWatcher managementEventWatcher;
        private readonly Dictionary<string, string> powerValues = new Dictionary<string, string>
        {
            {"4", "Entering Suspend"},
            {"7", "Resume from Suspend"},
            {"10", "Power Status Change"},
            {"11", "OEM Event"},
            {"18", "Resume Automatic"}
        };

        protected RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        protected bool _runAtBoot = false;

        public MainWindow()
        {
            InitializeComponent();

            // Get Settings
            _appSettings = AppSettings.Load();
            _labelWebsocketState = WebsocketState;
            _labelRazerState = RazerState;

            if (rkApp.GetValue(AppName) == null)
            {
                _runAtBoot = false;
                ContextMenuItemRunAtBoot.IsChecked = false;
            }
            else
            {
                _runAtBoot = true;
                ContextMenuItemRunAtBoot.IsChecked = true;
            }

            Init();
        }

        public void Init()
        {
            // Here we check if there are settings
            if (_appSettings.RazerAppId != null && _appSettings.WledIPAddress != null)
            {
                // Start
                if (_runAtBoot) Hide();

                RzChromaBroadcastAPI.UnRegisterEventNotification();
                RzChromaBroadcastAPI.UnInit();
                RzResult lResult = RzChromaBroadcastAPI.Init(Guid.Parse(_appSettings.RazerAppId));

                if (lResult == RzResult.Success)
                {
                    _totalLeds = 15;
                    TotalLeds.Content = _totalLeds.ToString();

                    _wledIP = _appSettings.WledIPAddress;

                    UpdateLabelRazerState("Initialization Success");

                    BroadcastEnabled.IsChecked = true;
                    ContextMenuItemSync.IsChecked = true;

                    _udpClient = new UdpClient();

                    SetupEvents();
                    Start();

                }
                else
                {
                    UpdateLabelRazerState("Invalid Razer App Id");
                }
            }
            else
            {
                if (_appSettings.RazerAppId == null)
                {
                    UpdateLabelRazerState("No Razer App Id");
                }

                if (_appSettings.WledIPAddress == null)
                {
                    UpdateLabelWebsocketState("No WLED IP");
                }
            }
        }

        void SetupEvents()
        {
            // Hook into Razer 
            RzChromaBroadcastAPI.RegisterEventNotification(OnChromaBroadcastEvent);

            _ep = new IPEndPoint(IPAddress.Parse(_appSettings.WledIPAddress), _appSettings.WledUDPPort);
            _udpClient.Connect(_ep);

            InitPowerEvents();
        }

        private void Start()
        {
            var WLEDStateObject = new
            {
                leds = new { count = 0, pwr = 0, fps = 0 }
            };

            string webUrl = $"http://{_wledIP}/json/info";
            var httpRequest = (HttpWebRequest)WebRequest.Create(webUrl);
            httpRequest.Accept = "application/json";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            var httpResult = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                httpResult = streamReader.ReadToEnd();
            }

            var state = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(httpResult, WLEDStateObject);

            if (state != null)
            {
                if (_totalLeds != state.leds.count)
                {
                    _totalLeds = state.leds.count;
                    TotalLeds.Content = _totalLeds.ToString();
                }

                UpdateLabelWebsocketState("Connected");
            }

            TurnOn();
        }

        private void Stop()
        {
            TurnOff();
            UpdateLabelWebsocketState("Disconnected");
        }

        private void UpdateLabelWebsocketState(string Text)
        {
            Dispatcher.Invoke(() =>
            {
                _labelWebsocketState.Content = Text;
            });
        }

        private void UpdateLabelRazerState(string Text)
        {
            Dispatcher.Invoke(() =>
            {
                _labelRazerState.Content = Text;
            });
        }

        private void SendDataToWLEDUDP(WLEDSegment leds)
        {
            _udpClient.Send(leds.b, leds.b.Length);
        }

        private void TurnOn()
        {
            _ledsOn = true;
            byte[] bytes = Encoding.UTF8.GetBytes($"{{\"on\":true, \"bri\":{_appSettings.LEDBrightness}, \"nl.mode\":0}}");
            _udpClient.Send(bytes, bytes.Length);
        }
        private void TurnOff()
        {
            _ledsOn = false;
            byte[] bytes = Encoding.UTF8.GetBytes("{\"on\":false}");
            _udpClient.Send(bytes, bytes.Length);
        }

        RzResult OnChromaBroadcastEvent(RzChromaBroadcastType type, RzChromaBroadcastStatus? status, RzChromaBroadcastEffect? effect)
        {
            if (type == RzChromaBroadcastType.BroadcastEffect)
            {
                Dispatcher.Invoke(() =>
                {
                    if (BroadcastEnabled.IsChecked == true)
                    {
                        if (!_ledsOn) TurnOn();

                        int[] color1 = { effect.Value.ChromaLink2.R, effect.Value.ChromaLink2.G, effect.Value.ChromaLink2.B };
                        int[] color2 = { effect.Value.ChromaLink3.R, effect.Value.ChromaLink3.G, effect.Value.ChromaLink3.B };
                        int[] color3 = { effect.Value.ChromaLink4.R, effect.Value.ChromaLink4.G, effect.Value.ChromaLink4.B };
                        int[] color4 = { effect.Value.ChromaLink5.R, effect.Value.ChromaLink5.G, effect.Value.ChromaLink5.B };
                        var colors = new WLEDSegment(_totalLeds, color1, color2, color3, color4);

                        // var jsonI = Newtonsoft.Json.JsonConvert.SerializeObject(colors);
                        // var data = $"{{\"on\":true, \"seg\":{jsonI}}}";

                        SendDataToWLEDUDP(colors);

                        CL2.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink2.R, effect.Value.ChromaLink2.G, effect.Value.ChromaLink2.B));
                        CL3.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink3.R, effect.Value.ChromaLink3.G, effect.Value.ChromaLink3.B));
                        CL4.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink4.R, effect.Value.ChromaLink4.G, effect.Value.ChromaLink4.B));
                        CL5.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink5.R, effect.Value.ChromaLink5.G, effect.Value.ChromaLink5.B));
                    } else
                    {
                        ContextMenuItemSync.IsChecked = false;
                        Stop();
                    }
                });
            }
            else if (type == RzChromaBroadcastType.BroadcastStatus)
            {
                if (status == RzChromaBroadcastStatus.Live)
                {
                    UpdateLabelRazerState("Connected");
                }
                else if (status == RzChromaBroadcastStatus.NotLive)
                {
                    Stop();
                    UpdateLabelRazerState("Disconnected");
                }
            }

            return RzResult.Success;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void InitPowerEvents()
        {
            var q = new WqlEventQuery();
            var scope = new ManagementScope("root\\CIMV2");

            q.EventClassName = "Win32_PowerManagementEvent";
            managementEventWatcher = new ManagementEventWatcher(scope, q);
            managementEventWatcher.EventArrived += PowerEventArrive;
            managementEventWatcher.Start();
        }

        private void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            foreach (PropertyData pd in e.NewEvent.Properties)
            {
                if (pd == null || pd.Value == null) continue;
                var name = powerValues.ContainsKey(pd.Value.ToString()) ? powerValues[pd.Value.ToString()] : pd.Value.ToString();
                if (name == "Entering Suspend")
                {
                    Stop();
                }
            }
        }

        private void contextMenuSyncWithRazerCheck(object sender, RoutedEventArgs e)
        {
            BroadcastEnabled.IsChecked = true;
        }

        private void contextMenuSyncWithRazerUnCheck(object sender, RoutedEventArgs e)
        {
            BroadcastEnabled.IsChecked = false;
            Stop();
        }

        private void syncWithRazerUnCheck(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void quitApplication(object sender, RoutedEventArgs e)
        {
            Stop();
            RzChromaBroadcastAPI.UnRegisterEventNotification();
            RzChromaBroadcastAPI.UnInit();
            if (managementEventWatcher != null) managementEventWatcher.Stop();
            // End application, wish we didn't need this:
            Thread.Sleep(1000);
            Application.Current.Shutdown();
        }

        public void ShowWindow()
        {
            Show();
        }

        private void showApplication(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        public void runOnBootEnable()
        {
            rkApp.SetValue(AppName, Process.GetCurrentProcess().MainModule.FileName);
            ContextMenuItemRunAtBoot.IsChecked = true;
        }

        public void runOnBootDisable()
        {
            rkApp.DeleteValue(AppName, false);
            ContextMenuItemRunAtBoot.IsChecked = false;
        }

        private void contextMenuRunAtBootCheck(object sender, RoutedEventArgs e)
        {
            runOnBootEnable();
        }

        private void contextMenuRunAtBootUnCheck(object sender, RoutedEventArgs e)
        {
            runOnBootDisable();
        }

        private void ShowSettings()
        {
            SettingsWindow sw = new SettingsWindow(this, _appSettings, _runAtBoot);
            sw.Show();
        }

        private void settingsShowWindow(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void buttonOpenSettingsMenu(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }
    }
}

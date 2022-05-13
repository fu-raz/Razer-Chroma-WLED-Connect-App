// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaBroadcast;
using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HidApiAdapter;
using RazerChromaWLEDConnect.Base;
using RazerChromaWLEDConnect.WLED;

namespace RazerChromaWLEDConnect
{
    public partial class MainWindow : Window
    {
        protected Label _labelRazerState;
        protected AppSettings appSettings;

        protected HidDevice keyboardLenovo;

        private ManagementEventWatcher managementEventWatcher;
        private readonly Dictionary<string, string> powerValues = new Dictionary<string, string>
        {
            {"4", "Entering Suspend"},
            {"7", "Resume from Suspend"},
            {"10", "Power Status Change"},
            {"11", "OEM Event"},
            {"18", "Resume Automatic"}
        };

        public MainWindow()
        {
            InitializeComponent();

            // Get Settings
            this.appSettings = AppSettings.Load();
            _labelRazerState = RazerState;
            ContextMenuItemRunAtBoot.IsChecked = this.appSettings.RunAtBoot;
            BroadcastEnabled.IsChecked = this.appSettings.Sync;
            Init();
        }

        public void Init()
        {
            this.addLenovoInstances();
            this.addWLEDInstances();

            // Here we check if there are settings
            if (this.appSettings.RazerAppId != null && this.appSettings.RazerAppId.Length > 0)
            {
                // TODO: Make this optional
                if (this.appSettings.RunAtBoot) Hide();

                // TODO: Do this a little neater
                RzChromaBroadcastAPI.UnRegisterEventNotification();
                RzChromaBroadcastAPI.UnInit();

                // Initialize the Chrome Broadcast connection
                RzResult lResult = RzChromaBroadcastAPI.Init(Guid.Parse(this.appSettings.RazerAppId));
                if (lResult == RzResult.Success)
                {
                    // Init successful
                    UpdateLabelRazerState("Initialization Success");
                    // Start setting up the event
                    SetupEvents();
                }
                else
                {
                    // Init unsuccessful. Razer App Id is already in use or is invalid
                    UpdateLabelRazerState("Invalid Razer App Id");
                }
            }
            else
            {
                UpdateLabelRazerState("No Razer App Id");
            }
        }

        public void addWLEDInstances()
        {
            // Clear instanced and add them again
            wledInstances.Children.Clear();
            if (this.appSettings.Instances != null)
            {
                for (int i = 1; i <= this.appSettings.Instances.Count; i++)
                {
                    //WLED instance = this.appSettings.Instances[i - 1];
                    this.addWLEDInstancePreview(i, this.appSettings.Instances[i - 1]);
                    this.appSettings.Instances[i - 1].load();
                }
            }
        }
        private void addWLEDInstancePreview(int i, RGBSettingsInterface instance)
        {
            RGBPreviewControl wic = new RGBPreviewControl(ref instance, this, i);
            wledInstances.Children.Add(wic);
        }

        void SetupEvents()
        {
            // Hook into Razer 
            RzChromaBroadcastAPI.RegisterEventNotification(OnChromaBroadcastEvent);

            // Listen to windows going to sleep
            InitPowerEvents();
        }

        private void addLenovoInstances()
        {
            var devices = HidDeviceManager.GetManager().SearchDevices(0, 0);
            if (devices.Count > 0)
            {
                foreach (HidDevice device in devices)
                {
                    device.Connect();

                    if ((device.VendorId == 0x048d && device.ProductId == 0xc965 && device.UsagePage == 0xff89 && device.Usage == 0x00cc) ||
                        (device.VendorId == 0x048d && device.ProductId == 0xc955 && device.UsagePage == 0xff89 && device.Usage == 0x00cc))
                    {
                        LenovoKeyboard k = new LenovoKeyboard();

                        if (this.appSettings.Instances.Count > 0)
                        {
                            // Find if we already have a lenovo instance
                            bool found = false;
                            foreach (var instance in this.appSettings.Instances)
                            {
                                if (instance is LenovoKeyboard)
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                this.appSettings.Instances.Add(k);
                                this.appSettings.Save();
                                break;
                            }
                        }
                        else
                        {
                            this.appSettings.Instances.Add(k);
                            this.appSettings.Save();
                            break;
                        }
                    }

                    device.Disconnect();
                }

            }
        }

        private void UpdateLabelRazerState(string Text)
        {
            Dispatcher.Invoke(() =>
            {
                _labelRazerState.Content = Text;
            });
        }

        RzResult OnChromaBroadcastEvent(RzChromaBroadcastType type, RzChromaBroadcastStatus? status, RzChromaBroadcastEffect? effect)
        {
            // The Razer Chroma Event listener
            // This method gets triggered whenever Razer broadcasts something. This could be a color change or the disconnection of Chroma
            if (type == RzChromaBroadcastType.BroadcastEffect)
            {
                Dispatcher.Invoke(() =>
                {
                    if (ContextMenuItemSync.IsChecked != BroadcastEnabled.IsChecked) ContextMenuItemSync.IsChecked = (bool)BroadcastEnabled.IsChecked;

                    if (BroadcastEnabled.IsChecked == true)
                    {

                        int[] color1 = { effect.Value.ChromaLink2.R, effect.Value.ChromaLink2.G, effect.Value.ChromaLink2.B };
                        int[] color2 = { effect.Value.ChromaLink3.R, effect.Value.ChromaLink3.G, effect.Value.ChromaLink3.B };
                        int[] color3 = { effect.Value.ChromaLink4.R, effect.Value.ChromaLink4.G, effect.Value.ChromaLink4.B };
                        int[] color4 = { effect.Value.ChromaLink5.R, effect.Value.ChromaLink5.G, effect.Value.ChromaLink5.B };

                        foreach (RGBBase ledInstance in this.appSettings.Instances)
                        {
                            // TODO Has to be a better way to do this
                            if (ledInstance is LenovoKeyboard)
                            {
                                LenovoKeyboard l = (LenovoKeyboard)ledInstance;
                                l.sendColors(color1, color2, color3, color4);
                            } else
                            {
                                WLEDModule l = (WLEDModule)ledInstance;
                                l.sendColors(color1, color2, color3, color4);
                            }
                        }

                        if (this.WindowState != WindowState.Minimized)
                        {
                            CL2.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink2.R, effect.Value.ChromaLink2.G, effect.Value.ChromaLink2.B));
                            CL3.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink3.R, effect.Value.ChromaLink3.G, effect.Value.ChromaLink3.B));
                            CL4.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink4.R, effect.Value.ChromaLink4.G, effect.Value.ChromaLink4.B));
                            CL5.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink5.R, effect.Value.ChromaLink5.G, effect.Value.ChromaLink5.B));
                        }
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
                    UpdateLabelRazerState("Disconnected");
                }
            }

            return RzResult.Success;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to quit?", "Quit", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.Quit();
            }
            else
            {
                e.Cancel = true;
            }
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
                    for (int i = 0; i < this.appSettings.Instances.Count; i++)
                    {
                        this.appSettings.Instances[i].turnOff();
                    }
                }
                else if (name == "Resume from Suspend")
                {
                    for (int i = 0; i < this.appSettings.Instances.Count; i++)
                    {
                        this.appSettings.Instances[i].turnOn();
                    }
                }
            }
        }

        public void Quit()
        {
            RzChromaBroadcastAPI.UnRegisterEventNotification();
            RzChromaBroadcastAPI.UnInit();
            if (managementEventWatcher != null) managementEventWatcher.Stop();

            for (int i = 0; i < this.appSettings.Instances.Count; i++)
            {
                this.appSettings.Instances[i].unload();
            }

            Thread.Sleep(1000);
            Application.Current.Shutdown();
        }

        private void quitApplication(object sender, RoutedEventArgs e)
        {
            this.Quit();
        }

        public void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void showApplication(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void ShowSettings()
        {
            SettingsWindow sw = new SettingsWindow(this, this.appSettings);
            sw.Show();
        }

        private void settingsShowWindow(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void Sync(bool syncWithRazer)
        {
            this.appSettings.Sync = syncWithRazer;
            this.appSettings.Save();
        }

        private void syncWithRazer(object sender, RoutedEventArgs e)
        {
            this.Sync(true);
        }
        private void syncWithRazerUnCheck(object sender, RoutedEventArgs e)
        {
            this.Sync(false);
        }

        private void contextMenuSyncWithRazerCheck(object sender, RoutedEventArgs e)
        {
            this.Sync(true);
            BroadcastEnabled.IsChecked = true;
        }

        private void contextMenuSyncWithRazerUnCheck(object sender, RoutedEventArgs e)
        {
            this.Sync(false);
            BroadcastEnabled.IsChecked = false;
        }

        private void StateChange(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void contextMenuRunAtBootCheck(object sender, RoutedEventArgs e)
        {
            this.appSettings.RunAtBoot = true;
            this.appSettings.Save();
        }

        private void contextMenuRunAtBootUnCheck(object sender, RoutedEventArgs e)
        {
            this.appSettings.RunAtBoot = false;
            this.appSettings.Save();
        }
    }
}

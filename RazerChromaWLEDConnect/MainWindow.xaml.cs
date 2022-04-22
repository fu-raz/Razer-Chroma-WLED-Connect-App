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

namespace RazerChromaWLEDConnect
{
    public partial class MainWindow : Window
    {
        static readonly string AppName = "RazerChromaWLEDConnect";

        protected Label _labelRazerState;
        protected AppSettings appSettings;

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
            this.appSettings = AppSettings.Load();
            _labelRazerState = RazerState;
            ContextMenuItemRunAtBoot.IsChecked = (rkApp.GetValue(AppName) == null);
            BroadcastEnabled.IsChecked = this.appSettings.Sync;
            Init();
        }

        public void Init()
        {
            this.addWLEDInstances();

            // Here we check if there are settings
            if (this.appSettings.RazerAppId != null)
            {
                // TODO: Make this optional
                if (_runAtBoot) Hide();

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
                    WLEDInstance instance = this.appSettings.Instances[i - 1];
                    this.addWLEDInstancePreview(i, instance);
                    instance.load();
                }
            }
        }
        private void addWLEDInstancePreview(int i, WLEDInstance instance)
        {
            WLEDPreviewControl wic = new WLEDPreviewControl(ref instance, i);
            wledInstances.Children.Add(wic);
        }

        void SetupEvents()
        {
            // Hook into Razer 
            RzChromaBroadcastAPI.RegisterEventNotification(OnChromaBroadcastEvent);
            // Listen to windows going to sleep
            InitPowerEvents();
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
                        
                        foreach(WLEDInstance wledInstance in this.appSettings.Instances)
                        {
                            wledInstance.sendColors(color1, color2, color3, color4);
                        }

                        CL2.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink2.R, effect.Value.ChromaLink2.G, effect.Value.ChromaLink2.B));
                        CL3.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink3.R, effect.Value.ChromaLink3.G, effect.Value.ChromaLink3.B));
                        CL4.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink4.R, effect.Value.ChromaLink4.G, effect.Value.ChromaLink4.B));
                        CL5.Fill = new SolidColorBrush(Color.FromRgb(effect.Value.ChromaLink5.R, effect.Value.ChromaLink5.G, effect.Value.ChromaLink5.B));
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
            } else
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
                        WLEDInstance instance = this.appSettings.Instances[i];
                        instance.turnOff();
                    }
                } else if (name == "Resume from Suspend")
                {
                    for (int i = 0; i < this.appSettings.Instances.Count; i++)
                    {
                        WLEDInstance instance = this.appSettings.Instances[i];
                        instance.turnOn();
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
                WLEDInstance instance = this.appSettings.Instances[i];
                instance.unload();
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

        public void runOnBoot(bool enabled)
        {
            if (enabled)
            {
                rkApp.SetValue(AppName, Process.GetCurrentProcess().MainModule.FileName);
            } else
            {
                rkApp.DeleteValue(AppName, false);
            }

            ContextMenuItemRunAtBoot.IsChecked = enabled;
        }

        private void contextMenuRunAtBootCheck(object sender, RoutedEventArgs e)
        {
            runOnBoot(true);
        }

        private void contextMenuRunAtBootUnCheck(object sender, RoutedEventArgs e)
        {
            runOnBoot(false);
        }

        private void ShowSettings()
        {
            SettingsWindow sw = new SettingsWindow(this, this.appSettings, _runAtBoot);
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
    }
}

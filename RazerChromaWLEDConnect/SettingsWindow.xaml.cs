// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tmds.MDns;
using System.Linq;
using RazerChromaWLEDConnect.Base;
using RazerChromaWLEDConnect.WLED;
using System.ComponentModel;
using System;

namespace RazerChromaWLEDConnect
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        protected MainWindow mainWindow;
        protected AppSettings appSettings;
        private bool discoveryMode = false;

        private double windowStartHeight = 0;
        private double scrollBoxStartHeight = 0;

        public SettingsWindow(MainWindow win, AppSettings appSettings)
        {
            this.mainWindow = win;
            this.appSettings = appSettings;

            InitializeComponent();

            this.windowStartHeight = this.Height;
            this.scrollBoxStartHeight = this.wledScrollViewer.Height;

            if (this.appSettings.RunAtBoot) settingsStartOnBootCheckbox.IsChecked = true;
            settingsRazerAppId.Text = this.appSettings.RazerAppId;
            if (this.appSettings.Instances != null)
            {
                for (int i = 1; i <= this.appSettings.Instances.Count; i++)
                {
                    this.appSettings.Instances[i - 1].PropertyChanged += this.Instance_PropertyChanged;
                    this.addSettingsControl(i, this.appSettings.Instances[i - 1]);
                }
            }
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                this.mainWindow.addControls();
            }
        }

        public void ShowSettings()
        {
            Show();
        }

        private void settingsSave(object sender, RoutedEventArgs e)
        {
            bool shouldReInit = false;

            if (this.appSettings.RazerAppId != settingsRazerAppId.Text)
            {
                this.appSettings.RazerAppId = settingsRazerAppId.Text;
                shouldReInit = true;
            }

            this.appSettings.Save();

            if (shouldReInit) this.mainWindow.Init();

            this.mainWindow.addControls();
            this.mainWindow.ContextMenuItemRunAtBoot.IsChecked = this.appSettings.RunAtBoot;

            Hide();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void addSettingsControl(int i, RGBSettingsInterface instance)
        {
            RGBSettingsControl ctrl = new RGBSettingsControl(ref instance, this, i);
            wledInstances.Children.Add(ctrl);
        }

        private void addInstance(WLEDModule instance)
        {
            this.appSettings.Instances.Add(instance);
            this.addSettingsControl(this.appSettings.Instances.Count, instance);
            this.appSettings.Save();
            this.mainWindow.addControls();
        }

        private void addInstance(object sender, RoutedEventArgs e)
        {
            WLEDModule i = new WLEDModule();
            this.addInstance(i);
        }
        private void checkboxRunAtBootEnable(object sender, RoutedEventArgs e)
        {
            this.appSettings.RunAtBoot = true;
        }

        private void checkboxRunAtBootDisable(object sender, RoutedEventArgs e)
        {
            this.appSettings.RunAtBoot = false;
        }

        public void deleteInstance(RGBSettingsControl instanceControl)
        {
            // Find the instance 
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to delete this instance?", "Delete", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.appSettings.Instances.Remove((RGBBase)instanceControl.GetInstance());
                this.appSettings.Save();
                wledInstances.Children.Remove(instanceControl);
                this.mainWindow.addControls();
            }
        }


        private void findInstances(object sender, RoutedEventArgs e)
        {
            discoveryMode = !discoveryMode;
            Button b = sender as Button;
            if (b == null) return;

            // Ok here we need to find the instances via mDNS
            // Thank you Aircoookie
            ServiceBrowser serviceBrowser = new ServiceBrowser();
            serviceBrowser.ServiceAdded += this.OnServiceAdded;

            if (discoveryMode)
            {
                //Start mDNS discovery
                b.Content = "Searching... click to stop";
                serviceBrowser.StartBrowse("_http._tcp");
            }
            else
            {
                //Stop mDNS discovery
                b.Content = "Find WLED Instances";
                serviceBrowser.StopBrowse();
            }
        }

        private async void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            var addr = (e.Announcement.Addresses.Count >= 1) ? e.Announcement.Addresses[0] : null;
            if (addr != null)
            {
                // Fast way to check if it's a WLED. They have no TXT data
                if (e.Announcement.Txt.Count > 0 && e.Announcement.Txt[0] == "")
                {
                    // Let's test if the is an actual WLED instance
                    WLEDModule testInstance = new WLEDModule();
                    testInstance.WLEDIPAddress = addr.ToString();
                    testInstance.load();

                    if (testInstance.IsConnected)
                    {
                        // This is an actual WLED instance
                        // Check if one of this is one of the existing instances
                        bool shouldAdd = true;

                        foreach (RGBBase wi in this.appSettings.Instances)
                        {
                            if (wi is WLEDModule)
                            {
                                WLEDModule wLED = (WLEDModule)wi;
                                // If we already have this mac address,
                                // but the IP is different. We should change it
                                if (wLED.MacAddress == testInstance.MacAddress)
                                {
                                    shouldAdd = false;

                                    if (wLED.WLEDIPAddress != testInstance.WLEDIPAddress)
                                    {
                                        wLED.WLEDIPAddress = testInstance.WLEDIPAddress;
                                    }

                                    if (wLED.WLEDPort != testInstance.WLEDPort)
                                    {
                                        wLED.WLEDPort = testInstance.WLEDPort;
                                    }

                                    wLED.Segments = testInstance.Segments;

                                    // We found it, so BREAK BREAK!
                                    break;
                                }
                            }
                        }

                        if (shouldAdd)
                        {
                            this.addInstance(testInstance);
                        }

                        // Save the new or changed settings
                        this.appSettings.Save();
                    }
                }
                
                
            }
        }

        private void resizeWindow(object sender, SizeChangedEventArgs e)
        {
            double newHeight = this.Height;
            double diffHeight = newHeight - this.windowStartHeight;

            if (diffHeight > 0)
            {
                this.wledScrollViewer.Height = this.scrollBoxStartHeight + diffHeight;
            } else
            {
                this.wledScrollViewer.Height = this.scrollBoxStartHeight;
            }
        }
    }
}

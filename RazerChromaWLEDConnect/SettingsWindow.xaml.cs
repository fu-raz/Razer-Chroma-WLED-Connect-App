// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tmds.MDns;
using System.Linq;

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
                    WLEDInstance instance = this.appSettings.Instances[i - 1];
                    addWLEDInstanceControl(i, instance);
                }
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
            this.mainWindow.ContextMenuItemRunAtBoot.IsChecked = this.appSettings.RunAtBoot;

            Hide();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void addWLEDInstanceControl(int i, WLEDInstance instance)
        {
            WLEDInstanceControl wic = new WLEDInstanceControl(ref instance, this, i);
            wledInstances.Children.Add(wic);
        }

        private void addInstance(WLEDInstance instance)
        {
            this.appSettings.Instances.Add(instance);
            addWLEDInstanceControl(this.appSettings.Instances.Count, instance);
            this.appSettings.Save();
            this.mainWindow.addWLEDInstances();
        }

        private void addInstance(object sender, RoutedEventArgs e)
        {
            WLEDInstance i = new WLEDInstance();
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

        public void deleteInstance(WLEDInstanceControl instanceControl)
        {
            // Find the instance 
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to delete this instance?", "Delete", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.appSettings.Instances.Remove(instanceControl.getInstance());
                this.appSettings.Save();
                wledInstances.Children.Remove(instanceControl);
                this.mainWindow.addWLEDInstances();
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
                    WLEDInstance testInstance = new WLEDInstance();
                    testInstance.WLEDIPAddress = addr.ToString();
                    testInstance.load();

                    if (testInstance.IsConnected)
                    {
                        // This is an actual WLED instance
                        // Check if one of this is one of the existing instances
                        bool shouldAdd = true;

                        foreach (WLEDInstance wi in this.appSettings.Instances)
                        {
                            // If we already have this mac address,
                            // but the IP is different. We should change it
                            if (wi.MacAddress == testInstance.MacAddress)
                            {
                                shouldAdd = false;

                                if (wi.WLEDIPAddress != testInstance.WLEDIPAddress)
                                {
                                    wi.WLEDIPAddress = testInstance.WLEDIPAddress;
                                }

                                if (wi.WLEDPort != testInstance.WLEDPort)
                                {
                                    wi.WLEDPort = testInstance.WLEDPort;
                                }

                                wi.Segments = testInstance.Segments;

                                // We found it, so BREAK BREAK!
                                break;
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

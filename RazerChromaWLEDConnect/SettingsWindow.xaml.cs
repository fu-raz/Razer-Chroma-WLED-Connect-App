// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RazerChromaWLEDConnect
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        protected MainWindow _win;
        protected AppSettings appSettings;
        public SettingsWindow(MainWindow win, AppSettings appSettings, bool settingStartOnBoot)
        {
            _win = win;
            this.appSettings = appSettings;

            InitializeComponent();
            if (settingStartOnBoot) settingsStartOnBootCheckbox.IsChecked = true;
            settingsRazerAppId.Text = this.appSettings.RazerAppId;
            for(int i = 1; i <= this.appSettings.Instances.Count; i++)
            {
                WLEDInstance instance = this.appSettings.Instances[i - 1];
                addWLEDInstanceControl(i, instance);
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

            if (shouldReInit) _win.Init();

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

        private void addInstance(object sender, RoutedEventArgs e)
        {
            WLEDInstance i = new WLEDInstance();
            this.appSettings.Instances.Add(i);
            addWLEDInstanceControl(this.appSettings.Instances.Count, i);
            this.appSettings.Save();
        }
        private void checkboxRunAtBootEnable(object sender, RoutedEventArgs e)
        {
            _win.runOnBoot(true);
        }

        private void checkboxRunAtBootDisable(object sender, RoutedEventArgs e)
        {
            _win.runOnBoot(false);
        }

        public void deleteInstance(WLEDInstanceControl instanceControl)
        {
            // Find the instance 
            this.appSettings.Instances.Remove(instanceControl.getInstance());
            this.appSettings.Save();
            wledInstances.Children.Remove(instanceControl);
        }
    }
}

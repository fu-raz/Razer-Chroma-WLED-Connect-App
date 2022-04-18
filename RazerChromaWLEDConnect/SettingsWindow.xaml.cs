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

namespace ChromaBroadcastSampleApplication
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        protected MainWindow _win;
        protected AppSettings _appSettings;
        public SettingsWindow(MainWindow win, AppSettings appSettings, bool settingStartOnBoot)
        {
            _win = win;
            _appSettings = appSettings;

            InitializeComponent();
            if (settingStartOnBoot) settingsStartOnBootCheckbox.IsChecked = true;
            settingsRazerAppId.Text = _appSettings.RazerAppId;
            settingsWledIPAddress.Text = _appSettings.WledIPAddress;
            settingsLEDBrightness.Value = _appSettings.LEDBrightness;
        }

        public void ShowSettings()
        {
            Show();
        }

        private void settingsSave(object sender, RoutedEventArgs e)
        {
            _appSettings.RazerAppId = settingsRazerAppId.Text;
            _appSettings.WledIPAddress = settingsWledIPAddress.Text;
            _appSettings.WledUDPPort = int.Parse(settingsWledPort.Text);
            _appSettings.LEDBrightness = (int)settingsLEDBrightness.Value;
            _appSettings.Save();

            _win.Init();
            Hide();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void checkboxRunAtBootEnable(object sender, RoutedEventArgs e)
        {
            _win.runOnBootEnable();
        }

        private void checkboxRunAtBootDisable(object sender, RoutedEventArgs e)
        {
            _win.runOnBootDisable();
        }
    }
}

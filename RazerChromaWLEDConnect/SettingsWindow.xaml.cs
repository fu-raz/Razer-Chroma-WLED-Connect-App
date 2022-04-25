﻿// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RazerChromaWLEDConnect
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        protected MainWindow _win;
        protected AppSettings appSettings;
        public SettingsWindow(MainWindow win, AppSettings appSettings)
        {
            _win = win;
            this.appSettings = appSettings;

            InitializeComponent();
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

            if (shouldReInit) _win.Init();
            _win.ContextMenuItemRunAtBoot.IsChecked = this.appSettings.RunAtBoot;

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
            _win.addWLEDInstances();
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
                _win.addWLEDInstances();
            }
        }
    }
}

using RazerChromaWLEDConnect.Base;
using RazerChromaWLEDConnect.WLED;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RazerChromaWLEDConnect.WLED
{
    /// TODO: There is probably a MUCH better way to do this
    /// for now I just want it to work LOL
    public partial class WLEDPreviewControl : UserControl
    {
        protected WLEDModule instanceObject;
        protected int num;
        protected MainWindow mainWindow;

        public WLEDPreviewControl(ref RGBSettingsInterface instance, int num, MainWindow mainWindow)
        {
            this.instanceObject = (WLEDModule)instance;
            this.num = num;
            this.mainWindow = mainWindow;

            InitializeComponent();

            this.DataContext = instance;
            templateGroup.Header = this.instanceObject.Name;

            instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LEDs")
            {
                if (this.mainWindow.WindowState != WindowState.Minimized)
                {
                    if (this.instanceObject.LEDs.Count > 1)
                    {
                        LinearGradientBrush brush = new LinearGradientBrush();
                        for (int i = 0; i < this.instanceObject.LEDs.Count; i++)
                        {
                            int[] color = this.instanceObject.LEDs[i];

                            double offset = (double)1 / this.instanceObject.LEDs.Count * i;
                            GradientStop stop = new GradientStop(Color.FromRgb((byte)color[0], (byte)color[1], (byte)color[2]), offset);
                            brush.GradientStops.Add(stop);
                            double offset2 = (double)1 / this.instanceObject.LEDs.Count * (i + 1) - 0.00001;
                            GradientStop stop2 = new GradientStop(Color.FromRgb((byte)color[0], (byte)color[1], (byte)color[2]), offset2);
                            brush.GradientStops.Add(stop2);
                        }
                        Separator.Background = brush;
                    } else
                    {
                        int[] color = this.instanceObject.LEDs[0];
                        Separator.Background = new SolidColorBrush(Color.FromRgb((byte)color[0], (byte)color[1], (byte)color[2]));
                    }
                }
            } else if (e.PropertyName == "Enabled" || e.PropertyName == "IsConnected")
            {
                if (!this.instanceObject.Enabled)
                {
                    ConnectionStatus.Content = "Disabled";
                } else if (!this.instanceObject.IsConnected)
                {
                    ConnectionStatus.Content = "Offline";
                } else if (this.instanceObject.IsConnected)
                {
                    ConnectionStatus.Content = "Connected";
                }
            } else if (e.PropertyName == "Name")
            {
                templateGroup.Header = this.instanceObject.Name;
            }
        }
    }
}

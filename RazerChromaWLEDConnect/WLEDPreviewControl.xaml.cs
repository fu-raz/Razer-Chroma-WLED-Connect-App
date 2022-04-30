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

namespace RazerChromaWLEDConnect
{
    /// TODO: There is probably a MUCH better way to do this
    /// for now I just want it to work LOL
    public partial class WLEDPreviewControl : UserControl
    {
        protected WLEDInstance  instanceObject;
        protected int num;
        protected MainWindow mainWindow;

        public WLEDPreviewControl(ref WLEDInstance instance, int num, MainWindow mainWindow)
        {
            this.instanceObject = instance;
            this.num = num;
            this.mainWindow = mainWindow;

            InitializeComponent();

            this.DataContext = instance;
            templateGroup.Header = "WLED Instance #" + num.ToString();

            instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LEDs")
            {
                if (this.mainWindow.WindowState != WindowState.Minimized)
                {
                    LinearGradientBrush brush = new LinearGradientBrush();
                    for (int i = 0; i < this.instanceObject.LEDs.Count; i++)
                    {
                        int[] color = this.instanceObject.LEDs[i];

                        double offset = 1.000 / (this.instanceObject.LEDs.Count - 1) * i;
                        GradientStop stop = new GradientStop(Color.FromRgb(
                            (byte)color[0],
                            (byte)color[1],
                            (byte)color[2]
                        ), offset);

                        brush.GradientStops.Add(stop);
                    }
                    Separator.Background = brush;
                }
            } else if (e.PropertyName == "Enabled" || e.PropertyName == "IsConnected")
            {
                if (!this.instanceObject.Enabled)
                {
                    ConnectionStatus.Content = "Disabled";
                } else if (!this.instanceObject.IsConnected)
                {
                    ConnectionStatus.Content = "Disconnected";
                } else if (this.instanceObject.IsConnected)
                {
                    ConnectionStatus.Content = "Connected";
                }
            }
        }
    }
}

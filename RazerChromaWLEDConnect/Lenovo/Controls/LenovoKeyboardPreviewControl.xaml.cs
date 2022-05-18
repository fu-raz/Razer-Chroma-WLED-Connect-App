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

namespace RazerChromaWLEDConnect.Lenovo
{
    /// TODO: There is probably a MUCH better way to do this
    /// for now I just want it to work LOL
    public partial class LenovoKeyboardPreviewControl : UserControl
    {
        protected LenovoKeyboard instanceObject;
        protected int num;
        protected MainWindow mainWindow;

        public LenovoKeyboardPreviewControl(ref RGBSettingsInterface instance, int num, MainWindow mainWindow)
        {
            this.instanceObject = (LenovoKeyboard)instance;
            this.num = num;
            this.mainWindow = mainWindow;

            InitializeComponent();

            this.DataContext = instance;

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

                        double offset = (double)1 / this.instanceObject.LEDs.Count * i;
                        GradientStop stop = new GradientStop(Color.FromRgb((byte)color[0], (byte)color[1], (byte)color[2]), offset);
                        brush.GradientStops.Add(stop);
                        double offset2 = (double)1 / this.instanceObject.LEDs.Count * (i + 1) - 0.00001;
                        GradientStop stop2 = new GradientStop(Color.FromRgb((byte)color[0], (byte)color[1], (byte)color[2]), offset2);
                        brush.GradientStops.Add(stop2);
                    }
                    Separator.Background = brush;
                }
            }
        }
    }
}

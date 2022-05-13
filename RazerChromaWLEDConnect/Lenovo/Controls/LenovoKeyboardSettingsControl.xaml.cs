using RazerChromaWLEDConnect.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Interaction logic for LenovoKeyboardSettingsControl.xaml
    /// </summary>
    public partial class LenovoKeyboardSettingsControl : UserControl
    {
        protected LenovoKeyboard instanceObject;
        protected RGBSettingsControl parentControl;
        protected int num;
        public LenovoKeyboardSettingsControl(ref RGBSettingsInterface instance, RGBSettingsControl parentControl, int num)
        {
            this.instanceObject = (LenovoKeyboard)instance;
            this.parentControl = parentControl;
            this.num = num;

            this.Background = (num % 2 == 0) ? 
                new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)) : 
                new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));

            InitializeComponent();

            this.DataContext = instance;
        }
    }
}

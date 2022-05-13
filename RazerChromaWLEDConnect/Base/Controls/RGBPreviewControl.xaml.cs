using RazerChromaWLEDConnect.Base;
using RazerChromaWLEDConnect.Lenovo;
using RazerChromaWLEDConnect.WLED;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace RazerChromaWLEDConnect.Base
{
    /// TODO: There is probably a MUCH better way to do this
    /// for now I just want it to work LOL
    public partial class RGBPreviewControl : UserControl
    {
        protected MainWindow parent;
        protected RGBSettingsInterface _RGBSettings;
        public RGBPreviewControl(ref RGBSettingsInterface baseInstance, MainWindow parent, int num)
        {
            InitializeComponent();

            this._RGBSettings = baseInstance;
            this.parent = parent;

            if (baseInstance is LenovoKeyboard)
            {
                this.Content = new LenovoKeyboardPreviewControl(ref baseInstance, num, parent);
            } else
            {
                this.Content = new WLEDPreviewControl(ref baseInstance, num, parent);
            }
        }
    }
}

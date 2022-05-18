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
    public partial class RGBSettingsControl : UserControl
    {
        protected SettingsWindow parent;
        protected RGBSettingsInterface _RGBSettings;
        public RGBSettingsControl(ref RGBSettingsInterface baseInstance, SettingsWindow parent, int num)
        {
            InitializeComponent();

            this._RGBSettings = baseInstance;
            this.parent = parent;

            if (baseInstance is LenovoKeyboard)
            {
                this.Content = new LenovoKeyboardSettingsControl(ref baseInstance, this, num);
            } else
            {
                this.Content = new WLEDSettingsControl(ref baseInstance, this, num);
            }
        }

        public RGBSettingsInterface GetInstance()
        {
            return this._RGBSettings;
        }

        public void deleteInstance()
        {
            this.parent.deleteInstance(this);
        }
    }
}

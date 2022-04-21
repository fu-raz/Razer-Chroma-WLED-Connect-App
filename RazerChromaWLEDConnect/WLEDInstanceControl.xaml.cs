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
    public partial class WLEDInstanceControl : UserControl
    {
        protected WLEDInstance  instanceObject;
        protected SettingsWindow parentControl;
        protected int num;

        public WLEDInstanceControl(ref WLEDInstance instance, SettingsWindow parentControl, int num)
        {
            this.instanceObject = instance;
            this.parentControl = parentControl;
            this.num = num;

            InitializeComponent();

            this.DataContext = instance;
            templateGroup.Header = "WLED Instance #" + num.ToString();
            instance.load();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        protected void saveInstance()
        {
            this.instanceObject.reload();
        }

        public WLEDInstance getInstance()
        {
            return this.instanceObject;
        }

        private void deleteInstance(object sender, RoutedEventArgs e)
        {
            this.parentControl.deleteInstance(this);
        }

        private void saveIPAddress(object sender, RoutedEventArgs e)
        {
            this.saveInstance();
        }
        private void save(object sender, TextChangedEventArgs e)
        {
            this.saveInstance();
        }

        private void saveValue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.saveInstance();
        }

        private void saveCheckbox(object sender, RoutedEventArgs e)
        {
            this.saveInstance();
        }

    }
}

using RazerChromaWLEDConnect.Base;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RazerChromaWLEDConnect.WLED
{
    /// TODO: There is probably a MUCH better way to do this
    /// for now I just want it to work LOL
    public partial class WLEDSettingsControl : UserControl
    {
        protected WLEDModule instanceObject;
        protected RGBSettingsControl parentControl;
        protected int num;

        public WLEDSettingsControl(ref RGBSettingsInterface instance, RGBSettingsControl parentControl, int num)
        {
            this.instanceObject = (WLEDModule)instance;
            this.parentControl = parentControl;
            this.num = num;

            if (num % 2 == 0)
            {
                this.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            }
            else
            {
                this.Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
            }

            InitializeComponent();

            if (this.instanceObject.Segments == null || this.instanceObject.Segments.Count <= 1)
            {
                this.perSegmentSelection.Visibility = Visibility.Collapsed;
                this.instanceObject.ColorTypeStrip = true;
            }

            this.DataContext = instance;
            templateGroup.Header = this.instanceObject.Name;
            this.instanceObject.load();

            if (this.instanceObject.Segments != null && this.instanceObject.Segments.Count > 0)
            {
                this.AddSegments();
            }
        }

        private void AddSegments()
        {
            this.segmentSyncList.Children.Clear();

            for (int i = 0; i < this.instanceObject.Segments.Count; i++)
            {
                WLEDSegment segment = this.instanceObject.Segments[i];
                WLEDSegmentSyncColors wsc = new WLEDSegmentSyncColors(segment);
                this.segmentSyncList.Children.Add(wsc);
            }
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

        public WLEDModule getInstance()
        {
            return this.instanceObject;
        }

        private void deleteInstance(object sender, RoutedEventArgs e)
        {
            this.instanceObject.unload();
            this.parentControl.deleteInstance();
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

        private void openInBrowser(object sender, RoutedEventArgs e)
        {
            string url = this.instanceObject.getUrl();
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void checkboxEnableUnchecked(object sender, RoutedEventArgs e)
        {
            this.instanceObject.unload();
        }
    }
}

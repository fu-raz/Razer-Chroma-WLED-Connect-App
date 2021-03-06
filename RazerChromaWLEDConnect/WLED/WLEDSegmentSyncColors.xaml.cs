using RazerChromaWLEDConnect.WLED;
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

namespace RazerChromaWLEDConnect.WLED
{
    /// <summary>
    /// Interaction logic for WLEDSegmentSyncColors.xaml
    /// </summary>
    public partial class WLEDSegmentSyncColors : UserControl
    {
        protected WLEDSegment wledSegment;
        public WLEDSegmentSyncColors(WLEDSegment wledSegment)
        {
            this.wledSegment = wledSegment;
            InitializeComponent();

            this.DataContext = wledSegment;
            this.SegmentTitle.Content = $"LED# {wledSegment.start.ToString()} - {wledSegment.stop.ToString()}";
        }
    }
}

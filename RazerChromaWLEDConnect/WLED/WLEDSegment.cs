using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazerChromaWLEDConnect.WLED
{
    public class WLEDSegment : INotifyPropertyChanged
    {
        public int start = 0;
        public int stop = 0;
        public int len = 0;
        public bool rev = false;

        private bool _color1;
        public bool Color1
        {
            get { return _color1; }
            set { _color1 = value; OnPropertyChanged("Color1"); }
        }
        private bool _color2;
        public bool Color2
        {
            get { return _color2; }
            set { _color2 = value; OnPropertyChanged("Color2"); }
        }
        private bool _color3;
        public bool Color3
        {
            get { return _color3; }
            set { _color3 = value; OnPropertyChanged("Color3"); }
        }
        private bool _color4;
        public bool Color4
        {
            get { return _color4; }
            set { _color4 = value; OnPropertyChanged("Color4"); }
        }
        private bool _gradient;
        public bool Gradient
        {
            get { return _gradient; }
            set { _gradient = value; OnPropertyChanged("Gradient"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

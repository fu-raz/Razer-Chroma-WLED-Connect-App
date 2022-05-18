using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazerChromaWLEDConnect.Base
{
    public interface RGBSettingsInterface
    {
        int Type { get; set; }
        bool Enabled { get; set; }
        int LedCount { get; set; }
        bool Gradient { get; set; }
        double Brightness { get; set; }
        bool Led1 { get; set; }
        bool Led2 { get; set; }
        bool Led3 { get; set; }
        bool Led4 { get; set; }
        List<int[]> LEDs { get; set; }
        void turnOn();
        void turnOff();
        void load();
        void unload();
        
        event PropertyChangedEventHandler PropertyChanged;
    }
}

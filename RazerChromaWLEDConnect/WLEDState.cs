using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazerChromaWLEDConnect
{
    public class WLEDState
    {
        public bool on = false;
        public int bri = 255;
        public bool live = false;

        public WLEDState(bool on, int bri, bool live)
        {
            this.on = on;
            this.bri = bri;
            this.live = live;
        }
    }
}

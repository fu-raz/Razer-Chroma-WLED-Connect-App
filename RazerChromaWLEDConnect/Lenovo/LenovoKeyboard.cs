using HidApiAdapter;
using RazerChromaWLEDConnect.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazerChromaWLEDConnect
{
    public class LenovoKeyboard : RGBBase
    {
        protected new int _type = 1;
        public new int Type { get { return _type; } set { _type = value; } }

        protected new int _ledCount = 4;
        public new int LedCount { get { return _ledCount; } set { _ledCount = value; } }

        protected new double _brightness = 2;
        public new double Brightness { get { return _brightness; } set { _brightness = value; this.OnPropertyChanged("Brightness"); } }

        protected HidDevice hidDevice;
        public LenovoKeyboard()
        {
            var devices = HidDeviceManager.GetManager().SearchDevices(0, 0);
            if (devices.Count > 0)
            {
                foreach (HidDevice hidDevice in devices)
                {
                    hidDevice.Connect();

                    if ((hidDevice.VendorId == 0x048d && hidDevice.ProductId == 0xc965 && hidDevice.UsagePage == 0xff89 && hidDevice.Usage == 0x00cc) ||
                        (hidDevice.VendorId == 0x048d && hidDevice.ProductId == 0xc955 && hidDevice.UsagePage == 0xff89 && hidDevice.Usage == 0x00cc))
                    {
                        this.hidDevice = hidDevice;
                        break;
                    }
                }
            }
        }
        public new void sendColors(int[] color1, int[] color2, int[] color3, int[] color4)
        {
            List<int[]> colors = new List<int[]>();

            if (this.Led1) { colors.Add(color1); }
            if (this.Led2) { colors.Add(color2); }
            if (this.Led3) { colors.Add(color3); }
            if (this.Led4) { colors.Add(color4); }

            List<int[]> leds = this.getLEDs(colors, this.LedCount, this.Gradient);
            this.LEDs = leds;

            byte[] lenovoColors = new byte[33] {
                0xcc,
                0x16,
                0x01,
                1,
                2,
                (byte)leds[0][0],
                (byte)leds[0][1],
                (byte)leds[0][2],
                (byte)leds[1][0],
                (byte)leds[1][1],
                (byte)leds[1][2],
                (byte)leds[2][0],
                (byte)leds[2][1],
                (byte)leds[2][2],
                (byte)leds[3][0],
                (byte)leds[3][1],
                (byte)leds[3][2],
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
            };
            this.hidDevice.SendFeatureReport(lenovoColors);
        }
    }
}

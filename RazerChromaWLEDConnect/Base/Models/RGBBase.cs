using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RazerChromaWLEDConnect.WLED;

namespace RazerChromaWLEDConnect.Base
{
    [Serializable]
    [XmlInclude(typeof(WLEDModule))]
    [XmlInclude(typeof(LenovoKeyboard))]
    [XmlRoot("RGBBase")]
    public class RGBBase : INotifyPropertyChanged, RGBSettingsInterface
    {
        protected int _type;
        public virtual int Type { get { return _type; } set { _type = value; } }

        protected bool _enabled = false;
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged("Enabled"); }
        }

        protected int _ledCount = 30;
        public int LedCount
        {
            get { return _ledCount; }
            set { _ledCount = value; OnPropertyChanged("LedCount"); }
        }

        protected bool _gradient = true;
        public bool Gradient
        {
            get { return _gradient; }
            set { _gradient = value; OnPropertyChanged("Brightness"); }
        }

        protected double _brightness = 255;
        public double Brightness
        {
            get { return _brightness; }
            set { _brightness = value; OnPropertyChanged("Brightness"); }
        }

        protected bool _led1 = false;
        public bool Led1
        {
            get { return _led1; }
            set { _led1 = value; OnPropertyChanged("Led1"); }
        }
        protected bool _led2 = false;
        public bool Led2
        {
            get { return _led2; }
            set { _led2 = value; OnPropertyChanged("Led2"); }
        }
        protected bool _led3 = false;
        public bool Led3
        {
            get { return _led3; }
            set { _led3 = value; OnPropertyChanged("Led3"); }
        }
        protected bool _led4 = false;
        public bool Led4
        {
            get { return _led4; }
            set { _led4 = value; OnPropertyChanged("Led4"); }
        }

        protected List<int[]> _leds;
        public List<int[]> LEDs
        {
            get { return _leds; }
            set { _leds = value; OnPropertyChanged("LEDs"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<int[]> getLEDs(List<int[]> colors, int ledCount, bool gradient)
        {
            List<int[]> leds = new List<int[]>();

            if (colors.Count == 1)
            {
                // There's only one color, so all the LEDs get the same color
                for (int led = 0; led < ledCount; led++)
                {
                    leds.Add(colors[0]);
                }
            }
            else if (colors.Count > 1)
            {
                // There are more colors
                if (gradient)
                {
                    // Are we blending the colors?
                    // Determine how many steps are in between the colors
                    // * - - - - - - - - - - - - - *
                    // If we have 2 colors and 15 LEDs that would be (15 - 2) / 1 = 13
                    // * - - - - - - * - - - - - - *
                    // If we have 3 colors and 15 LEDs that would be (15 - 3) / 2 = 6
                    // * - - - - * - - - - * - - - -
                    // If we have 4 colors and 15 LEDs that would be (15 - 4) / 3 ~ 4
                    int ledsPerColor = (int)Math.Ceiling((float)(ledCount - colors.Count) / (colors.Count - 1));

                    // First color is always the first color
                    leds.Add(colors[0]);

                    for (int led = 1; led < ledCount; led++)
                    {
                        int[] pushColor = new int[3];

                        for (int colorNum = 1; colorNum < colors.Count; colorNum++)
                        {
                            if (led < colorNum * (ledsPerColor + 1))
                            {
                                // This is the color we're coming from
                                int[] baseColor = colors[colorNum - 1];

                                int[] nextColor = colors[colorNum];

                                // First we need to get the led for the current segment by modding by ledsPerColor;
                                int ledSegmentNum = led % (ledsPerColor + 1);

                                // Now we get the calculated color
                                pushColor[0] = (int)Math.Round(baseColor[0] + (float)(nextColor[0] - baseColor[0]) / (ledsPerColor + 1) * ledSegmentNum);
                                pushColor[1] = (int)Math.Round(baseColor[1] + (float)(nextColor[1] - baseColor[1]) / (ledsPerColor + 1) * ledSegmentNum);
                                pushColor[2] = (int)Math.Round(baseColor[2] + (float)(nextColor[2] - baseColor[2]) / (ledsPerColor + 1) * ledSegmentNum);

                                break;
                            }
                            else if (led == colorNum * (ledsPerColor + 1))
                            {
                                pushColor = colors[colorNum];
                                break;
                            }
                        }

                        leds.Add(pushColor);
                    }
                }
                else
                {
                    // We're not blending the colors
                    // * * * * - - - - + + + + = = =
                    // If we have 4 colors and 15 LEDs
                    // * * * * * - - - - - + + + + +
                    // If we have 3 colors and 15 LEDs
                    int ledsPerColor = (int)Math.Ceiling(ledCount / (float)colors.Count);
                    int currentLed = 0;
                    foreach (int[] color in colors)
                    {
                        for (int i = 0; i < ledsPerColor; i++)
                        {
                            leds.Add(color);
                            currentLed++;
                            if (currentLed >= ledCount) break;
                        }
                    }
                }
            }

            return leds;
        }

        public void turnOn()
        {

        }

        public void turnOff()
        {

        }

        public void unload()
        {

        }

        public void load()
        {

        }

        public void reload()
        {
            unload();
            load();
        }

        public virtual void sendColors(int[] color1, int[] color2, int[] color3, int[] color4)
        {

        }
    }
}

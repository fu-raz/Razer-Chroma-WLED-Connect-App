using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RazerChromaWLEDConnect
{
    public class WLEDInstance : INotifyPropertyChanged
    {
        private bool _enabled = false;
        public bool Enabled {
            get { return _enabled; }
            set { _enabled = value; this.OnPropertyChanged("Enabled"); }
        }

        private int _ledCount = 30;
        public int LedCount
        {
            get { return _ledCount; } 
            set { _ledCount = value; this.OnPropertyChanged("LedCount"); }
        }

        private string _wledIpAddress;
        public string WLEDIPAddress
        {
            get { return _wledIpAddress; }
            set { _wledIpAddress = value; this.OnPropertyChanged("WLEDIPAddress"); }
        }

        private int _wledPort = 21324;
        public int WLEDPort
        {
            get { return _wledPort; }
            set { _wledPort = value; this.OnPropertyChanged("WLEDPort"); }
        }

        private double _brightness = 255;
        public double Brightness
        {
            get { return _brightness; }
            set { _brightness = value; this.OnPropertyChanged("Brightness"); }
        }

        private bool _gradient = true;
        public bool Gradient
        {
            get { return _gradient; }
            set { _gradient = value; this.OnPropertyChanged("Brightness"); }
        }

        private bool _led1 = false;
        public bool Led1
        {
            get { return _led1; }
            set { _led1 = value; this.OnPropertyChanged("Led1"); }
        }
        private bool _led2 = false;
        public bool Led2
        {
            get { return _led2; }
            set { _led2 = value; this.OnPropertyChanged("Led2"); }
        }
        private bool _led3 = false;
        public bool Led3
        {
            get { return _led3; }
            set { _led3 = value; this.OnPropertyChanged("Led3"); }
        }
        private bool _led4 = false;
        public bool Led4
        {
            get { return _led4; }
            set { _led4 = value; this.OnPropertyChanged("Led4"); }
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; this.OnPropertyChanged("IsConnected"); }
        }
        public bool IsOn = false;

        private List<int[]> _colors;
        public List<int[]> Colors
        {
            get { return _colors; }
            set { _colors = value; this.OnPropertyChanged("Colors"); }
        }

        protected int[] lastColor1;
        protected int[] lastColor2;
        protected int[] lastColor3;
        protected int[] lastColor4;

        UdpClient udpClient;

        public event PropertyChangedEventHandler PropertyChanged;

        public WLEDInstance()
        {
            // We should check if it's a valid IP then do a call to get the led count
            int[] defaultColor = new int[3];
            defaultColor[0] = 0;
            defaultColor[1] = 0;
            defaultColor[2] = 0;

            if (Led1) this.Colors.Add(defaultColor);
            if (Led2) this.Colors.Add(defaultColor);
            if (Led3) this.Colors.Add(defaultColor);
            if (Led4) this.Colors.Add(defaultColor);

            this.lastColor1 = defaultColor;
            this.lastColor2 = defaultColor;
            this.lastColor3 = defaultColor;
            this.lastColor4 = defaultColor;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected UdpClient getUDPConnection()
        {
            if (this.udpClient == null)
            {
                this.udpClient = new UdpClient();
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(WLEDIPAddress), WLEDPort);
                try
                {
                    this.udpClient.Connect(ep);
                } catch
                {
                    this.udpClient.Dispose();
                    this.udpClient = null;
                }
            }

            return this.udpClient;
        }

        public void turnOn()
        {
            if (this.Enabled && this.IsConnected && !this.IsOn)
            {
                var state = new WLEDState(true, (int)Math.Round(this._brightness), true);
                byte[] json = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(state));
                UdpClient conn = getUDPConnection();
                if (conn != null)
                {
                    conn.Send(json, json.Length);
                    this.IsOn = true;
                }
            }
        }
        
        public void turnOff()
        {
            if (this.Enabled && this.IsConnected && this.IsOn)
            {
                var state = new WLEDState(false, (int)Math.Round(this._brightness), false);
                byte[] json = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(state));
                UdpClient conn = getUDPConnection();
                if (conn != null)
                {
                    conn.Send(json, json.Length);
                    this.IsOn = false;
                }
            }
        }

        public void reload()
        {
            unload();
            load();
        }

        public void unload()
        {
            if (this.udpClient != null)
            {
                if (this.IsOn) this.turnOff();
                this.udpClient.Close();
                this.udpClient.Dispose();
                this.udpClient = null;
            }
        }

        public void load()
        {
            // Define the object we get from the JSON API
            var WLEDStateObject = new
            {
                leds = new { count = 0, pwr = 0, fps = 0 }
            };
            // Create the json URL from the IP address
            try
            {
                string webUrl = $"http://{WLEDIPAddress}/json/info";
                var httpRequest = (HttpWebRequest)WebRequest.Create(webUrl);
                httpRequest.Accept = "application/json";
                httpRequest.Timeout = 2000;
                // Get the request response
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                var httpResult = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    httpResult = streamReader.ReadToEnd();
                }
                // Deserialize the JSON to the WLED State object
                var state = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(httpResult, WLEDStateObject);
                // Check if we have a state object in the response
                if (state != null)
                {
                    if (LedCount != state.leds.count)
                    {
                        LedCount = state.leds.count;
                    }

                    this.IsConnected = true;

                    if (this.Enabled)
                    {
                        this.turnOn();
                    }
                } else
                {
                    this.IsConnected = false;
                }
            } catch (Exception ex)
            {
                this.IsConnected = false;
            }
        }

        public void sendColors(int[] color1, int[] color2, int[] color3, int[] color4)
        {
            if (this.IsConnected && this.Enabled)
            {
                if (!this.IsOn) this.turnOn();

                List<int[]> colors = new List<int[]>();

                // Should update?
                bool shouldUpdate = false;

                // Add LEDs to the list
                if (Led1)
                {
                    
                    if (!color1.SequenceEqual(this.lastColor1))
                    {
                        colors.Add(color1);
                        shouldUpdate = true;
                        this.lastColor1 = color1;
                    }
                }
                if (Led2)
                {
                    if (!color2.SequenceEqual(this.lastColor2))
                    {
                        colors.Add(color2);
                        shouldUpdate = true;
                        this.lastColor2 = color2;
                    }
                }
                if (Led3)
                {
                    if (!color3.SequenceEqual(this.lastColor3))
                    {
                        colors.Add(color3);
                        shouldUpdate = true;
                        this.lastColor3 = color3;
                    }
                }
                if (Led4)
                {
                    if (!color4.SequenceEqual(this.lastColor4))
                    {
                        colors.Add(color4);
                        shouldUpdate = true;
                        this.lastColor4 = color4;
                    }
                }

                if (shouldUpdate)
                {
                    // Update colors of interface
                    this.Colors = colors;

                    byte[] colorBytes;

                    // Get the leds
                    List<int[]> leds = this.getLEDs(colors);

                    // Let's do some optimizing
                    // I guess if we're always going to be sending all the LEDS, we might as well use DRGB all the time
                    colorBytes = getUDPBytesDRGB(leds);

                    UdpClient conn = getUDPConnection();

                    if (conn != null)
                    {
                        conn.Send(colorBytes, colorBytes.Length);
                    }
                }
            }
        }

        public List<int[]> getLEDs(List<int[]> colors)
        {
            List<int[]> leds = new List<int[]>();

            if (colors.Count == 1)
            {
                // There's only one color, so all the LEDs get the same color
                for (int led = 0; led < LedCount; led++)
                {
                    leds.Add(colors[0]);
                }
            }
            else if (colors.Count > 1)
            {
                // There are more colors
                if (this.Gradient)
                {
                    // Are we blending the colors?
                    // Determine how many steps are in between the colors
                    // * - - - - - - - - - - - - - *
                    // If we have 2 colors and 15 LEDs that would be (15 - 2) / 1 = 13
                    // * - - - - - - * - - - - - - *
                    // If we have 3 colors and 15 LEDs that would be (15 - 3) / 2 = 6
                    // * - - - - * - - - - * - - - -
                    // If we have 4 colors and 15 LEDs that would be (15 - 4) / 3 ~ 4
                    int ledsPerColor = (int)Math.Ceiling((float)(this.LedCount - colors.Count) / (colors.Count - 1));

                    // First color is always the first color
                    leds.Add(colors[0]);

                    for (int led = 1; led < this.LedCount; led++)
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
                                pushColor[0] = (int)Math.Round(baseColor[0] + ((float)(nextColor[0] - baseColor[0]) / (ledsPerColor + 1)) * ledSegmentNum);
                                pushColor[1] = (int)Math.Round(baseColor[1] + ((float)(nextColor[1] - baseColor[1]) / (ledsPerColor + 1)) * ledSegmentNum);
                                pushColor[2] = (int)Math.Round(baseColor[2] + ((float)(nextColor[2] - baseColor[2]) / (ledsPerColor + 1)) * ledSegmentNum);

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
                    int ledsPerColor = (int)Math.Ceiling((float)this.LedCount / (float)colors.Count);
                    int currentLed = 0;
                    foreach(int[] color in colors)
                    {
                        for (int i = 0; i < ledsPerColor; i++)
                        {
                            leds.Add(color);
                            currentLed++;
                            if (currentLed >= this.LedCount) break;
                        }
                    }
                }
            }

            return leds;
        }

        protected byte[] getUDPBytesDRGB(List<int[]> leds)
        {
            byte[] colorBytes = new byte[leds.Count * 3 + 2];
            colorBytes[0] = 2;
            colorBytes[1] = 255;

            for (int i = 0; i < leds.Count; i++)
            {
                colorBytes[2 + i * 3] = (byte)leds[i][0];
                colorBytes[3 + i * 3] = (byte)leds[i][1];
                colorBytes[4 + i * 3] = (byte)leds[i][2];
            }

            return colorBytes;
        }
        public byte[] getUDPBytes(List<int[]> leds)
        {
            byte[] colorBytes = new byte[leds.Count * 4 + 2];
            colorBytes[0] = 1;
            colorBytes[1] = 255;

            for (int i = 0; i < leds.Count; i++)
            {
                colorBytes[2 + i * 4] = (byte)i;
                colorBytes[3 + i * 4] = (byte)leds[i][0];
                colorBytes[4 + i * 4] = (byte)leds[i][1];
                colorBytes[5 + i * 4] = (byte)leds[i][2];
            }

            return colorBytes;
        }
    }
}

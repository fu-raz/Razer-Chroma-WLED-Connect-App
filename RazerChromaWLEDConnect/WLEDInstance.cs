using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RazerChromaWLEDConnect
{
    public class WLEDInstance : INotifyPropertyChanged
    {
        private bool _enabled = false;
        public bool Enabled
        {
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

        private string _macAddress;
        public string MacAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; this.OnPropertyChanged("MacAddress"); }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; this.OnPropertyChanged("GroupName"); }
        }

        public bool IsOn = false;

        private List<int[]> _leds;
        public List<int[]> LEDs
        {
            get { return _leds; }
            set { _leds = value; this.OnPropertyChanged("LEDs"); }
        }

        private List<WLEDSegment> _segments;
        public List<WLEDSegment> Segments
        {
            get { return _segments; }
            set { _segments = value; this.OnPropertyChanged("Segments"); }
        }

        private bool _colorTypeStrip = true;
        public bool ColorTypeStrip
        {
            get { return _colorTypeStrip; }
            set { _colorTypeStrip = value; this.OnPropertyChanged("ColorTypeStrip"); }
        }

        private bool _colorTypeSegment = false;
        public bool ColorTypeSegment
        {
            get { return _colorTypeSegment; }
            set { _colorTypeSegment = value; this.OnPropertyChanged("ColorTypeSegment"); }
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

            this.lastColor1 = defaultColor;
            this.lastColor2 = defaultColor;
            this.lastColor3 = defaultColor;
            this.lastColor4 = defaultColor;

            this.GroupName = Guid.NewGuid().ToString();
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
                }
                catch
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

        public string getUrl()
        {
            return $"http://{WLEDIPAddress}";
        }

        public void load()
        {
            // Define the object we get from the JSON API

            var WLEDStateObject = new
            {
                state = new {
                    on = false,
                    seg = new List<WLEDSegment>()
                },
                info = new {
                    leds = new { count = 0, pwr = 0, fps = 0 },
                    name = "",
                    udpport = _wledPort,
                    vid = 0,
                    mac = ""
                }
            };

            // Create the json URL from the IP address
            try
            {
                string webUrl = $"{this.getUrl()}/json";
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
                var wled = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(httpResult, WLEDStateObject);
                // Check if we have a state object in the response
                if (wled != null)
                {
                    // Get LED amount from JSON API
                    if (this.LedCount != wled.info.leds.count)
                    {
                        this.LedCount = wled.info.leds.count;
                    }

                    // Get UDP port from JSON API
                    if (this.WLEDPort != wled.info.udpport)
                    {
                        this.WLEDPort = wled.info.udpport;
                    }

                    // Get MAC Address for id from JSON API
                    if (this.MacAddress != wled.info.mac)
                    {
                        this.MacAddress = wled.info.mac;
                    }

                    this.Segments = wled.state.seg;

                    this.IsConnected = true;

                    if (this.Enabled)
                    {
                        this.turnOn();
                    }
                }
                else
                {
                    this.IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                this.IsConnected = false;
            }
        }

        public void sendColors(int[] color1, int[] color2, int[] color3, int[] color4)
        {
            if (this.IsConnected && this.Enabled)
            {
                if (!this.IsOn) this.turnOn();

                if (this.ColorTypeStrip)
                {
                    List<int[]> colors = new List<int[]>();

                    // Should update?
                    bool shouldUpdate = false;

                    // Add LEDs to the list
                    // Here we optimize by checking if the new led color is the same as the previous color
                    // If there's nothing to update, we don't update

                    // TODO: We could keep track of all previous leds and only change the ones we need
                    // if that's a realistic scenario
                    if (this.Led1)
                    {
                        colors.Add(color1);
                        if (!color1.SequenceEqual(this.lastColor1))
                        {
                            shouldUpdate = true;
                            this.lastColor1 = color1;
                        }
                    }
                    if (this.Led2)
                    {
                        colors.Add(color2);
                        if (!color2.SequenceEqual(this.lastColor2))
                        {
                            shouldUpdate = true;
                            this.lastColor2 = color2;
                        }
                    }
                    if (this.Led3)
                    {
                        colors.Add(color3);
                        if (!color3.SequenceEqual(this.lastColor3))
                        {
                            shouldUpdate = true;
                            this.lastColor3 = color3;
                        }
                    }
                    if (this.Led4)
                    {
                        colors.Add(color4);
                        if (!color4.SequenceEqual(this.lastColor4))
                        {
                            shouldUpdate = true;
                            this.lastColor4 = color4;
                        }
                    }

                    if (shouldUpdate)
                    {
                        byte[] colorBytes;

                        // Get the leds
                        List<int[]> leds = this.getLEDs(colors, this.LedCount, this.Gradient);

                        // Let's do some optimizing
                        // I guess if we're always going to be sending all the LEDS, we might as well use DRGB all the time
                        colorBytes = getUDPBytesDRGB(leds);

                        UdpClient conn = getUDPConnection();

                        if (conn != null)
                        {
                            conn.Send(colorBytes, colorBytes.Length);
                        }
                    }
                } else if (this.ColorTypeSegment)
                {
                    List<int[]> leds = new List<int[]>();

                    foreach (WLEDSegment wledSegment in this.Segments)
                    {
                        List<int[]> colors = new List<int[]>();
                        if (wledSegment.Color1) colors.Add(color1);
                        if (wledSegment.Color2) colors.Add(color2);
                        if (wledSegment.Color3) colors.Add(color3);
                        if (wledSegment.Color4) colors.Add(color4);

                        List<int[]> segmentLeds = this.getLEDs(colors, wledSegment.len, wledSegment.Gradient);
                        foreach (int[] segmentLed in segmentLeds)
                        {
                            leds.Add(segmentLed);
                        }
                    }

                    byte[] colorBytes;
                    colorBytes = getUDPBytesDRGB(leds);
                    UdpClient conn = getUDPConnection();

                    if (conn != null)
                    {
                        conn.Send(colorBytes, colorBytes.Length);
                    }
                }
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
                    int ledsPerColor = (int)Math.Ceiling((float)ledCount / (float)colors.Count);
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

            this.LEDs = leds;

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

using RazerChromaWLEDConnect.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RazerChromaWLEDConnect.WLED
{
    public class WLEDModule : RGBBase
    {
        protected new int _type = 0;

        private string _wledIpAddress;
        public string WLEDIPAddress
        {
            get { return _wledIpAddress; }
            set { _wledIpAddress = value; OnPropertyChanged("WLEDIPAddress"); }
        }

        private int _wledPort = 21324;
        public int WLEDPort
        {
            get { return _wledPort; }
            set { _wledPort = value; OnPropertyChanged("WLEDPort"); }
        }
        
        private string _macAddress;
        public string MacAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; OnPropertyChanged("MacAddress"); }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; OnPropertyChanged("GroupName"); }
        }

        public bool IsOn = false;

        private List<WLEDSegment> _segments;
        public List<WLEDSegment> Segments
        {
            get { return _segments; }
            set { _segments = value; OnPropertyChanged("Segments"); }
        }

        private bool _colorTypeStrip = true;
        public bool ColorTypeStrip
        {
            get { return _colorTypeStrip; }
            set { _colorTypeStrip = value; OnPropertyChanged("ColorTypeStrip"); }
        }

        private bool _colorTypeSegment = false;
        public bool ColorTypeSegment
        {
            get { return _colorTypeSegment; }
            set { _colorTypeSegment = value; OnPropertyChanged("ColorTypeSegment"); }
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        protected long lastCheck = 0;

        protected int[] lastColor1;
        protected int[] lastColor2;
        protected int[] lastColor3;
        protected int[] lastColor4;

        UdpClient udpClient;

        public WLEDModule()
        {
            // We should check if it's a valid IP then do a call to get the led count
            int[] defaultColor = new int[3];
            defaultColor[0] = 0;
            defaultColor[1] = 0;
            defaultColor[2] = 0;

            lastColor1 = defaultColor;
            lastColor2 = defaultColor;
            lastColor3 = defaultColor;
            lastColor4 = defaultColor;

            GroupName = Guid.NewGuid().ToString();
        }

        protected UdpClient getUDPConnection()
        {
            if (this.udpClient == null)
            {
                this.udpClient = new UdpClient();
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(this.WLEDIPAddress), this.WLEDPort);
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

            return udpClient;
        }

        public override void turnOn()
        {
            if (Enabled && IsConnected && !IsOn)
            {
                WLEDState state = new WLEDState(true, (int)Math.Round(_brightness), true);
                byte[] json = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(state));
                UdpClient conn = getUDPConnection();
                if (conn != null)
                {
                    conn.Send(json, json.Length);
                    IsOn = true;
                }
            }
        }

        public override void turnOff()
        {
            if (IsConnected && IsOn)
            {
                // Get a byte list of black LEDS
                List<int[]> blackLEDS = new List<int[]>();
                for(int i = 0; i < this.LedCount; i++)
                {
                    blackLEDS.Add(this.getBlack());
                }
                // Get the black leds with timeout
                byte[] black = this.getUDPBytesDRGB(blackLEDS, 1);

                WLEDState state = new WLEDState(false, (int)Math.Round(_brightness), false);
                byte[] json = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(state));

                UdpClient conn = this.getUDPConnection();
                if (conn != null)
                {
                    // Send black
                    conn.Send(black, black.Length);
                    // TODO: Get this from settings?
                    // Turn off
                    conn.Send(json, json.Length);
                    IsOn = false;
                }
            }
        }

        public override void unload()
        {
            if (this.udpClient != null)
            {
                this.turnOff();
                this.udpClient.Close();
                this.udpClient.Dispose();
                this.udpClient = null;
            }
        }

        public string getUrl()
        {
            return $"http://{WLEDIPAddress}";
        }

        public override void load()
        {
            // Define the object we get from the JSON API

            var WLEDStateObject = new
            {
                state = new
                {
                    on = false,
                    seg = new List<WLEDSegment>()
                },
                info = new
                {
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
                string webUrl = $"{getUrl()}/json";
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

                    if (this.Segments == null || this.Segments.Count != wled.state.seg.Count)
                    {
                        this.Segments = wled.state.seg;
                    }

                    if (this.Name != wled.info.name)
                    {
                        this.Name = wled.info.name;
                    }

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

        public override void sendColors(int[] color1, int[] color2, int[] color3, int[] color4)
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
                    
                    // Get the leds for the preview window
                    List<int[]> leds = this.getLEDs(colors, this.LedCount, this.Gradient);
                    if (this.LEDs != null && !this.LEDs.SequenceEqual(leds))
                    {
                        this.LEDs = leds;
                    }

                    if (shouldUpdate || this.hasTimedOut(60))
                    {
                        byte[] colorBytes;

                        
                        // Let's do some optimizing
                        // I guess if we're always going to be sending all the LEDS, we might as well use DRGB all the time
                        colorBytes = this.getUDPBytesDRGB(leds, 120);

                        UdpClient conn = this.getUDPConnection();

                        if (conn != null)
                        {
                            conn.Send(colorBytes, colorBytes.Length);
                        }
                    }
                }
                else if (this.ColorTypeSegment)
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
                    this.LEDs = leds;

                    byte[] colorBytes;
                    colorBytes = this.getUDPBytesDRGB(leds);
                    UdpClient conn = this.getUDPConnection();

                    if (conn != null)
                    {
                        conn.Send(colorBytes, colorBytes.Length);
                    }
                }
            } else if (this.Enabled)
            {
                // We should try every x seconds if we can connect to WLED
                // could be that the connection dropped
                // This sendColors function get fired very often

                DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
                long timeStamp = now.ToUnixTimeSeconds();

                // TODO: maybe make this a setting?
                if (timeStamp - this.lastCheck >= 60)
                {
                    this.lastCheck = timeStamp;
                    this.load();
                }
            }
        }

        protected byte[] getUDPBytesDRGB(List<int[]> leds, int timeout = 255)
        {
            byte[] colorBytes = new byte[leds.Count * 3 + 2];
            colorBytes[0] = 2;
            colorBytes[1] = (byte)timeout;

            for (int i = 0; i < leds.Count; i++)
            {
                colorBytes[2 + i * 3] = (byte)leds[i][0];
                colorBytes[3 + i * 3] = (byte)leds[i][1];
                colorBytes[4 + i * 3] = (byte)leds[i][2];
            }

            return colorBytes;
        }
    }
}

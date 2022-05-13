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

        private bool _isConnected = false;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; OnPropertyChanged("IsConnected"); }
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
            if (udpClient == null)
            {
                udpClient = new UdpClient();
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(WLEDIPAddress), WLEDPort);
                try
                {
                    udpClient.Connect(ep);
                }
                catch
                {
                    udpClient.Dispose();
                    udpClient = null;
                }
            }

            return udpClient;
        }

        public new void turnOn()
        {
            if (Enabled && IsConnected && !IsOn)
            {
                var state = new WLEDState(true, (int)Math.Round(_brightness), true);
                byte[] json = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(state));
                UdpClient conn = getUDPConnection();
                if (conn != null)
                {
                    conn.Send(json, json.Length);
                    IsOn = true;
                }
            }
        }

        public new void turnOff()
        {
            if (Enabled && IsConnected && IsOn)
            {
                var state = new WLEDState(false, (int)Math.Round(_brightness), false);
                byte[] json = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(state));
                UdpClient conn = getUDPConnection();
                if (conn != null)
                {
                    conn.Send(json, json.Length);
                    IsOn = false;
                }
            }
        }

        public new void unload()
        {
            if (udpClient != null)
            {
                if (IsOn) turnOff();
                udpClient.Close();
                udpClient.Dispose();
                udpClient = null;
            }
        }

        public string getUrl()
        {
            return $"http://{WLEDIPAddress}";
        }

        public new void load()
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
                    if (LedCount != wled.info.leds.count)
                    {
                        LedCount = wled.info.leds.count;
                    }

                    // Get UDP port from JSON API
                    if (WLEDPort != wled.info.udpport)
                    {
                        WLEDPort = wled.info.udpport;
                    }

                    // Get MAC Address for id from JSON API
                    if (MacAddress != wled.info.mac)
                    {
                        MacAddress = wled.info.mac;
                    }

                    if (Segments == null || Segments.Count != wled.state.seg.Count)
                    {
                        Segments = wled.state.seg;
                    }

                    IsConnected = true;

                    if (Enabled)
                    {
                        turnOn();
                    }
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
            }
        }

        public new void sendColors(int[] color1, int[] color2, int[] color3, int[] color4)
        {
            if (IsConnected && Enabled)
            {
                if (!IsOn) turnOn();

                if (ColorTypeStrip)
                {
                    List<int[]> colors = new List<int[]>();

                    // Should update?
                    bool shouldUpdate = false;

                    // Add LEDs to the list
                    // Here we optimize by checking if the new led color is the same as the previous color
                    // If there's nothing to update, we don't update

                    // TODO: We could keep track of all previous leds and only change the ones we need
                    // if that's a realistic scenario
                    if (Led1)
                    {
                        colors.Add(color1);
                        if (!color1.SequenceEqual(lastColor1))
                        {
                            shouldUpdate = true;
                            lastColor1 = color1;
                        }
                    }
                    if (Led2)
                    {
                        colors.Add(color2);
                        if (!color2.SequenceEqual(lastColor2))
                        {
                            shouldUpdate = true;
                            lastColor2 = color2;
                        }
                    }
                    if (Led3)
                    {
                        colors.Add(color3);
                        if (!color3.SequenceEqual(lastColor3))
                        {
                            shouldUpdate = true;
                            lastColor3 = color3;
                        }
                    }
                    if (Led4)
                    {
                        colors.Add(color4);
                        if (!color4.SequenceEqual(lastColor4))
                        {
                            shouldUpdate = true;
                            lastColor4 = color4;
                        }
                    }

                    if (shouldUpdate)
                    {
                        byte[] colorBytes;

                        // Get the leds
                        List<int[]> leds = getLEDs(colors, LedCount, Gradient);

                        LEDs = leds;
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
                else if (ColorTypeSegment)
                {
                    List<int[]> leds = new List<int[]>();

                    foreach (WLEDSegment wledSegment in Segments)
                    {
                        List<int[]> colors = new List<int[]>();
                        if (wledSegment.Color1) colors.Add(color1);
                        if (wledSegment.Color2) colors.Add(color2);
                        if (wledSegment.Color3) colors.Add(color3);
                        if (wledSegment.Color4) colors.Add(color4);

                        List<int[]> segmentLeds = getLEDs(colors, wledSegment.len, wledSegment.Gradient);
                        foreach (int[] segmentLed in segmentLeds)
                        {
                            leds.Add(segmentLed);
                        }
                    }
                    LEDs = leds;

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

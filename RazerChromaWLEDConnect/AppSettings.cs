// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RazerChromaWLEDConnect
{
    public class AppSettings
    {
        private static readonly string pathSettingsFile = System.AppDomain.CurrentDomain.BaseDirectory + "\\settings.xml";

        public string RazerAppId = null;
        public bool Sync = false;
        public string WledIPAddress = null;
        public int WledUDPPort = 21324;
        public int LEDBrightness = 255;
        public List<WLEDInstance> Instances = new List<WLEDInstance>();
        public bool RunAtBoot = false;

        static readonly string AppName = "RazerChromaWLEDConnect";
        protected RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public AppSettings()
        {
            this.RunAtBoot = (rkApp.GetValue(AppName) != null);
        }

        public void Save()
        {
            // if (!File.Exists(pathSettingsFile)) File.Create(pathSettingsFile);
            if (RunAtBoot)
            {
                rkApp.SetValue(AppName, Process.GetCurrentProcess().MainModule.FileName);
            }
            else
            {
                rkApp.DeleteValue(AppName, false);
            }

            using (StreamWriter sw = new StreamWriter(pathSettingsFile))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                xmls.Serialize(sw, this);
            }
        }
        public static AppSettings Load()
        {
            if (File.Exists(pathSettingsFile))
            {
                using (StreamReader sw = new StreamReader(pathSettingsFile))
                {
                    XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                    return xmls.Deserialize(sw) as AppSettings;
                }
            }
            else
            {
                return new AppSettings();
            }
        }
    }
}

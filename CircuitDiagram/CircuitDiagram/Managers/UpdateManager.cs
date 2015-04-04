using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CircuitDiagram
{
    static class UpdateManager
    {
        private const string UpdateDocUrl = "http://www.circuit-diagram.org/app/appversion.xml";

        public static readonly BuildChannelAttribute BuildChannelVersion;

        public static readonly Version BuildVersion;

        public static UpdateChannelType UpdateChannel { get; set; }

        public static string AppDisplayVersion { get; private set; }

        static UpdateManager()
        {
            // Fetch the build channel attribute the program was compiled with
            System.Reflection.Assembly _assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            BuildVersion = _assemblyInfo.GetName().Version;
            BuildChannelVersion = _assemblyInfo.GetCustomAttributes(typeof(BuildChannelAttribute), false).FirstOrDefault(item => item is BuildChannelAttribute) as BuildChannelAttribute;
            if (BuildChannelVersion == null)
                BuildChannelVersion = new BuildChannelAttribute("", UpdateChannelType.Stable);

            // Check if the update channel has been overridden by the user
            if (SettingsManager.Settings.HasSetting("updateChannel"))
            {
                UpdateChannelType updateChannel;
                if (Enum.TryParse<UpdateChannelType>(SettingsManager.Settings.Read("updateChannel") as string, out updateChannel))
                    UpdateChannel = updateChannel;
            }
            else
            {
                UpdateChannel = BuildChannelVersion.UpdateChannel;
            }
            
            // Set app display version
            AppDisplayVersion = String.Format("{0}.{1}.{2} {3} ", BuildVersion.Major, BuildVersion.Minor, BuildVersion.Build, BuildChannelVersion.DisplayName);
#if PORTABLE
            AppDisplayVersion += String.Format("(Portable, Build {0})", BuildVersion.Revision);
#elif DEBUG
            AppDisplayVersion += String.Format("(Debug, Build {0})", BuildVersion.Revision);
#else
            AppDisplayVersion += String.Format("(Build {0})", BuildVersion.Revision);
#endif
        }

        public static UpdateDetails CheckForUpdates()
        {
            System.Net.WebRequest updateDocStream = System.Net.WebRequest.Create(UpdateDocUrl);
            XmlDocument updateDoc = new XmlDocument();
            updateDoc.Load(updateDocStream.GetResponse().GetResponseStream());

            // Navigate to selected build channel
            XmlNode channel = updateDoc.SelectSingleNode(String.Format("/version/application[@name='CircuitDiagram']/channel[@name='{0}']",
                BuildChannelVersion.UpdateChannel.ToString()));

            if (channel == null)
            {
                // Use default channel
                channel = updateDoc.SelectSingleNode("/version/application[@name='CircuitDiagram']/channel");
            }

            if (channel == null)
            {
                // Unable to find any channel
                throw new System.IO.InvalidDataException("No suitable update channel could be found.");
            }

            Version serverVersion = null;
            string serverVersionName = null;
            string serverDownloadUrl = null;

            foreach (XmlNode childNode in channel.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "version":
                        serverVersion = new Version(childNode.InnerText);
                        break;
                    case "name":
                        serverVersionName = childNode.InnerText;
                        break;
                    case "url":
                        serverDownloadUrl = childNode.InnerText;
                        break;
                }
            }

            if (serverVersion != null && BuildVersion.CompareTo(serverVersion) < 0)
            {
                return new UpdateDetails() { Version = serverVersionName, DownloadUrl = serverDownloadUrl };
            }
            else
            {
                // No new version available
                return null;
            }
        }
    }
}

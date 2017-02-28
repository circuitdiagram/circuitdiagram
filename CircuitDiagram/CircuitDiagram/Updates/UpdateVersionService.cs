// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CircuitDiagram.View.Services;

namespace CircuitDiagram.Updates
{
    class UpdateVersionService : IUpdateVersionService
    {
        private const string UpdateDocUrl = "http://www.circuit-diagram.org/app/appversion.xml";

        private static readonly Lazy<string> AppDisplayVersion = new Lazy<string>(LoadAppDisplayVersion);

        private readonly Version buildVersion;
        private readonly UpdateChannelType updateChannel;

        public UpdateVersionService()
        {
            // Fetch the build channel attribute the program was compiled with
            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            buildVersion = assemblyInfo.GetName().Version;
            var buildChannelVersion = assemblyInfo.GetCustomAttributes(typeof(BuildChannelAttribute), false).FirstOrDefault(item => item is BuildChannelAttribute) as BuildChannelAttribute ??
                                      new BuildChannelAttribute("", UpdateChannelType.Stable);

            // Check if the update channel has been overridden by the user
            //if (SettingsManager.Settings.HasSetting("updateChannel"))
            //{
            //    UpdateChannelType updateChannel;
            //    if (Enum.TryParse<UpdateChannelType>(SettingsManager.Settings.Read("updateChannel") as string, out updateChannel))
            //        UpdateChannel = updateChannel;
            //}
            //else
            //{
                updateChannel = buildChannelVersion.UpdateChannel;
            //}
        }

        private static string LoadAppDisplayVersion()
        {
            // Fetch the build channel attribute the program was compiled with
            var assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assemblyInfo.GetName().Version;
            var buildChannelVersion = assemblyInfo.GetCustomAttributes(typeof(BuildChannelAttribute), false).FirstOrDefault(item => item is BuildChannelAttribute) as BuildChannelAttribute ??
                                      new BuildChannelAttribute("", UpdateChannelType.Stable);
            
            string displayVersion = $"{version.Major}.{version.Minor}.{version.Build} {buildChannelVersion.DisplayName} ";
#if DEBUG
            displayVersion += $"(Debug, Build {version.Revision})";
#else
            displayVersion += String.Format("(Build {0})", version.Revision);
#endif

            return displayVersion;
        }

        public static string GetAppDisplayVersion() => AppDisplayVersion.Value;

        string IUpdateVersionService.GetAppDisplayVersion() => AppDisplayVersion.Value;
        
        public IEnumerable<string> GetUpdateChannels()
        {
            return Enum.GetNames(typeof(UpdateChannelType));
        }

        public string GetSelectedUpdateChannel()
        {
            return updateChannel.ToString();
        }

        public void SetUpdateChannel(string channel)
        {
            throw new NotImplementedException();
        }

        public async Task<UpdateDetails> CheckForUpdates()
        {
            var updateDocStream = await WebRequest.Create(UpdateDocUrl).GetResponseAsync();

            var updateDoc = XDocument.Load(updateDocStream.GetResponseStream());

            // Navigate to selected build channel
            var channel = updateDoc.XPathSelectElement($"/version/application[@name='CircuitDiagram']/channel[@name='{updateChannel}']");

            if (channel == null)
            {
                // Use default channel
                channel = updateDoc.XPathSelectElement("/version/application[@name='CircuitDiagram']/channel");
            }

            if (channel == null)
            {
                // Unable to find any channel
                throw new System.IO.InvalidDataException("No suitable update channel could be found.");
            }

            Version serverVersion = null;
            string serverVersionName = null;
            string serverDownloadUrl = null;

            foreach (var childNode in channel.Elements())
            {
                switch (childNode.Name.LocalName)
                {
                    case "version":
                        serverVersion = new Version(childNode.Value);
                        break;
                    case "name":
                        serverVersionName = childNode.Value;
                        break;
                    case "url":
                        serverDownloadUrl = childNode.Value;
                        break;
                }
            }

            if (serverVersion != null && buildVersion.CompareTo(serverVersion) < 0)
            {
                return new UpdateDetails() {Version = serverVersionName, DownloadUrl = serverDownloadUrl};
            }
            else
            {
                // No new version available
                return null;
            }
        }
    }
}

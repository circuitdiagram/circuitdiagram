using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CircuitDiagram
{
    static class UpdateManager
    {
        public static UpdateDetails CheckForUpdates()
        {
            // Check for new version
            System.Reflection.Assembly _assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            Version thisVersion = _assemblyInfo.GetName().Version;
            BuildChannelAttribute channelAttribute = _assemblyInfo.GetCustomAttributes(typeof(BuildChannelAttribute), false).FirstOrDefault(item => item is BuildChannelAttribute) as BuildChannelAttribute;
            if (channelAttribute == null)
                channelAttribute = new BuildChannelAttribute("", BuildChannelAttribute.ChannelType.Stable, 0);

            System.Net.WebRequest updateDocStream = System.Net.WebRequest.Create("http://www.circuit-diagram.org/app/appversion.xml");
            XmlDocument updateDoc = new XmlDocument();
            updateDoc.Load(updateDocStream.GetResponse().GetResponseStream());

            bool foundUpdate = false;
            if (channelAttribute.Type == BuildChannelAttribute.ChannelType.Dev)
            {
                // Check for latest dev build
                XmlNode devChannel = updateDoc.SelectSingleNode("/version/application[@name='CircuitDiagram']/channel[@name='Dev']");
                if (devChannel != null)
                {
                    Version serverVersion = null;
                    string serverVersionName = null;
                    int serverIncrement = 0;
                    string serverDownloadUrl = null;

                    foreach (XmlNode childNode in devChannel.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "version":
                                serverVersion = new Version(childNode.InnerText);
                                break;
                            case "name":
                                serverVersionName = childNode.InnerText;
                                break;
                            case "increment":
                                serverIncrement = int.Parse(childNode.InnerText);
                                break;
                            case "url":
                                serverDownloadUrl = childNode.InnerText;
                                break;
                        }
                    }

                    if (serverVersion != null && thisVersion.CompareTo(serverVersion) < 0 || (thisVersion.CompareTo(serverVersion) == 0 && channelAttribute.Increment < serverIncrement))
                    {
                        return new UpdateDetails() { Version = serverVersionName, DownloadUrl = serverDownloadUrl };
                    }
                }
            }

            if (!foundUpdate)
            {
                // Check for latest stable build
                XmlNode stableChannel = updateDoc.SelectSingleNode("/version/application[@name='CircuitDiagram']/channel[@name='Stable']");
                if (stableChannel != null)
                {
                    Version serverVersion = null;
                    string serverVersionName = null;
                    string serverDownloadUrl = null;

                    foreach (XmlNode childNode in stableChannel.ChildNodes)
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

                    if (serverVersion != null && thisVersion.CompareTo(serverVersion) < 0)
                    {
                        return new UpdateDetails() { Version = serverVersionName, DownloadUrl = serverDownloadUrl };
                    }
                }
            }

            // No new version available
            return null;
        }
    }
}

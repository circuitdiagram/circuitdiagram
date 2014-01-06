// Program.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Text;
using NDesk.Options;
using System.Xml;
using System.IO;
using CircuitDiagram.Components;
using CircuitDiagram.IO;
using CircuitDiagram.Components.Description;
using System.Security.Cryptography.X509Certificates;

namespace cdcompile
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = null;
            string config = null;
            bool help = false;
            var p = new OptionSet() {
                { "o|output=", v => output = v },
                { "i|input|config=", v => config = v },
   	            { "h|?|help",   v => help = v != null },
            };
            List<string> extra = p.Parse(args);

            List<ComponentDescription> componentDescriptions = new List<ComponentDescription>();
            List<BinaryResource> binaryResources = new List<BinaryResource>();

            string keyPath = null;
            if (config != null && File.Exists(config))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(config);

                XmlElement root = doc.SelectSingleNode("/cdcom") as XmlElement;
                string markupVersion = (root.HasAttribute("markupversion") ? root.Attributes["markupversion"].InnerText : null);
                string xUiApp = (root.HasAttribute("x-uiapp") ? root.Attributes["x-uiapp"].InnerText : null);
                keyPath = (root.HasAttribute("key") ? root.Attributes["key"].InnerText : null);

                XmlNodeList componentNodes = doc.SelectNodes("/cdcom/component");
                foreach (XmlNode component in componentNodes)
                {
                    string internalId = "C0";
                    if ((component as XmlElement).HasAttribute("id"))
                        internalId = component.Attributes["id"].InnerText;
                    string path = component.Attributes["path"].InnerText;

                    XmlLoader loader = new CircuitDiagram.IO.XmlLoader();
                    loader.Load(new FileStream(path, FileMode.Open));

                    ComponentDescription description = loader.GetDescriptions()[0];
                    description.ID = internalId;

                    foreach (XmlNode iconNode in component.SelectNodes("icon"))
                    {
                        if ((iconNode as XmlElement).HasAttribute("configuration"))
                        {
                            ComponentConfiguration matchedConfiguration = description.Metadata.Configurations.FirstOrDefault(configuration => configuration.Name == iconNode.Attributes["configuration"].InnerText);
                            if (matchedConfiguration != null)
                            {
                                byte[] iconData = File.ReadAllBytes(iconNode.InnerText);
                                matchedConfiguration.IconData = iconData;
                                matchedConfiguration.IconMimeType = "image/png";
                            }
                        }
                        else
                        {
                            byte[] iconData = File.ReadAllBytes(iconNode.InnerText);
                            description.Metadata.IconData = iconData;
                            description.Metadata.IconMimeType = "image/png";
                        }
                    }

                    componentDescriptions.Add(description);
                }
            }

            FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write);

            string keyString = null;
            if (keyPath != null && File.Exists(keyPath))
                keyString = File.ReadAllText(keyPath);

            X509Certificate2 certificate = null;
            if (keyString != null)
            {
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
		        store.Open(OpenFlags.OpenExistingOnly);
	
		        //Put certificates from the store into a collection so user can select one.
		        X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates;

		        X509Certificate2Collection collection = X509Certificate2UI.SelectFromCollection(fcollection, "Select an X509 Certificate", "Choose a certificate to examine.", X509SelectionFlag.SingleSelection);
                certificate = collection[0];
            }

            CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings settings = new CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings();
            settings.Certificate = certificate;
            CircuitDiagram.IO.BinaryWriter writer = new CircuitDiagram.IO.BinaryWriter(stream, settings);
            writer.Descriptions.AddRange(componentDescriptions);
            writer.Resources.AddRange(binaryResources);
            writer.Write();
            stream.Flush();
        }
    }
}

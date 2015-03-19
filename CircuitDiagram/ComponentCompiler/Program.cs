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
using System.Diagnostics;

namespace cdcompile
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = null;
            string input = null;
            List<string> iconPaths = new List<string>();
            bool sign = false;
            string certThumb = null;
            bool help = false;
            var p = new OptionSet() {
                { "i|input=", "Path to input XML component.", v => input = v },
                { "o|output=", "Path to write compiled component to.", v => output = v },
                { "icon=", "Path to PNG icon.", v => iconPaths.Add(v)},
                { "sign", "If present, presents a dialog for choosing a certificate for component signing.", v => sign = v != null },
                { "certificate=", "Thumbprint of certificate to use for signing.", v => certThumb = v},
   	            { "h|?|help", "Display help and options.",   v => help = v != null },
            };
            List<string> extra = p.Parse(args);

            List<ComponentDescription> componentDescriptions = new List<ComponentDescription>();
            List<BinaryResource> binaryResources = new List<BinaryResource>();

            if (input == null || output == null || help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            XmlLoader loader = new CircuitDiagram.IO.XmlLoader();
            loader.Load(new FileStream(input, FileMode.Open));

            ComponentDescription description = loader.GetDescriptions()[0];
            description.ID = "C0";
            componentDescriptions.Add(description);

            // First icon is default
            if (iconPaths.Count > 0)
            {
                byte[] iconData = File.ReadAllBytes(iconPaths[0]);


                description.Metadata.Icon = new MultiResolutionImage();
                description.Metadata.Icon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });

                // Icons with same base name are different resolutions
                string baseName = GetIconBaseName(iconPaths[0]);
                if (baseName != null)
                {
                    var otherResolutions = iconPaths.Where(ic => ic != iconPaths[0] && GetIconBaseName(ic) == baseName);

                    foreach(var otherResolution in otherResolutions)
                    {
                        iconData = File.ReadAllBytes(otherResolution);
                        description.Metadata.Icon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });
                    }
                }
            }

            // Map remaining icons to configurations
            for (int i = 1; i < iconPaths.Count && i < description.Metadata.Configurations.Count; i++)
            {
                ComponentConfiguration matchedConfiguration = description.Metadata.Configurations[i];
                byte[] iconData = File.ReadAllBytes(iconPaths[i]);
                matchedConfiguration.Icon = new MultiResolutionImage();
                matchedConfiguration.Icon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });
            }

            FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write);

            X509Certificate2 certificate = null;
            if (sign && certThumb == null)
            {
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
		        store.Open(OpenFlags.OpenExistingOnly);
	
		        //Put certificates from the store into a collection so user can select one.
		        X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates;

                IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
		        X509Certificate2Collection collection = X509Certificate2UI.SelectFromCollection(fcollection, "Select an X509 Certificate",
                    "Choose a certificate to sign your component with.", X509SelectionFlag.SingleSelection, handle);
                certificate = collection[0];
            }
            else if (sign)
            {
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                store.Open(OpenFlags.OpenExistingOnly);
                X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates;
                certificate = fcollection.Find(X509FindType.FindByThumbprint, certThumb, false)[0];
            }

            CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings settings = new CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings();
            settings.Certificate = certificate;
            CircuitDiagram.IO.BinaryWriter writer = new CircuitDiagram.IO.BinaryWriter(stream, settings);
            writer.Descriptions.AddRange(componentDescriptions);
            writer.Resources.AddRange(binaryResources);
            writer.Write();
            stream.Flush();
        }

        private static string GetIconBaseName(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            int pos = fileName.LastIndexOf("_");
            if (pos > 0)
                return fileName.Substring(0, pos);
            else
                return null;
        }
    }
}

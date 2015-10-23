// Program.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2015  Sam Fisher
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
using System.Text.RegularExpressions;
using CircuitDiagram.IO.Descriptions;
using CircuitDiagram.IO.Descriptions.Xml;

namespace cdcompile
{
    class Program
    {
        static int Main(string[] args)
        {
            var compileOptions = new CompileOptions();
            bool help = false;
            
            var p = new OptionSet() {
                { "i|input=", "Path to input XML component or component folder.", v => compileOptions.Input = v },
                { "o|output=", "Path to write compiled component to.", v => compileOptions.Output = v },
                { "icon=", "Path to PNG icon.", v => compileOptions.IconPaths.Add(v)},
                { "icondir=", "Find icons automatically in the specified directory.", v => compileOptions.DefaultIconPath = v},
                { "sign", "If present, presents a dialog for choosing a certificate for component signing.", v => compileOptions.Sign = v != null },
                { "certificate=", "Thumbprint of certificate to use for signing.", v => compileOptions.CertificateThumbprint = v},
   	            { "h|?|help", "Display help and options.",   v => help = v != null },
                { "r|recursive", "Recursively search sub-directories of the input directory", v => compileOptions.Recursive = v != null },
                { "v|verbose", "Prints extra information to the console.", v => compileOptions.Verbose = v != null },
                { "s|strict", "Fail if an icon cannot be found.", v => compileOptions.Strict = v != null },
                { "c|colour", "Use colours in output.", v => compileOptions.Colour = v != null }
            };
            List<string> extra = p.Parse(args);

            if (compileOptions.Input == null || compileOptions.Output == null || help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (File.Exists(compileOptions.Input))
            {
                // Compile a single component
                try
                {
                    CompileComponent(compileOptions.Input, compileOptions);
                }
                catch (IconNotFoundException ex)
                {
                    WriteLineColour(ConsoleColor.Red, compileOptions, ex.ToString());
                    return 1;
                }

                Console.WriteLine("Compiled {0}", Path.GetFileName(compileOptions.Input));
            }
            else if (Directory.Exists(compileOptions.Input))
            {
                // Compile a directory of components
                Console.WriteLine("Compiling components...");

                int failed = 0;
                string[] inputPaths = Directory.GetFiles(compileOptions.Input, "*.xml",
                    compileOptions.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (string input in inputPaths)
                {
                    try
                    {
                        if (!CompileComponent(input, compileOptions))
                            failed++;
                    }
                    catch (IconNotFoundException ex)
                    {
                        WriteLineColour(ConsoleColor.Red, compileOptions, ex.ToString());
                        return 1;
                    }
                }

                Console.WriteLine("Compiled {0} components", inputPaths.Length - failed);
            }

            return 0;
        }

        private static void WriteLineColour(ConsoleColor colour, CompileOptions compileOptions, string format, params object[] arg)
        {
            if (compileOptions.Colour)
                Console.ForegroundColor = colour;

            Console.WriteLine(format, arg);

            if (compileOptions.Colour)
                Console.ResetColor();
        }
        
        private static bool CompileComponent(string inputPath, CompileOptions compileOptions)
        {
            List<ComponentDescription> componentDescriptions = new List<ComponentDescription>();
            List<BinaryResource> binaryResources = new List<BinaryResource>();

            XmlLoader loader = new XmlLoader();
            loader.Load(new FileStream(inputPath, FileMode.Open));

            if (loader.LoadErrors.Count(e => e.Category == LoadErrorCategory.Error) > 0)
            {
                foreach (var error in loader.LoadErrors)
                    Console.WriteLine(error.ToString());
                return false;
            }

            ComponentDescription description = loader.GetDescriptions()[0];
            description.ID = "C0";
            componentDescriptions.Add(description);

            SetIcons(compileOptions, description);

            string output = compileOptions.Output;

            if (compileOptions.Recursive)
            {
                string inputSubdirectory = Path.GetDirectoryName(inputPath.Remove(0, compileOptions.Input.Length));

                if (inputSubdirectory.IndexOf("\\") == 0)
                    inputSubdirectory = inputSubdirectory.Remove(0, 1);

                if (inputSubdirectory != "\\")
                    output = Path.Combine(output, inputSubdirectory);
            }

            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            if (!Path.HasExtension(output))
            {
                output += "\\" + description.ComponentName.ToLowerInvariant() + ".cdcom";
            }

            FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write);

            X509Certificate2 certificate = SelectCertificate(compileOptions);

            CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings settings = new CircuitDiagram.IO.BinaryWriter.BinaryWriterSettings();
            settings.Certificate = certificate;
            CircuitDiagram.IO.BinaryWriter writer = new CircuitDiagram.IO.BinaryWriter(stream, settings);
            writer.Descriptions.AddRange(componentDescriptions);
            writer.Resources.AddRange(binaryResources);
            writer.Write();
            stream.Flush();

            Console.WriteLine("{0} -> {1}", Path.GetFullPath(inputPath), Path.GetFullPath(output));

            return true;
        }

        private static void SetIcons(CompileOptions compileOptions, ComponentDescription description)
        {
            if (compileOptions.DefaultIconPath != null)
            {
                // Set icon from default path
                description.Metadata.Icon = SetDefaultIcon(description.ComponentName, "", compileOptions.DefaultIconPath, compileOptions);
            }
            else
            {
                // Icon was specified manually
                // First icon is default
                if (compileOptions.IconPaths.Count > 0)
                {
                    byte[] iconData = File.ReadAllBytes(compileOptions.IconPaths[0]);

                    description.Metadata.Icon = new MultiResolutionImage();
                    description.Metadata.Icon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });

                    // Icons with same base name are different resolutions
                    string baseName = GetIconBaseName(compileOptions.IconPaths[0]);
                    if (baseName != null)
                    {
                        var otherResolutions = compileOptions.IconPaths.Where(ic => ic != compileOptions.IconPaths[0] && GetIconBaseName(ic) == baseName);

                        foreach (var otherResolution in otherResolutions)
                        {
                            iconData = File.ReadAllBytes(otherResolution);
                            description.Metadata.Icon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });
                        }
                    }
                }
            }

            // Map remaining icons to configurations
            if (compileOptions.DefaultIconPath != null)
            {
                for (int i = 1; i < description.Metadata.Configurations.Count; i++)
                {
                    var configuration = description.Metadata.Configurations[i];

                    // Set icon from default path
                    configuration.Icon = SetDefaultIcon(description.ComponentName, configuration.Name, compileOptions.DefaultIconPath, compileOptions);
                }
            }
            else
            {
                // Icon was specified manually
                for (int i = 1; i < compileOptions.IconPaths.Count && i < description.Metadata.Configurations.Count; i++)
                {
                    var configuration = description.Metadata.Configurations[i];

                    byte[] iconData = File.ReadAllBytes(compileOptions.IconPaths[i]);
                    configuration.Icon = new MultiResolutionImage();
                    configuration.Icon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });
                }
            }
        }

        private static X509Certificate2 SelectCertificate(CompileOptions compileOptions)
        {
            X509Certificate2 certificate = null;
            if (compileOptions.Sign && compileOptions.CertificateThumbprint == null)
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
            else if (compileOptions.Sign)
            {
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                store.Open(OpenFlags.OpenExistingOnly);
                X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates;
                certificate = fcollection.Find(X509FindType.FindByThumbprint, compileOptions.CertificateThumbprint, false)[0];
            }
            return certificate;
        }

        private static MultiResolutionImage SetDefaultIcon(string componentName, string configuration, string defaultIconPath, CompileOptions compileOptions)
        {
            // Construct base name
            string baseName = String.Format("{0}", FormatNameForIcon(componentName));
            if (!String.IsNullOrEmpty(configuration))
                baseName += String.Format("_{0}", FormatNameForIcon(configuration));

            // Attempt to set common DPI values: 32, 64
            int[] resolutions = new int[] { 32, 64 };
            var returnIcon = new MultiResolutionImage();
            foreach(int resolution in resolutions)
            {
                string iconFileName = String.Format("{0}_{1}.png", baseName, resolution);
                string iconPath = Path.Combine(defaultIconPath, iconFileName);

                if (!File.Exists(iconPath))
                {
                    if (compileOptions.Strict && string.IsNullOrEmpty(configuration)) // Must have a default icon
                        throw new IconNotFoundException(componentName, configuration, resolution, iconPath);
                    continue;
                }

                if (compileOptions.Verbose)
                    WriteLineColour(ConsoleColor.Cyan, compileOptions, "Icon for {0}\\{1}@{2}: {3}", componentName, configuration, resolution, iconPath);

                var iconData = File.ReadAllBytes(iconPath);
                returnIcon.Add(new SingleResolutionImage() { Data = iconData, MimeType = "image/png" });
            }

            if (returnIcon.Count > 0)
                return returnIcon;
            else
                return null;
        }

        private static string FormatNameForIcon(string componentName)
        {
            componentName = componentName.ToLowerInvariant();
            componentName = componentName.Replace(" ", "_");
            componentName = Regex.Replace(componentName, @"[\(\)-]", "");
            return componentName;
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

        private static int GetIconResolutionFromPath(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            int pos = fileName.LastIndexOf("_");
            if (pos > 0)
                return int.Parse(fileName.Substring(pos + 1));
            else
                return 0;
        }
    }
}

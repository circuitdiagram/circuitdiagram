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
using System.Diagnostics;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.Xml;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CircuitDiagram.Compiler;

namespace ComponentCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            var compileOptions = new CliCompileOptions();
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
                { "manifest=", "Write a manifest file of compiled components.", v => compileOptions.WriteManifest = v }
            };
            p.Parse(args);
            
            if (compileOptions.Input == null || compileOptions.Output == null || help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (compileOptions.Verbose)
                log4net.Config.BasicConfigurator.Configure();

            var compiledComponents = new List<ComponentCompileResult>();
            if (File.Exists(compileOptions.Input))
            {
                // Compile a single component
                var entry = CompileComponent(compileOptions.Input, compileOptions);
                compiledComponents.Add(entry);
                
                Console.WriteLine("Compiled {0}", Path.GetFileName(compileOptions.Input));
            }
            else if (Directory.Exists(compileOptions.Input))
            {
                // Compile a directory of components
                Console.WriteLine("Compiling components...");
                
                string[] inputPaths = Directory.GetFiles(compileOptions.Input, "*.xml",
                    compileOptions.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (var input in inputPaths)
                {
                    var entry = CompileComponent(input, compileOptions);
                    compiledComponents.Add(entry);
                }
                
                Console.WriteLine("Compiled {0} components", compiledComponents.Count(c => c.Success));
            }

            if (compileOptions.WriteManifest != null)
                WriteManifest(compiledComponents, compileOptions);

            return compiledComponents.All(c => c.Success) ? 0 : 1;
        }

        private static ComponentCompileResult CompileComponent(string inputPath, CliCompileOptions cliOptions)
        {
            var compiler = new CompilerService();

            var resources = new MappedDirectoryResourceProvider(cliOptions.DefaultIconPath);

            // Icon paths are specified as:
            // --icon resourceName fileName
            // --icon wire_32 wire_32.png wire_64 wire_64.png
            for (int i = 0; i < cliOptions.IconPaths.Count; i += 2)
                resources.Mappings.Add(cliOptions.IconPaths[i], cliOptions.IconPaths[i + 1]);
            
            var options = new CompileOptions()
            {
                CertificateThumbprint = cliOptions.CertificateThumbprint
            };

            if (cliOptions.Sign && cliOptions.CertificateThumbprint == null)
                options.CertificateThumbprint = SelectCertificate();
            
            string outputPath = GetOutputPath(inputPath, cliOptions);

            ComponentCompileResult result;
            using (var input = File.OpenRead(inputPath))
            using (var output = File.OpenWrite(outputPath))
            {
                result = compiler.Compile(input, output, resources, options);
            }

            if (result.Success)
                Console.WriteLine("{0} -> {1}", Path.GetFullPath(inputPath), Path.GetFullPath(outputPath));

            return result;
        }

        private static X509Certificate2 SelectCertificate()
        {
            X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
            store.Open(OpenFlags.OpenExistingOnly);

            //Put certificates from the store into a collection so user can select one.
            X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates;

            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            X509Certificate2Collection collection = X509Certificate2UI.SelectFromCollection(fcollection, "Select an X509 Certificate",
                "Choose a certificate to sign your component with.", X509SelectionFlag.SingleSelection, handle);
            return collection[0];
        }

        private static string GetOutputPath(string inputPath, CliCompileOptions cliOptions)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string outputName = Path.Combine(cliOptions.Output, fileName + ".cdcom");
            return outputName;
        }

        private static void WriteManifest(IList<ComponentCompileResult> compiledEntries, CliCompileOptions compileOptions)
        {
            using (var fs = File.OpenWrite(compileOptions.WriteManifest))
            {
                var writer = new XmlTextWriter(fs, Encoding.UTF8);

                writer.WriteStartDocument();
                writer.WriteStartElement("components");

                foreach (var entry in compiledEntries.Where(c => c.Success))
                {
                    writer.WriteStartElement("component");
                    writer.WriteAttributeString("name", entry.ComponentName);
                    writer.WriteAttributeString("author", entry.Author);
                    writer.WriteAttributeString("guid", entry.Guid.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndDocument();

                writer.Flush();
                writer.Close();
            }
        } 
    }
}

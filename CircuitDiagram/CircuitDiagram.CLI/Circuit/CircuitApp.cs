// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2020  Samuel Fisher
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
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.Document;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Skia;
using CircuitDiagram.TypeDescriptionIO.Util;
using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SkiaSharp;

namespace CircuitDiagram.CLI.Circuit
{
    static class CircuitApp
    {
        public static int Run(Options options)
        {
            Console.WriteLine($"{Path.GetFileName(options.Input)} -> {Path.GetFileName(options.Output)}");

            var reader = new CircuitDiagramDocumentReader();
            CircuitDiagramDocument circuit;
            using (var fs = File.Open(options.Input, FileMode.Open, FileAccess.Read))
            {
                circuit = reader.ReadCircuit(fs);
            }

            var loggerFactory = new LoggerFactory();

            if (options.Verbose)
            {
                loggerFactory.AddConsole(LogLevel.Information);
            }

            var descriptionLookup = new DirectoryComponentDescriptionLookup(loggerFactory, options.ComponentsDirectory ?? Path.GetDirectoryName(options.Input), true);
            var renderer = new CircuitRenderer(descriptionLookup);
            var drawingContext = new SkiaDrawingContext((int)circuit.Size.Width, (int)circuit.Size.Height, SKColors.White);

            try
            {
                renderer.RenderCircuit(circuit, drawingContext);
            }
            catch (MissingComponentDescriptionException ex)
            {
                Console.Error.WriteLine($"Unable to find component {ex.MissingType}.");

                var allDescriptions = descriptionLookup.GetAllDescriptions().OrderBy(x => x.ComponentName).ToList();

                if (!allDescriptions.Any())
                {
                    Console.Error.Write("No components were loaded. Is the --components option set correctly?");
                }
                else
                {
                    Console.Error.WriteLine("Ensure this component is available in the --components directory. The following components were loaded:");

                    foreach (var description in allDescriptions)
                    {
                        Console.Error.WriteLine($"  - {description.ComponentName} ({description.Metadata.GUID})");
                    }
                }
                
                return 1;
            }

            using (var outputFs = File.OpenWrite(options.Output))
            {
                drawingContext.WriteAsPng(outputFs);
            }

            return 0;
        }

        [Verb("circuit", HelpText = "Render a CDDX circuit as an image.")]
        public class Options
        {
            [Value(0, Required = true, HelpText = "Path to components directory.")]
            public string Input { get; set; }

            [Option('o', Required = true, HelpText = "Path to output file.")]
            public string Output { get; set; }

            [Option("components", HelpText = "Paths to components directory.")]
            public string ComponentsDirectory { get; set; }

            [Value('v')]
            public bool Verbose { get; set; }
        }
    }
}

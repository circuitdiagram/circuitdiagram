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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.CLI.Logging;
using CircuitDiagram.Document;
using CircuitDiagram.Drawing;
using CircuitDiagram.Render;
using CircuitDiagram.Render.ImageSharp;
using CircuitDiagram.Render.Skia;
using CircuitDiagram.TypeDescriptionIO.Util;
using CircuitDiagram.TypeDescriptionIO.Xml;
using CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace CircuitDiagram.CLI.Circuit
{
    class CircuitApp
    {
        private readonly ILogger<CircuitApp> _logger;
        private readonly DirectoryComponentDescriptionLookup _componentDescriptionLookup;

        public CircuitApp(
            ILogger<CircuitApp> logger,
            DirectoryComponentDescriptionLookup componentDescriptionLookup)
        {
            _logger = logger;
            _componentDescriptionLookup = componentDescriptionLookup;
        }

        public int Run(Options options)
        {
            _logger.LogInformation($"{Path.GetFileName(options.Input)} -> {Path.GetFileName(options.Output)}");

            CircuitDocument circuit;
            var inputFileExtension = Path.GetExtension(options.Input);
            switch (inputFileExtension)
            {
                case ".cddx":
                {
                    var reader = new CircuitDiagramDocumentReader();
                    using (var fs = File.Open(options.Input, FileMode.Open, FileAccess.Read))
                    {
                        circuit = reader.ReadCircuit(fs);
                    }
                    break;
                }
                default:
                {
                    _logger.LogError($"Unknown file type: '{inputFileExtension}'.");
                    return 1;
                }
            }

            var renderer = new CircuitRenderer(_componentDescriptionLookup);

            var bufferRenderer = new SkiaBufferedDrawingContext();
            renderer.RenderCircuit(circuit, bufferRenderer);

            IPngDrawingContext context;
            switch (options.Renderer)
            {
                case PngRenderer.Skia:
                    context = new SkiaDrawingContext((int)circuit.Size.Width, (int)circuit.Size.Height, SKColors.White);
                    break;
                case PngRenderer.ImageSharp:
                    context = new ImageSharpDrawingContext((int)circuit.Size.Width, (int)circuit.Size.Height, SixLabors.ImageSharp.Color.White);
                    break;
                default:
                    _logger.LogError("Unsupported renderer.");
                    return 1;
            }

            try
            {
                renderer.RenderCircuit(circuit, context);
            }
            catch (MissingComponentDescriptionException ex)
            {
                _logger.LogError($"Unable to find component {ex.MissingType}.");

                var allDescriptions = _componentDescriptionLookup.GetAllDescriptions().OrderBy(x => x.ComponentName).ToList();

                if (!allDescriptions.Any())
                {
                    _logger.LogInformation("No components were loaded. Is the --components option set correctly?");
                }
                else
                {
                    _logger.LogInformation("Ensure this component is available in the --components directory. The following components were loaded:");

                    foreach (var description in allDescriptions)
                    {
                        _logger.LogInformation($"  - {description.ComponentName} ({description.Metadata.GUID})");
                    }
                }

                return 1;
            }

            using (var outputFs = File.OpenWrite(options.Output))
            {
                context.WriteAsPng(outputFs);
            }

            return 0;
        }

        public static int Run(IHost host, Options options)
        {
            var services = new ServiceCollection();
            services.AddLogging(x => x.SetupLogging(options.Verbose, options.Silent));

            var loader = new XmlLoader();
            loader.UseDefinitions();
            services.AddSingleton(s => new DirectoryComponentDescriptionLookup(s.GetRequiredService<ILoggerFactory>(), options.Components ?? Path.GetDirectoryName(options.Input), true, loader));

            var serviceProvider = services.BuildServiceProvider();

            var app = ActivatorUtilities.CreateInstance<CircuitApp>(serviceProvider);
            return app.Run(options);
        }

        public static Command BuildCommand()
        {
            var command = new Command("circuit", "Render a circuit document as an image.");

            var input = new Argument("input");
            input.Description = "Path to circuit document to render (*.cddx).";
            input.Arity = ArgumentArity.ExactlyOne;
            command.AddArgument(input);

            var output = new Option<string>(new[] { "-o", "--output" }, "Path to output file.");
            output.IsRequired = true;
            output.Argument.Name = "path";
            command.AddOption(output);

            var components = new Option<string>("--components", "Path to components directory.");
            command.AddOption(components);

            var renderer = new Option<PngRenderer>("--renderer", () => PngRenderer.Skia, "Renderer to use for PNG outputs.");
            command.AddOption(renderer);

            var debugLayout = new Option<bool>(new[] { "-d", "--debug-layout" }, "Draw component start and end points.");
            command.AddOption(debugLayout);

            var grid = new Option<bool>("--grid", "Draw a background grid.");
            command.AddOption(grid);

            var scale = new Option<double>("--scale", () => 1.0, "Scale the output image.");
            command.AddOption(scale);

            command.Handler = CommandHandler.Create<IHost, Options>(Run);
            return command;
        }

        public class Options
        {
            public string Input { get; set; }

            public string Output { get; set; }

            public string Components { get; set; }

            public PngRenderer Renderer { get; set; }

            public bool Verbose { get; set; }

            public bool Silent { get; set; }
        }
    }
}

// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2019  Samuel Fisher
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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.CLI.Component.OutputGenerators;
using Microsoft.Extensions.Logging;
using CircuitDiagram.Logging;
using CircuitDiagram.CLI.Component.InputRunners;
using CircuitDiagram.CLI.Component.Manifest;
using CircuitDiagram.TypeDescriptionIO.Util;
using CommandLine;
using Microsoft.Extensions.Logging.Abstractions;

namespace CircuitDiagram.CLI.Component
{
    static class ComponentApp
    {
        public static int Run(Options options)
        {
            var loggerFactory = new LoggerFactory();
            if (!options.Silent)
            {
                loggerFactory.AddProvider(new BasicConsoleLogger(options.Verbose ? LogLevel.Debug : LogLevel.Information));
            }

            var logger = loggerFactory.CreateLogger(typeof(ComponentApp));

            if (!options.Input.Any())
            {
                logger.LogError("At least one input file must be specified.");
                return 1;
            }

            var generators = new Dictionary<IOutputGenerator, string>();
            var outputGenerators = new OutputGeneratorRepository(options.Renderer);
            bool outputIsDirectory = Directory.Exists(options.Output);
            if (options.Output != null && !outputIsDirectory)
            {
                if (outputGenerators.TryGetGeneratorByFileExtension(Path.GetExtension(options.Output), out var generator))
                {
                    // Use the generator implied by the file extension
                    generators.Add(generator, options.Output);
                }
                else if (options.Formats?.Any() != true)
                {
                    logger.LogError("Unable to infer format from output file extension." + Environment.NewLine +
                                    "Specify a known file extension or specify explicitly using --format");
                    return 1;
                }
                else
                {
                    logger.LogInformation("Unable to infer format from output file extension. Using formats only.");
                }
            }

            if (options.Formats?.Any() == true)
            {
                foreach (var format in options.Formats)
                {
                    if (outputGenerators.TryGetGeneratorByFormat(format, out var generator))
                    {
                        generators.Add(generator, outputIsDirectory ? options.Output : null);
                    }
                    else
                    {
                        logger.LogError($"Unknown format: {format}");
                        return 1;
                    }
                }
            }

            var previewOptions = new PreviewGenerationOptions
            {
                Center = true,
                Crop = options.Autosize,
                Width = options.Width,
                Height = options.Height,
                Configuration = options.Configuration,
                DebugLayout = options.DebugLayout,
                Grid = options.Grid,
                Scale = options.Scale,
                Properties = new Dictionary<string, string>(),
            };

            if (options.RenderPropertiesPath != null && File.Exists(options.RenderPropertiesPath))
            {
                logger.LogDebug($"Applying render properties from '{options.RenderPropertiesPath}'");
                PreviewGenerationOptionsReader.Read(options.RenderPropertiesPath, previewOptions);
            }

            DirectoryComponentDescriptionLookup componentDescriptionLookup;
            var descriptionLookupLoggerFactory = options.Verbose ? loggerFactory : (ILoggerFactory)NullLoggerFactory.Instance;
            if (options.ComponentsDirectory != null)
            {
                componentDescriptionLookup = new DirectoryComponentDescriptionLookup(descriptionLookupLoggerFactory, options.ComponentsDirectory, true);
            }
            else if (Directory.Exists(options.Input))
            {
                componentDescriptionLookup = new DirectoryComponentDescriptionLookup(descriptionLookupLoggerFactory, options.Input, true);
            }
            else
            {
                componentDescriptionLookup = new DirectoryComponentDescriptionLookup(descriptionLookupLoggerFactory, Environment.CurrentDirectory, true);
            }

            var resourceProvider = ResourceProviderFactory.Create(loggerFactory.CreateLogger(typeof(ResourceProviderFactory)), options.Resources?.ToArray() ?? new string[0]);
            var outputRunner = new OutputRunner(loggerFactory.CreateLogger<OutputRunner>(), resourceProvider);
            var compileRunner = new ComponentDescriptionRunner(loggerFactory.CreateLogger<ComponentDescriptionRunner>(), outputRunner);
            var configurationDefinitionRunner = new ConfigurationDefinitionRunner(loggerFactory.CreateLogger<ConfigurationDefinitionRunner>(), componentDescriptionLookup, outputRunner);
            var results = new List<IManifestEntry>();

            var inputs = new List<string>();
            if (File.Exists(options.Input))
            {
                logger.LogDebug("Input is a file.");
                inputs.Add(options.Input);
            }
            else if (Directory.Exists(options.Input))
            {
                logger.LogDebug("Input is a directory.");
                foreach (var generator in generators)
                {
                    if (generator.Value != null && !Directory.Exists(generator.Value))
                    {
                        logger.LogError("Outputs must be directories when the input is a directory.");
                        return 1;
                    }
                }

                foreach (var file in Directory.GetFiles(options.Input, "*.xml",
                                                        options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    inputs.Add(file);
                }

                foreach (var file in Directory.GetFiles(options.Input, "*.yaml",
                                                        options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    inputs.Add(file);
                }
            }
            else
            {
                logger.LogError($"Input is not a valid file or directory: {options.Input}");
                return 1;
            }

            foreach (var file in inputs.OrderBy(x => x))
            {
                logger.LogDebug($"Starting '{file}'");

                IManifestEntry result;

                switch (Path.GetExtension(file))
                {
                    case ".xml":
                        result = compileRunner.CompileOne(file, previewOptions, options.AllConfigurations, generators);
                        break;
                    case ".yaml":
                        result = configurationDefinitionRunner.CompileOne(file, previewOptions, generators);
                        break;
                    default:
                        throw new NotSupportedException($"File type '{Path.GetExtension(file)}' not supported.");
                }

                if (result == null)
                {
                    return 2;
                }

                results.Add(result);

                logger.LogDebug($"Finshed '{file}'");
            }

            if (options.Manifest != null)
            {
                using (var manifestFs = File.Open(options.Manifest, FileMode.Create))
                {
                    logger.LogInformation($"Writing manifest to {options.Manifest}");
                    ManifestWriter.WriteManifest(results, manifestFs);
                }
            }

            return 0;
        }

        [Verb("component", HelpText = "Compile and render components.")]
        public class Options
        {
            [Value(0, Required = true, HelpText = "Path to component or directory containing components to compile.")]
            public string Input { get; set; }

            [Option('o', HelpText = "Path to output file (the format will be inferred from the extension). Cannot be used for directory inputs or in combination with specific output format options.")]
            public string Output { get; set; }

            [Option("manifest", HelpText = "Writes a manifest file listing the compiled components.")]
            public string Manifest { get; set; }

            [Option("autosize", HelpText = "Automatically size the output image to fit the rendered preview.")]
            public bool Autosize { get; set; }

            [Option('w', "width", Default = 640 , HelpText = "Width of output images to generate.")]
            public int Width { get; set; }

            [Option('h', "height", Default = 480, HelpText = "Height of output images to generate.")]
            public int Height { get; set; }

            [Option('r', "recursive", HelpText = "Recursively searches sub-directories of the input directory.")]
            public bool Recursive { get; set; }

            [Option('v')]
            public bool Verbose { get; set; }

            [Option('s', "silent", HelpText = "Does not output anything to the console.")]
            public bool Silent { get; set; }

            [Option('c', "configuration", HelpText = "Name of component configuration to use (supported output formats only).")]
            public string Configuration { get; set; }

            [Option("all-configurations", HelpText = "Produce an output for every component configuration (supported output formats only).")]
            public bool AllConfigurations { get; set; }

            [Option("components", HelpText = "Paths to components directory.")]
            public string ComponentsDirectory { get; set; }

            [Option("format", HelpText = "Output formats to write.")]
            public IEnumerable<string> Formats { get; set; }

            [Option("resources", HelpText = "Resources to use in generating the output.")]
            public IEnumerable<string> Resources { get; set; }

            [Option("renderer", Default = PngRenderer.Skia, HelpText = "Renderer to use for PNG outputs (Skia or ImageSharp).")]
            public PngRenderer Renderer { get; set; }

            [Option('p', "props", HelpText = "Path to render.properties file for preview generation.")]
            public string RenderPropertiesPath { get; set; }

            [Option('d', "debug-layout", HelpText = "Draw component start and end points.")]
            public bool DebugLayout { get; set; }

            [Option("grid", HelpText = "Draw a background grid.")]
            public bool Grid { get; set; }

            [Option("scale", HelpText = "Scale the output image.", Default = 1.0)]
            public double Scale { get; set; }
        }
    }
}

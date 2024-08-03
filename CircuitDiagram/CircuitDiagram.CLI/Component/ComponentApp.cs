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
using System.IO;
using System.Linq;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.CLI.Component.OutputGenerators;
using Microsoft.Extensions.Logging;
using CircuitDiagram.CLI.Component.InputRunners;
using CircuitDiagram.CLI.Component.Manifest;
using CircuitDiagram.TypeDescriptionIO.Util;
using CircuitDiagram.Render;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using CircuitDiagram.Compiler;
using CircuitDiagram.CLI.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions;

namespace CircuitDiagram.CLI.Component
{
    class ComponentApp
    {
        private readonly ILogger<ComponentApp> _logger;
        private readonly OutputGeneratorRepository _outputGeneratorRepository;
        private readonly ComponentDescriptionRunner _compileRunner;
        private readonly ConfigurationDefinitionRunner _configurationDefinitionRunner;

        public ComponentApp(
            ILogger<ComponentApp> logger,
            OutputGeneratorRepository outputGeneratorRepository,
            ComponentDescriptionRunner compileRunner,
            ConfigurationDefinitionRunner configurationDefinitionRunner)
        {
            _logger = logger;
            _outputGeneratorRepository = outputGeneratorRepository;
            _compileRunner = compileRunner;
            _configurationDefinitionRunner = configurationDefinitionRunner;
        }

        public int Run(Options options)
        {
            if (!options.Input.Any())
            {
                _logger.LogError("At least one input file must be specified.");
                return 1;
            }

            var generators = new Dictionary<IOutputGenerator, string>();
            bool outputIsDirectory = Directory.Exists(options.Output);
            if (options.Output != null && !outputIsDirectory)
            {
                if (_outputGeneratorRepository.TryGetGeneratorByFileExtension(Path.GetExtension(options.Output), out var generator))
                {
                    // Use the generator implied by the file extension
                    generators.Add(generator, options.Output);
                }
                else if (options.Format?.Any() != true)
                {
                    _logger.LogError("Unable to infer format from output file extension." + Environment.NewLine +
                                    "Specify a known file extension or specify explicitly using --format");
                    return 1;
                }
                else
                {
                    _logger.LogInformation("Unable to infer format from output file extension. Using formats only.");
                }
            }

            if (options.Format?.Any() == true)
            {
                foreach (var format in options.Format)
                {
                    if (_outputGeneratorRepository.TryGetGeneratorByFormat(format, out var generator))
                    {
                        generators.Add(generator, outputIsDirectory ? options.Output : null);
                    }
                    else
                    {
                        _logger.LogError($"Unknown format: {format}");
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
                FontFamily = options.FontFamily,
                FontSizeScale = options.FontSizeScale,
            };

            if (options.Props != null && File.Exists(options.Props))
            {
                _logger.LogDebug($"Applying render properties from '{options.Props}'");
                PreviewGenerationOptionsReader.Read(options.Props, previewOptions);
            }

            var results = new List<IManifestEntry>();

            var inputs = new List<string>();
            if (File.Exists(options.Input))
            {
                _logger.LogDebug("Input is a file.");
                inputs.Add(options.Input);
            }
            else if (Directory.Exists(options.Input))
            {
                _logger.LogDebug("Input is a directory.");
                foreach (var generator in generators)
                {
                    if (generator.Value != null && !Directory.Exists(generator.Value))
                    {
                        _logger.LogError("Outputs must be directories when the input is a directory.");
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
                _logger.LogError($"Input is not a valid file or directory: {options.Input}");
                return 1;
            }

            foreach (var file in inputs.OrderBy(x => x))
            {
                _logger.LogDebug($"Starting '{file}'");

                IManifestEntry result;

                switch (Path.GetExtension(file))
                {
                    case ".xml":
                        result = _compileRunner.CompileOne(file, previewOptions, options.AllConfigurations, generators);
                        break;
                    case ".yaml":
                        result = _configurationDefinitionRunner.CompileOne(file, previewOptions, generators);
                        break;
                    default:
                        throw new NotSupportedException($"File type '{Path.GetExtension(file)}' not supported.");
                }

                if (result == null)
                {
                    return 2;
                }

                results.Add(result);

                _logger.LogDebug($"Finshed '{file}'");
            }

            if (options.Manifest != null)
            {
                using (var manifestFs = File.Open(options.Manifest, FileMode.Create))
                {
                    _logger.LogInformation($"Writing manifest to {options.Manifest}");
                    ManifestWriter.WriteManifest(results, manifestFs);
                }
            }

            return 0;
        }

        public static int Run(IHost host, Options options)
        {
            var services = new ServiceCollection();
            services.AddLogging(x => x.SetupLogging(options.Verbose, options.Silent));
            services.AddSingleton(s => ActivatorUtilities.CreateInstance<OutputGeneratorRepository>(s, options.Renderer));

            if (options.Components != null)
            {
                var xmlLoader = new TypeDescriptionIO.Xml.XmlLoader();
                xmlLoader.UseDefinitions();

                services.AddSingleton<IComponentDescriptionLookup>(s =>
                    new DirectoryComponentDescriptionLookup(s.GetRequiredService<ILoggerFactory>(), options.Components, options.Recursive, xmlLoader));
            }
            else
            {
                services.AddSingleton<IComponentDescriptionLookup, DictionaryComponentDescriptionLookup>();
            }

            services.AddSingleton(s =>
                ResourceProviderFactory.Create(s.GetRequiredService<ILogger<ResourceProviderFactory>>(), options.Resources));
            services.AddSingleton<OutputRunner>();
            services.AddSingleton<ComponentDescriptionRunner>();
            services.AddSingleton<ConfigurationDefinitionRunner>();

            var serviceProvider = services.BuildServiceProvider();

            var app = ActivatorUtilities.CreateInstance<ComponentApp>(serviceProvider);
            return app.Run(options);
        }

        public static Command BuildCommand()
        {
            var command = new Command("component", "Compile and render components.");

            var input = new Argument("input");
            input.Description = "Path to component or directory containing components to compile.";
            input.Arity = ArgumentArity.ExactlyOne;
            command.AddArgument(input);

            var output = new Option<string>(new[] { "-o", "--output" }, "Path to output file (the format will be inferred from the extension). Cannot be used for directory inputs or in combination with specific output format options.");
            output.IsRequired = true;
            output.Argument.Name = "path";
            command.AddOption(output);

            var manifest = new Option<string>("--manifest", "Writes a manifest file listing the compiled components.");
            manifest.Argument.Name = "path";
            command.AddOption(manifest);

            var autosize = new Option<bool>("--autosize", "Automatically size the output image to fit the rendered preview.");
            command.AddOption(autosize);

            var width = new Option<int>(new[] { "--width" }, () => 640, "Width of output images to generate.");
            command.AddOption(width);

            var height = new Option<int>(new[] { "--height" }, () => 480, "Height of output images to generate.");
            command.AddOption(height);

            var recursive = new Option<bool>(new[] { "-r", "--recursive" }, "Recursively searches sub-directories of the input directory.");
            command.AddOption(recursive);

            var configuration = new Option<string>(new[] { "-c", "--configuration" }, "Name of component configuration to use (supported output formats only).");
            command.AddOption(configuration);

            var allConfigurations = new Option<bool>("--all-configurations", "Produce an output for every component configuration (supported output formats only).");
            command.AddOption(allConfigurations);

            var components = new Option<string>("--components", "Path to components directory.");
            command.AddOption(components);

            var format = new Option<string[]>("--format", "Output formats to write.");
            command.AddOption(format);

            var resources = new Option<string[]>("--resources", "Resources to use in generating the output.");
            command.AddOption(resources);

            var renderer = new Option<PngRenderer>("--renderer", () => PngRenderer.Skia, "Renderer to use for PNG outputs.");
            command.AddOption(renderer);

            var renderProperties = new Option<string>(new[] { "-p", "--props" }, "Path to render.properties file for preview generation.");
            command.AddOption(renderProperties);

            var debugLayout = new Option<bool>(new[] { "-d", "--debug-layout" }, "Draw component start and end points.");
            command.AddOption(debugLayout);

            var grid = new Option<bool>("--grid", "Draw a background grid.");
            command.AddOption(grid);

            var scale = new Option<double>("--scale", () => 1.0, "Scale the output image.");
            command.AddOption(scale);

            var fontFamily = new Option<string>(new[] { "--font-family" }, "Font family name to use for PNG outputs.");
            command.AddOption(fontFamily);

            var fontSizeScale = new Option<float>("--font-size-scale", () => 1.0f, "Scale the font size.");
            command.AddOption(fontSizeScale);

            command.Handler = CommandHandler.Create<IHost, Options>(Run);
            return command;
        }

        public class Options
        {
            public string Input { get; set; }

            public string Output { get; set; }

            public string Manifest { get; set; }

            public bool Autosize { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public bool Recursive { get; set; }

            public bool Verbose { get; set; }

            public bool Silent { get; set; }

            public string Configuration { get; set; }

            public bool AllConfigurations { get; set; }

            public string Components { get; set; }

            public string[] Format { get; set; }

            public string[] Resources { get; set; }

            public PngRenderer Renderer { get; set; }

            public string Props { get; set; }

            public bool DebugLayout { get; set; }

            public bool Grid { get; set; }

            public double Scale { get; set; }

            public string FontFamily { get; set; }

            public float FontSizeScale { get; set; }
        }
    }
}

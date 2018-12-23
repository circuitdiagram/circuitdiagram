// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.CommandLine;
using CircuitDiagram.Logging;
using CircuitDiagram.CLI.Component.InputRunners;
using CircuitDiagram.CLI.Component.Manifest;
using CircuitDiagram.TypeDescriptionIO.Util;

namespace CircuitDiagram.CLI.Component
{
    static class ComponentApp
    {
        public static void Run(string[] args)
        {
            IReadOnlyList<string> input = Array.Empty<string>();
            IReadOnlyList<string> resources = null;
            IReadOnlyList<string> additionalFormats = null;
            string output = null;
            string manifest = null;

            // Preview options
            bool autosize = false;
            double imgWidth = 640;
            double imgHeight = 480;
            string configuration = null;

            bool recursive = false;
            bool silent = false;
            bool verbose = false;

            var cliOptions = ArgumentSyntax.Parse(args, options =>
            {
                options.ApplicationName = "cdcli component";

                options.DefineOption("o|output", ref output,
                                     "Output file (the format will be inferred from the extension). Cannot be used for directory inputs or in combination with specific output format options.");
                var manifestOption = options.DefineOption("manifest", ref manifest, false, "Writes a manifest file listing the compiled components.");
                if (manifestOption.IsSpecified && manifest == null)
                    manifest = "manifest.xml";
                options.DefineOption("autosize", ref autosize, "Automatically sizes the output image to fit the rendered preview.");
                options.DefineOption("w|width", ref imgWidth, double.Parse, "Width of output images to generate (default=640).");
                options.DefineOption("h|height", ref imgHeight, double.Parse, "Height of output images to generate (default=480).");
                options.DefineOption("r|recursive", ref recursive, "Recursively searches sub-directories of the input directory.");
                options.DefineOption("s|silent", ref silent, "Does not output anything to the console on successful operation.");
                options.DefineOption("v|verbose", ref verbose, "Outputs extra information to the console.");
                options.DefineOption("c|configuration", ref configuration, "Name of component configuration to use.");
                options.DefineOptionList("resources", ref resources, "Resources to use in generating the output. Either a directory, or a space-separated list of [key] [filename] pairs.");
                options.DefineOptionList("format", ref additionalFormats, "Output formats to write.");
                options.DefineParameterList("input", ref input, "Components to compile.");
            });

            var loggerFactory = new LoggerFactory();
            if (!silent)
                loggerFactory.AddProvider(new BasicConsoleLogger(LogLevel.Information));

            var logger = loggerFactory.CreateLogger(typeof(ComponentApp));

            if (!input.Any())
            {
                logger.LogError("At least one input file must be specified.");
                Environment.Exit(1);
            }

            var generators = new Dictionary<IOutputGenerator, string>();
            var outputGenerators = new OutputGeneratorRepository();
            bool outputIsDirectory = Directory.Exists(output);
            if (output != null && !outputIsDirectory)
            {
                if (outputGenerators.TryGetGeneratorByFileExtension(Path.GetExtension(output), out var generator))
                {
                    // Use the generator implied by the file extension
                    generators.Add(generator, output);
                }
                else if (additionalFormats?.Any() != true)
                {
                    logger.LogError("Unable to infer format from output file extension." + Environment.NewLine +
                                    "Specify a known file extension or specify explicitly using --format");
                    Environment.Exit(1);
                }
                else
                {
                    logger.LogInformation("Unable to infer format from output file extension. Using formats only.");
                }
            }

            if (additionalFormats?.Any() == true)
            {
                foreach (var format in additionalFormats)
                {
                    if (outputGenerators.TryGetGeneratorByFormat(format, out var generator))
                    {
                        generators.Add(generator, outputIsDirectory ? output : null);
                    }
                    else
                    {
                        logger.LogError($"Unknown format: {format}");
                        Environment.Exit(1);
                    }
                }
            }

            var previewOptions = new PreviewGenerationOptions
            {
                Center = true,
                Crop = autosize,
                Width = imgWidth,
                Height = imgHeight,
                Configuration = configuration,
                Properties = new Dictionary<string, string>(),
            };

            DirectoryComponentDescriptionLookup componentDescriptionLookup;
            if (input.Count == 1 && Directory.Exists(input.Single()))
                componentDescriptionLookup = new DirectoryComponentDescriptionLookup(input.Single(), true);
            else
                componentDescriptionLookup = new DirectoryComponentDescriptionLookup(input.ToArray());
            
            var resourceProvider = ResourceProviderFactory.Create(loggerFactory.CreateLogger(typeof(ResourceProviderFactory)), resources);
            var outputRunner = new OutputRunner(loggerFactory.CreateLogger<OutputRunner>(), resourceProvider);
            var compileRunner = new ComponentDescriptionRunner(loggerFactory.CreateLogger<ComponentDescriptionRunner>(), outputRunner);
            var configurationDefinitionRunner = new ConfigurationDefinitionRunner(loggerFactory.CreateLogger<ConfigurationDefinitionRunner>(), componentDescriptionLookup, outputRunner);
            var results = new List<IManifestEntry>();

            var inputs = new List<string>();
            foreach (var i in input)
            {
                if (File.Exists(i))
                {
                    inputs.Add(i);
                }
                else if (Directory.Exists(i))
                {
                    foreach (var generator in generators)
                    {
                        if (generator.Value != null && !Directory.Exists(generator.Value))
                        {
                            logger.LogError("Outputs must be directories when the input is a directory.");
                            Environment.Exit(1);
                        }
                    }

                    foreach (var file in Directory.GetFiles(i, "*.xml",
                                                            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        inputs.Add(file);
                    }

                    foreach (var file in Directory.GetFiles(i, "*.yaml",
                                                            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        inputs.Add(file);
                    }
                }
                else
                {
                    logger.LogError($"Input is not a valid file or directory: {i}");
                    Environment.Exit(1);
                }
            }

            foreach (var file in inputs.OrderBy(x => x))
            {
                IManifestEntry result;

                switch (Path.GetExtension(file))
                {
                    case ".xml":
                        result = compileRunner.CompileOne(file, previewOptions, generators);
                        break;
                    case ".yaml":
                        result = configurationDefinitionRunner.CompileOne(file, previewOptions, generators);
                        break;
                    default:
                        throw new NotSupportedException($"File type '{Path.GetExtension(file)}' not supported.");
                }

                results.Add(result);
            }

            if (manifest != null)
            {
                using (var manifestFs = File.Open(manifest, FileMode.Create))
                {
                    logger.LogInformation($"Writing manifest to {manifest}");
                    ManifestWriter.WriteManifest(results, manifestFs);
                }
            }
        }
    }
}

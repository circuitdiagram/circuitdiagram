// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using CircuitDiagram.Compiler;
using CircuitDiagram.IO;
using CircuitDiagram.IO.Descriptions.Xml;
using CircuitDiagram.Logging;
using CircuitDiagram.TypeDescription;
using ComponentCompiler.ComponentPreview;
using ComponentCompiler.OutputGenerators;
using Microsoft.Extensions.Logging;

namespace ComponentCompiler
{
    class Program
    {
        private static readonly ILogger Log = LogManager.GetLogger<Program>();
        
        static void Main(string[] args)
        {
            IReadOnlyList<string> input = Array.Empty<string>();
            IReadOnlyList<string> resources = null;
            IReadOnlyList<string> additionalFormats = null;
            string output = null;
            string cdcom = null;
            string svg = null;
            string png = null;
            string manifest = null;

            // Preview options
            bool autosize = false;
            double imgWidth = 640;
            double imgHeight = 480;

            bool recursive = false;
            bool silent = false;
            bool verbose = false;
            bool version = false;
#if NET461
            bool listGenerators = false;
#endif

            var cliOptions = ArgumentSyntax.Parse(args, options =>
            {
                options.ApplicationName = "cdcompile";

                options.DefineOption("o|output", ref output,
                                     "Output file (the format will be inferred from the extension). Cannot be used for directory inputs or in combination with specific output format options.");

                var cdcomOption = options.DefineOption("cdcom", ref cdcom, false, "Output a compiled binary component.");
                if (cdcomOption.IsSpecified && cdcom == null)
                    cdcom = string.Empty;

                var svgOption = options.DefineOption("svg", ref svg, false, "Render preview in SVG format.");
                if (svgOption.IsSpecified && svg == null)
                    svg = string.Empty;

                var pngOption = options.DefineOption("png", ref png, false, "Render preview in PNG format (experimental).");
                if (pngOption.IsSpecified && png == null)
                    png = string.Empty;

                var manifestOption = options.DefineOption("manifest", ref manifest, false, "Writes a manifest file listing the compiled components.");
                if (manifestOption.IsSpecified && manifest == null)
                    manifest = "manifest.xml";

                options.DefineOption("autosize", ref autosize, "Automatically sizes the output image to fit the rendered preview.");
                options.DefineOption("w|width", ref imgWidth, double.Parse, "Width of output images to generate (default=640).");
                options.DefineOption("h|height", ref imgHeight, double.Parse, "Height of output images to generate (default=480).");

                options.DefineOption("r|recursive", ref recursive, "Recursively searches sub-directories of the input directory.");
                options.DefineOption("s|silent", ref silent, "Does not output anything to the console on successful operation.");
                options.DefineOption("v|verbose", ref verbose, "Outputs extra information to the console.");

                options.DefineOption("version", ref version, "Prints the version of this application.");

#if NET461
                options.DefineOption("list-generators", ref listGenerators, "List the available output generators.");
#endif

                options.DefineOptionList("resources", ref resources, "Resources to use in generating the output. Either a directory, or a space-separated list of [key] [filename] pairs.");

                options.DefineOptionList("format", ref additionalFormats, "Output formats to write.");

                options.DefineParameterList("input", ref input, "Components to compile.");
            });
            
            if (version)
            {
                var assemblyName = typeof(Program).GetTypeInfo().Assembly.GetName();
                Console.WriteLine($"cdcompile {assemblyName.Version} ({assemblyName.ProcessorArchitecture})");
                return;
            }

            if (!silent)
                LogManager.LoggerFactory.AddProvider(new BasicConsoleLogger(verbose ? LogLevel.Debug : LogLevel.Information));

#if NET461
            if (listGenerators)
            {
                foreach (var generator in new OutputGeneratorRepository().AllGenerators)
                {
                    Console.WriteLine($"{generator.Format}: {generator.FileExtension}");
                }
                return;
            }
#endif

            if (!input.Any())
                cliOptions.ReportError("At least one input file must be specified.");

            if (output != null && (svg != null || png != null))
                cliOptions.ReportError("Supplying both --output and a specific format is not supported.");

            IResourceProvider resourceProvider = null;
            if (resources != null && resources.Count == 1)
            {
                string directory = resources.Single();
                if (!Directory.Exists(directory))
                    cliOptions.ReportError($"Directory '{directory}' used for --resources does not exist.");

                Log.LogDebug($"Using directory '{directory}' as resource provider.");
                resourceProvider = new DirectoryResourceProvider(resources.Single());
            }
            else if (resources != null && resources.Count % 2 == 0)
            {
                Log.LogDebug("Mapping resources as key-file pairs.");
                resourceProvider = new FileMapResourceProvider();
                for (int i = 0; i + 1 < resources.Count; i += 2)
                    ((FileMapResourceProvider)resourceProvider).Mappings.Add(resources[i], resources[i + 1]);
            }
            else if (resources != null)
            {
                cliOptions.ReportError("--resources must either be a directory or a space-separated list of [key] [filename] pairs.");
            }
            else
            {
                Log.LogDebug("Not supplying resources.");
                resourceProvider = new FileMapResourceProvider();
            }

            var formats = new Dictionary<IOutputGenerator, string>();
            var outputGenerators = new OutputGeneratorRepository();
            bool outputIsDirectory = Directory.Exists(output);
            if (output != null && !outputIsDirectory)
            {
                if (outputGenerators.TryGetGeneratorByFileExtension(Path.GetExtension(output), out var generator))
                {
                    // Use the generator implied by the file extension
                    formats.Add(generator, output);
                }
                else
                {
                    Log.LogError("Unable to infer format from output file extension.");
                    Environment.Exit(1);
                }
            }
            if (cdcom != null)
                formats.Add(new BinaryComponentGenerator(), NullIfEmpty(cdcom));
            if (svg != null)
                formats.Add(new SvgPreviewRenderer(), NullIfEmpty(svg));
            if (png != null)
                formats.Add(new PngPreviewRenderer(), NullIfEmpty(png));
            if (additionalFormats != null)
            {
                foreach (var format in additionalFormats)
                {
                    IOutputGenerator generator;
                    if (outputGenerators.TryGetGeneratorByFormat(format, out generator))
                    {
                        formats.Add(generator, outputIsDirectory ? output : null);
                    }
                    else
                    {
                        Log.LogError($"Unknown format: {format}");
                        Environment.Exit(1);
                    }
                }
            }

            var previewOptions = new PreviewGenerationOptions
            {
                Center = true,
                Crop = autosize,
                Width = imgWidth,
                Height = imgHeight
            };

            var results = new List<CompileResult>();

            foreach (var i in input)
            {
                if (File.Exists(i))
                {
                    var result = Run(i, resourceProvider, previewOptions, formats);
                    results.Add(result);
                }
                else if (Directory.Exists(i))
                {
                    foreach (var generator in formats)
                    {
                        if (generator.Value != null && !Directory.Exists(generator.Value))
                            cliOptions.ReportError("Outputs must be directories when the input is a directory.");
                    }

                    foreach (var file in Directory.GetFiles(i, "*.xml",
                                                            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        var result = Run(file, resourceProvider, previewOptions, formats);
                        results.Add(result);
                    }
                }
                else
                {
                    Log.LogError($"Input is not a valid file or directory: {i}");
                    Environment.Exit(1);
                }
            }

            if (manifest != null)
            {
                using (var manifestFs = File.Open(manifest, FileMode.Create))
                {
                    Log.LogInformation($"Writing manifest to {manifest}");
                    ManifestGenerator.WriteManifest(results, manifestFs);
                }
            }
        }

        static CompileResult Run(string inputFile, 
            IResourceProvider resourceProvider,
            PreviewGenerationOptions previewOptions,
            IDictionary<IOutputGenerator, string> formats)
        {
            Log.LogInformation(inputFile);

            var loader = new XmlLoader();
            using (var fs = File.OpenRead(inputFile))
            {
                loader.Load(fs);

                if (loader.LoadErrors.Any())
                {
                    foreach (var error in loader.LoadErrors)
                    {
                        switch (error.Category)
                        {
                            case LoadErrorCategory.Error:
                                Log.LogError(error.Message);
                                break;
                        }
                    }

                    if (loader.LoadErrors.Any(x => x.Category == LoadErrorCategory.Error))
                        Environment.Exit(1);
                }

                var description = loader.GetDescriptions()[0];

                var outputs = Generate(fs, description, Path.GetFileNameWithoutExtension(inputFile), resourceProvider, formats, previewOptions);

                return new CompileResult(description.Metadata.Author,
                                         description.ComponentName,
                                         description.Metadata.GUID,
                                         true,
                                         description.Metadata.AdditionalInformation,
                                         inputFile,
                                         description.Metadata.Entries.ToImmutableDictionary(),
                                         outputs.ToImmutableDictionary());
            }
        }

        internal static IEnumerable<KeyValuePair<string, string>> Generate(FileStream input,
                                                                           ComponentDescription description,
                                                                           string inputBaseName,
                                                                           IResourceProvider resourceProvider,
                                                                           IDictionary<IOutputGenerator, string> formats,
                                                                           PreviewGenerationOptions previewOptions)
        {
            foreach (var f in formats)
            {
                string format = f.Key.FileExtension.Substring(1);
                string autoGeneratedName = $"{inputBaseName}{f.Key.FileExtension}";
                string outputPath = f.Value != null && Directory.Exists(f.Value) ? Path.Combine(f.Value, autoGeneratedName) : f.Value ?? autoGeneratedName;
                using (var output = File.Open(outputPath, FileMode.Create))
                {
                    Log.LogDebug($"Starting {format} generation.");
                    input.Seek(0, SeekOrigin.Begin);
                    f.Key.Generate(description,
                                   resourceProvider,
                                   previewOptions,
                                   input,
                                   output);
                    Log.LogInformation($"  {format,-4} -> {outputPath}");
                }

                yield return new KeyValuePair<string, string>(format, outputPath);
            }
        }

        private static string NullIfEmpty(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? null : input;
        }
    }
}
